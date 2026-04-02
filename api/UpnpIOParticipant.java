package org.openhab.core.io.transport.upnp;
 * The {@link UpnpIOParticipant} is an interface that needs to
 * be implemented by classes that wants to participate in
 * UPNP communication
public interface UpnpIOParticipant {
    /** Get the UDN of the participant **/
    String getUDN();
    /** Called when the UPNP IO service receives a {variable,value} tuple for the given UPNP service **/
    void onValueReceived(String variable, String value, String service);
     * Called to notify if a GENA subscription succeeded or failed.
     * @param service the UPnP service subscribed
     * @param succeeded true if the subscription succeeded; false if failed
    void onServiceSubscribed(String service, boolean succeeded);
     * Called when the UPNP IO service is unable to poll the UDN of the participant, given that
     * an addStatusListener is registered.
     * @param status false, if the poll fails when the polling was previously successful; true if the poll succeeds
     *            when the polling was previously failing
    void onStatusChanged(boolean status);
