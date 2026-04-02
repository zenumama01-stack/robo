import org.openhab.core.config.discovery.DiscoveryListener;
import org.openhab.core.config.discovery.DiscoveryServiceRegistry;
import org.openhab.core.config.discovery.ScanListener;
 * The {@link DiscoveryServiceRegistryImpl} is a concrete implementation of the {@link DiscoveryServiceRegistry}.
 * This implementation tracks any existing {@link DiscoveryService} and registers itself as {@link DiscoveryListener} on
 * it.
 * This implementation does neither handle memory leaks (orphaned listener instances) nor blocked listeners. No
 * performance optimizations have been done (synchronization).
@Component(immediate = true, service = DiscoveryServiceRegistry.class)
public final class DiscoveryServiceRegistryImpl implements DiscoveryServiceRegistry, DiscoveryListener {
    private final Map<DiscoveryService, Set<DiscoveryResult>> cachedResults = new HashMap<>();
    private final class AggregatingScanListener implements ScanListener {
        private final @Nullable ScanListener listener;
        private int finishedDiscoveryServices = 0;
        private boolean errorOccurred = false;
        private int numberOfDiscoveryServices;
        private AggregatingScanListener(int numberOfDiscoveryServices, @Nullable ScanListener listener) {
            this.numberOfDiscoveryServices = numberOfDiscoveryServices;
            this.listener = listener;
        public void onFinished() {
            ScanListener listener = null;
                finishedDiscoveryServices++;
                logger.debug("Finished {} of {} discovery services.", finishedDiscoveryServices,
                        numberOfDiscoveryServices);
                if (!errorOccurred && finishedDiscoveryServices == numberOfDiscoveryServices) {
                    listener = this.listener;
            if (listener != null) {
                listener.onFinished();
        public void onErrorOccurred(@Nullable Exception exception) {
                if (!errorOccurred) {
                    // Skip error logging for aborted scans
                    if (!(exception instanceof CancellationException)) {
                        if (exception != null) {
                            logger.warn("Error occurred while executing discovery service: {}", exception.getMessage(),
                listener.onErrorOccurred(exception);
        public void reduceNumberOfDiscoveryServices() {
                numberOfDiscoveryServices--;
    private final Set<DiscoveryService> discoveryServices = new CopyOnWriteArraySet<>();
    private final Set<DiscoveryService> discoveryServicesAll = new CopyOnWriteArraySet<>();
    private final Set<DiscoveryListener> listeners = new CopyOnWriteArraySet<>();
    private final AtomicBoolean active = new AtomicBoolean();
    private final Logger logger = LoggerFactory.getLogger(DiscoveryServiceRegistryImpl.class);
    protected void activate() {
        active.set(true);
        for (final DiscoveryService discoveryService : discoveryServicesAll) {
            addDiscoveryServiceActivated(discoveryService);
        active.set(false);
            removeDiscoveryServiceActivated(discoveryService);
            cachedResults.clear();
    public boolean abortScan(ThingTypeUID thingTypeUID) throws IllegalStateException {
        Set<DiscoveryService> discoveryServicesForThingType = getDiscoveryServices(thingTypeUID);
        if (discoveryServicesForThingType.isEmpty()) {
            logger.warn("No discovery service for thing type '{}' found!", thingTypeUID);
        return abortScans(discoveryServicesForThingType);
    public boolean abortScan(String bindingId) throws IllegalStateException {
        Set<DiscoveryService> discoveryServicesForBinding = getDiscoveryServices(bindingId);
        if (discoveryServicesForBinding.isEmpty()) {
            logger.warn("No discovery service for binding '{}' found!", bindingId);
        return abortScans(discoveryServicesForBinding);
    public void addDiscoveryListener(DiscoveryListener listener) throws IllegalStateException {
        Map<DiscoveryService, Set<DiscoveryResult>> existingResults;
        existingResults.forEach((service, results) -> {
            results.forEach(result -> listener.thingDiscovered(service, result));
    public boolean startScan(ThingTypeUID thingTypeUID, @Nullable String input, @Nullable ScanListener listener)
            throws IllegalStateException {
        return startScans(discoveryServicesForThingType, input, listener);
    public boolean startScan(String bindingId, @Nullable String input, @Nullable ScanListener listener)
        final Set<DiscoveryService> discoveryServicesForBinding = getDiscoveryServices(bindingId);
            logger.warn("No discovery service for binding id '{}' found!", bindingId);
        return startScans(discoveryServicesForBinding, input, listener);
    public boolean supportsDiscovery(ThingTypeUID thingTypeUID) {
        return !getDiscoveryServices(thingTypeUID).isEmpty();
    public boolean supportsDiscovery(String bindingId) {
        return !getDiscoveryServices(bindingId).isEmpty();
    public List<ThingTypeUID> getSupportedThingTypes() {
        List<ThingTypeUID> thingTypeUIDs = new ArrayList<>();
        for (DiscoveryService discoveryService : discoveryServices) {
            thingTypeUIDs.addAll(discoveryService.getSupportedThingTypes());
        return thingTypeUIDs;
    public List<String> getSupportedBindings() {
        List<String> bindings = new ArrayList<>();
            Collection<ThingTypeUID> supportedThingTypes = discoveryService.getSupportedThingTypes();
            for (ThingTypeUID thingTypeUID : supportedThingTypes) {
                bindings.add(thingTypeUID.getBindingId());
    public void removeDiscoveryListener(DiscoveryListener listener) throws IllegalStateException {
    public void thingDiscovered(final DiscoveryService source, final DiscoveryResult result) {
            Objects.requireNonNull(cachedResults.computeIfAbsent(source, unused -> new HashSet<>())).add(result);
        for (final DiscoveryListener listener : listeners) {
                listener.thingDiscovered(source, result);
                logger.error("Cannot notify the DiscoveryListener '{}' on Thing discovered event!",
                        listener.getClass().getName(), ex);
    public void thingRemoved(final DiscoveryService source, final ThingUID thingUID) {
            Iterator<DiscoveryResult> it = cachedResults.getOrDefault(source, Set.of()).iterator();
                if (it.next().getThingUID().equals(thingUID)) {
                listener.thingRemoved(source, thingUID);
                logger.error("Cannot notify the DiscoveryListener '{}' on Thing removed event!",
    public @Nullable Collection<ThingUID> removeOlderResults(final DiscoveryService source, final Instant timestamp,
            final @Nullable Collection<ThingTypeUID> thingTypeUIDs, @Nullable ThingUID bridgeUID) {
        Set<ThingUID> removedResults = new HashSet<>();
                Collection<ThingUID> olderResults = listener.removeOlderResults(source, timestamp, thingTypeUIDs,
                        bridgeUID);
                if (olderResults != null) {
                    removedResults.addAll(olderResults);
                logger.error("Cannot notify the DiscoveryListener '{}' on all things removed event!",
        return removedResults;
    private boolean abortScans(Set<DiscoveryService> discoveryServices) {
        boolean allServicesAborted = true;
                logger.debug("Abort scan for thing types '{}' on '{}'...", supportedThingTypes,
                        discoveryService.getClass().getName());
                discoveryService.abortScan();
                logger.debug("Scan for thing types '{}' aborted on '{}'.", supportedThingTypes,
                logger.error("Cannot abort scan for thing types '{}' on '{}'!", supportedThingTypes,
                        discoveryService.getClass().getName(), ex);
                allServicesAborted = false;
        return allServicesAborted;
    private boolean startScans(Set<DiscoveryService> discoveryServices, @Nullable String input,
            @Nullable ScanListener listener) {
        boolean atLeastOneDiscoveryServiceHasBeenStarted = false;
        if (discoveryServices.size() > 1) {
            logger.debug("Trying to start {} scans with an aggregating listener.", discoveryServices.size());
            AggregatingScanListener aggregatingScanListener = new AggregatingScanListener(discoveryServices.size(),
                    listener);
                if (startScan(discoveryService, input, aggregatingScanListener)) {
                    atLeastOneDiscoveryServiceHasBeenStarted = true;
                            "Reducing number of discovery services in aggregating listener, because discovery service failed to start scan.");
                    aggregatingScanListener.reduceNumberOfDiscoveryServices();
            if (startScan(discoveryServices.iterator().next(), input, listener)) {
        return atLeastOneDiscoveryServiceHasBeenStarted;
    private boolean startScan(DiscoveryService discoveryService, @Nullable String input,
            logger.debug("Triggering scan for thing types '{}' on '{}'...", supportedThingTypes,
                    discoveryService.getClass().getSimpleName());
            if (discoveryService.isScanInputSupported() && input != null) {
                discoveryService.startScan(input, listener);
                discoveryService.startScan(listener);
            logger.error("Cannot trigger scan for thing types '{}' on '{}'!", supportedThingTypes,
                    discoveryService.getClass().getSimpleName(), ex);
    private Set<DiscoveryService> getDiscoveryServices(ThingTypeUID thingTypeUID) throws IllegalStateException {
        Set<DiscoveryService> discoveryServices = new HashSet<>();
        for (DiscoveryService discoveryService : this.discoveryServices) {
            Collection<ThingTypeUID> discoveryThingTypes = discoveryService.getSupportedThingTypes();
            if (discoveryThingTypes.contains(thingTypeUID)) {
                discoveryServices.add(discoveryService);
        return discoveryServices;
    public Set<DiscoveryService> getDiscoveryServices(String bindingId) throws IllegalStateException {
            for (ThingTypeUID thingTypeUID : discoveryThingTypes) {
                if (thingTypeUID.getBindingId().equals(bindingId)) {
    protected void addDiscoveryService(final DiscoveryService discoveryService) {
        discoveryServicesAll.add(discoveryService);
        if (active.get()) {
    private void addDiscoveryServiceActivated(final DiscoveryService discoveryService) {
        discoveryService.addDiscoveryListener(this);
    protected void removeDiscoveryService(DiscoveryService discoveryService) {
        discoveryServicesAll.remove(discoveryService);
    private void removeDiscoveryServiceActivated(DiscoveryService discoveryService) {
        discoveryServices.remove(discoveryService);
        discoveryService.removeDiscoveryListener(this);
            cachedResults.remove(discoveryService);
    private int getMaxScanTimeout(Set<DiscoveryService> discoveryServices) {
            if (discoveryService.getScanTimeout() > result) {
                result = discoveryService.getScanTimeout();
    public int getMaxScanTimeout(ThingTypeUID thingTypeUID) {
        return getMaxScanTimeout(getDiscoveryServices(thingTypeUID));
    public int getMaxScanTimeout(String bindingId) {
        return getMaxScanTimeout(getDiscoveryServices(bindingId));
