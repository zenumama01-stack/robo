package org.openhab.core.config.discovery.mdns;
import org.openhab.core.config.discovery.DiscoveryResult;
import org.openhab.core.config.discovery.mdns.internal.MDNSDiscoveryService;
 * A {@link MDNSDiscoveryParticipant} that is registered as a service is picked up by the {@link MDNSDiscoveryService}
 * and can thus contribute {@link DiscoveryResult}s from
 * mDNS scans.
 * @author Tobias Bräutigam - Initial contribution
public interface MDNSDiscoveryParticipant {
     * Defines the list of thing types that this participant can identify
     * @return a set of thing type UIDs for which results can be created
    Set<ThingTypeUID> getSupportedThingTypeUIDs();
     * Defines the mDNS service type this participant listens to
     * @return a valid mDNS service type (see: http://www.dns-sd.org/ServiceTypes.html)
    String getServiceType();
     * Creates a discovery result for a mDNS service
     * @param service the mDNS service found on the network
     * @return the according discovery result or <code>null</code>, if device is not
     *         supported by this participant
    DiscoveryResult createResult(ServiceInfo service);
     * Returns the thing UID for a mDNS service
     * @param service the mDNS service on the network
     * @return a thing UID or <code>null</code>, if device is not supported
     *         by this participant
    ThingUID getThingUID(ServiceInfo service);
     * Some openHAB bindings handle devices that can sometimes be a bit late in updating their mDNS announcements, which
     * means that such devices are repeatedly removed from, and (re)added to, the Inbox.
     * To prevent this, a binding that implements an MDNSDiscoveryParticipant may OPTIONALLY implement this method to
     * specify an additional delay period (grace period) to wait before the device is removed from the Inbox.
     * @param serviceInfo the mDNS ServiceInfo describing the device on the network.
     * @return the additional grace period delay in seconds before the device will be removed from the Inbox.
    default long getRemovalGracePeriodSeconds(ServiceInfo serviceInfo) {
