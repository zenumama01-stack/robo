package org.openhab.core.config.discovery.upnp.internal;
import org.jupnp.transport.RouterException;
import org.openhab.core.config.discovery.upnp.UpnpDiscoveryParticipant;
 * This is a {@link DiscoveryService} implementation, which can find UPnP devices in the network.
 * Support for further devices can be added by implementing and registering a {@link UpnpDiscoveryParticipant}.
 * @author Andre Fuechsel - Added call of removeOlderResults
 * @author Gary Tse - Add NetworkAddressChangeListener to handle interface changes
 * @author Tim Roberts - Added primary address change
@Component(immediate = true, service = DiscoveryService.class, configurationPid = "discovery.upnp")
public class UpnpDiscoveryService extends AbstractDiscoveryService
        implements RegistryListener, NetworkAddressChangeListener {
    private final Logger logger = LoggerFactory.getLogger(UpnpDiscoveryService.class);
     * Map of scheduled tasks to remove devices from the Inbox
    private final Map<UDN, Future<?>> deviceRemovalTasks = new ConcurrentHashMap<>();
    private final Set<UpnpDiscoveryParticipant> participants = new CopyOnWriteArraySet<>();
    public UpnpDiscoveryService(final @Nullable Map<String, Object> configProperties, //
            final @Reference UpnpService upnpService, //
    protected void modified(Map<String, Object> configProperties) {
    protected void setNetworkAddressService(NetworkAddressService networkAddressService) {
        networkAddressService.addNetworkAddressChangeListener(this);
    protected void unsetNetworkAddressService(NetworkAddressService networkAddressService) {
    protected void addUpnpDiscoveryParticipant(UpnpDiscoveryParticipant participant) {
        Collection<RemoteDevice> devices = upnpService.getRegistry().getRemoteDevices();
        for (RemoteDevice device : devices) {
            if (!device.isRoot() && !participant.notifyChildDevices()) {
            DiscoveryResult result = participant.createResult(device);
    protected void removeUpnpDiscoveryParticipant(UpnpDiscoveryParticipant participant) {
        for (UpnpDiscoveryParticipant participant : participants) {
        upnpService.getRegistry().addListener(this);
        for (RemoteDevice device : upnpService.getRegistry().getRemoteDevices()) {
            remoteDeviceAdded(upnpService.getRegistry(), device);
            for (RemoteDevice childDevice : device.getEmbeddedDevices()) {
                remoteDeviceAdded(upnpService.getRegistry(), childDevice);
        if (!isBackgroundDiscoveryEnabled()) {
    public void remoteDeviceAdded(Registry registry, RemoteDevice device) {
                    if (participant.getRemovalGracePeriodSeconds(device) > 0) {
                        cancelRemovalTask(device.getIdentity().getUdn());
     * If the device has been scheduled to be removed, cancel its respective removal task
    private void cancelRemovalTask(UDN udn) {
        Future<?> deviceRemovalTask = deviceRemovalTasks.remove(udn);
    public void remoteDeviceRemoved(Registry registry, RemoteDevice device) {
                ThingUID thingUID = participant.getThingUID(device);
                    long gracePeriod = participant.getRemovalGracePeriodSeconds(device);
                        UDN udn = device.getIdentity().getUdn();
                        cancelRemovalTask(udn);
                        deviceRemovalTasks.put(udn, scheduler.schedule(() -> {
    public void onChanged(final List<CidrAddress> added, final List<CidrAddress> removed) {
            if (!removed.isEmpty()) {
                upnpService.getRegistry().removeAllRemoteDevices();
                upnpService.getRouter().disable();
                upnpService.getRouter().enable();
            } catch (RouterException e) {
                logger.error("Could not restart UPnP network components.", e);
    public void remoteDeviceUpdated(Registry registry, RemoteDevice device) {
    public void localDeviceAdded(Registry registry, LocalDevice device) {
    public void localDeviceRemoved(Registry registry, LocalDevice device) {
    public void beforeShutdown(Registry registry) {
    public void remoteDeviceDiscoveryStarted(Registry registry, RemoteDevice device) {
    public void remoteDeviceDiscoveryFailed(Registry registry, RemoteDevice device, Exception ex) {
