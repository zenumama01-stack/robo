 * A {@link SddpDeviceParticipant} that is registered as a service is picked up by the {@link SddpDiscoveryService} and
 * can thus be informed when the SDDP service discovers or removes an {@link SddpDevice}.
public interface SddpDeviceParticipant {
    void deviceAdded(SddpDevice device);
    void deviceRemoved(SddpDevice device);
