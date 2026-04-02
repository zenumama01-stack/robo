package org.openhab.core.config.discovery.upnp;
 * A {@link UpnpDiscoveryParticipant} that is registered as a service is picked up by the UpnpDiscoveryService
 * UPnP scans.
public interface UpnpDiscoveryParticipant {
     * According to the UPnP specification, the minimum MaxAge is 1800 seconds.
    long MIN_MAX_AGE_SECS = 1800;
     * Creates a discovery result for a upnp device
     * @param device the upnp device found on the network
    DiscoveryResult createResult(RemoteDevice device);
     * Returns the thing UID for a upnp device
     * @param device the upnp device on the network
    ThingUID getThingUID(RemoteDevice device);
     * The JUPnP library strictly follows the UPnP specification insofar as if a device fails to send its next
     * 'ssdp:alive' notification within its declared 'maxAge' period, it is immediately considered to be gone. But
     * unfortunately some openHAB bindings handle devices that can sometimes be a bit late in sending their 'ssdp:alive'
     * notifications even though they have not really gone offline, which means that such devices are repeatedly removed
     * from, and (re)added to, the Inbox.
     * To prevent this, a binding that implements a UpnpDiscoveryParticipant may OPTIONALLY implement this method to
     * @param device the UPnP device on the network
     * @return the additional grace period delay in seconds before the device will be removed from the Inbox
    default long getRemovalGracePeriodSeconds(RemoteDevice device) {
     * The discovery always notifies participants about discovered root devices. And if the participant also
     * wants to be notified about embedded child devices then it shall override this method.
     * @return true if the participant wants to be also notified about embedded child devices.
    default boolean notifyChildDevices() {
