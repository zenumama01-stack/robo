package org.openhab.core.config.discovery.mdns.internal;
import org.openhab.core.config.discovery.AbstractDiscoveryService;
import org.openhab.core.config.discovery.mdns.MDNSDiscoveryParticipant;
 * This is a {@link DiscoveryService} implementation, which can find mDNS services in the network. Support for further
 * devices can be added by implementing and registering a {@link MDNSDiscoveryParticipant}.
 * @author Kai Kreuzer - Improved startup behavior and background discovery
 * @author Andre Fuechsel - make {@link #startScan()} asynchronous
@Component(immediate = true, service = DiscoveryService.class, configurationPid = "discovery.mdns")
public class MDNSDiscoveryService extends AbstractDiscoveryService implements ServiceListener {
    private static final Duration FOREGROUND_SCAN_TIMEOUT = Duration.ofMillis(200);
    private final Logger logger = LoggerFactory.getLogger(MDNSDiscoveryService.class);
    private final Set<MDNSDiscoveryParticipant> participants = new CopyOnWriteArraySet<>();
    private final MDNSClient mdnsClient;
     * Map of scheduled tasks to remove devices from the Inbox.
    private Map<String, ScheduledFuture<?>> deviceRemovalTasks = new ConcurrentHashMap<>();
    public MDNSDiscoveryService(final @Nullable Map<String, Object> configProperties, //
            final @Reference MDNSClient mdnsClient, //
        super(5);
        super.activate(configProperties);
        if (isBackgroundDiscoveryEnabled()) {
            for (MDNSDiscoveryParticipant participant : participants) {
                mdnsClient.addServiceListener(participant.getServiceType(), this);
            mdnsClient.removeServiceListener(participant.getServiceType(), this);
    protected void modified(@Nullable Map<String, Object> configProperties) {
        super.modified(configProperties);
    protected void startBackgroundDiscovery() {
        startScan(true);
    protected void stopBackgroundDiscovery() {
    protected void startScan() {
        startScan(false);
    protected synchronized void stopScan() {
        removeOlderResults(getTimestampOfLastScan());
        super.stopScan();
    private void startScan(boolean isBackground) {
            scan(isBackground);
     * Scan has 2 different behaviors. background/ foreground. Background scans can
     * have much higher timeout. Foreground scans have only a short timeout as human
     * users may become impatient. The underlying reason is that the jmDNS
     * implementation {@code MDNSClient#list(String)} has a default timeout of 6
     * seconds when no ServiceInfo is found. When there are many participants,
     * waiting 6 seconds for each non-existent type is too long.
     * @param isBackground true if it is background scan, false otherwise.
    private void scan(boolean isBackground) {
            long start = System.currentTimeMillis();
            ServiceInfo[] services;
            if (isBackground) {
                services = mdnsClient.list(participant.getServiceType());
                services = mdnsClient.list(participant.getServiceType(), FOREGROUND_SCAN_TIMEOUT);
            logger.debug("{} services found for {}; duration: {}ms", services.length, participant.getServiceType(),
                    System.currentTimeMillis() - start);
            for (ServiceInfo serviceInfo : services) {
                createDiscoveryResult(participant, serviceInfo);
    protected void addMDNSDiscoveryParticipant(MDNSDiscoveryParticipant participant) {
        participants.add(participant);
    protected void removeMDNSDiscoveryParticipant(MDNSDiscoveryParticipant participant) {
        participants.remove(participant);
    public Set<ThingTypeUID> getSupportedThingTypes() {
        Set<ThingTypeUID> supportedThingTypes = new HashSet<>();
            supportedThingTypes.addAll(participant.getSupportedThingTypeUIDs());
        return supportedThingTypes;
    public void serviceAdded(@NonNullByDefault({}) ServiceEvent serviceEvent) {
         * Do nothing when a service is added, as we will get a <code>serviceResolved</code> event afterwards,
         * which contains the fully resolved ServiceInfo. If we would already create a DiscoveryResult here,
         * we would not have the necessary full information.
    public void serviceRemoved(@NonNullByDefault({}) ServiceEvent serviceEvent) {
        // note: {@link ServiceEvent} JavaDoc says getInfo() result can be null; but seems never to be so here.
            if (participant.getServiceType().equals(serviceEvent.getType())) {
                removeDiscoveryResult(participant, serviceEvent.getInfo());
    public void serviceResolved(@NonNullByDefault({}) ServiceEvent serviceEvent) {
        considerService(serviceEvent);
    private void considerService(ServiceEvent serviceEvent) {
                    createDiscoveryResult(participant, serviceEvent.getInfo());
    private void createDiscoveryResult(MDNSDiscoveryParticipant participant, ServiceInfo serviceInfo) {
            DiscoveryResult result = participant.createResult(serviceInfo);
                cancelRemovalTask(serviceInfo);
                thingDiscovered(result, FrameworkUtil.getBundle(participant.getClass()));
            logger.error("Participant '{}' threw an exception", participant.getClass().getName(), e);
    private void removeDiscoveryResult(MDNSDiscoveryParticipant participant, ServiceInfo serviceInfo) {
            ThingUID thingUID = participant.getThingUID(serviceInfo);
            if (thingUID != null) {
                long gracePeriod = participant.getRemovalGracePeriodSeconds(serviceInfo);
                if (gracePeriod <= 0) {
                    thingRemoved(thingUID);
                    scheduleRemovalTask(thingUID, serviceInfo, gracePeriod);
     * If the device has been scheduled to be removed, cancel its respective removal task.
    private void cancelRemovalTask(ServiceInfo serviceInfo) {
        ScheduledFuture<?> deviceRemovalTask = deviceRemovalTasks.remove(serviceInfo.getQualifiedName());
        if (deviceRemovalTask != null) {
            deviceRemovalTask.cancel(false);
     * Schedule a task that will remove the device from the Inbox after the given grace period has expired.
     * @param thingUID the UID of the Thing to be removed.
     * @param gracePeriod the scheduled delay in seconds.
    private void scheduleRemovalTask(ThingUID thingUID, ServiceInfo serviceInfo, long gracePeriod) {
        deviceRemovalTasks.put(serviceInfo.getQualifiedName(), scheduler.schedule(() -> {
        }, gracePeriod, TimeUnit.SECONDS));
