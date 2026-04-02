 * A {@link SddpDiscoveryParticipant} that is registered as a service is picked up by the {@link SddpDiscoveryService}
 * and can thus contribute {@link DiscoveryResult}s from SDDP scans.
public interface SddpDiscoveryParticipant {
     * Creates a discovery result for a SDDP device
     * @param device the SDDP device found on the network
    DiscoveryResult createResult(SddpDevice device);
     * Returns the thing UID for a SDDP device
     * @param device the SDDP device on the network
    ThingUID getThingUID(SddpDevice device);
