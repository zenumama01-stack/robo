package org.openhab.core.io.rest.mdns.internal;
import org.openhab.core.io.transport.mdns.MDNSService;
import org.openhab.core.io.transport.mdns.ServiceDescription;
import org.openhab.core.net.HttpServiceUtil;
 * This class announces the REST API through mDNS for clients to automatically
 * discover it.
 * @author Markus Rathgeb - Use HTTP service utility functions
@Component(immediate = true, configurationPid = "org.openhab.mdns", property = {
        Constants.SERVICE_PID + "=org.openhab.mdns" //
public class MDNSAnnouncer {
    private int httpSSLPort;
    private int httpPort;
    private String mdnsName;
    private MDNSService mdnsService;
    public void setMDNSService(MDNSService mdnsService) {
        this.mdnsService = mdnsService;
    public void unsetMDNSService(MDNSService mdnsService) {
        this.mdnsService = null;
    public void activate(BundleContext bundleContext, Map<String, Object> properties) {
        if (!"false".equalsIgnoreCase((String) properties.get("enabled"))) {
            if (mdnsService != null) {
                mdnsName = bundleContext.getProperty("mdnsName");
                if (mdnsName == null) {
                    mdnsName = "openhab";
                    httpPort = HttpServiceUtil.getHttpServicePort(bundleContext);
                    if (httpPort != -1) {
                        mdnsService.registerService(getDefaultServiceDescription());
                    httpSSLPort = HttpServiceUtil.getHttpServicePortSecure(bundleContext);
                    if (httpSSLPort != -1) {
                        mdnsService.registerService(getSSLServiceDescription());
            mdnsService.unregisterService(getDefaultServiceDescription());
            mdnsService.unregisterService(getSSLServiceDescription());
    private ServiceDescription getDefaultServiceDescription() {
        Hashtable<String, String> serviceProperties = new Hashtable<>();
        serviceProperties.put("uri", RESTConstants.REST_URI);
        return new ServiceDescription("_" + mdnsName + "-server._tcp.local.", mdnsName, httpPort, serviceProperties);
    private ServiceDescription getSSLServiceDescription() {
        ServiceDescription description = getDefaultServiceDescription();
        description.serviceType = "_" + mdnsName + "-server-ssl._tcp.local.";
        description.serviceName = mdnsName + "-ssl";
        description.servicePort = httpSSLPort;
