package org.openhab.core.io.transport.mdns;
import javax.jmdns.JmDNS;
 * This interface defines how to get an JmDNS instance
 * to access Bonjour/MDNS
 * @author Tobias Br�utigam - Initial contribution
public interface MDNSClient {
     * This method returns the set of JmDNS instances
     * @return a set of JmDNS instances
    Set<JmDNS> getClientInstances();
     * Listen for services of a given type
     * @param type full qualified service type
     * @param listener listener for service updates
    void addServiceListener(String type, ServiceListener listener);
     * Remove listener for services of a given type
    void removeServiceListener(String type, ServiceListener listener);
     * Register a service
     * @param description service to register, described by (@link ServiceDescription)
    void registerService(ServiceDescription description) throws IOException;
     * Unregister a service. The service should have been registered.
     * @param description service to remove, described by (@link ServiceDescription)
    void unregisterService(ServiceDescription description);
     * Unregister all services
    void unregisterAllServices();
     * Returns a list of service infos of the specified type
     * @param type service type name
     * @return an array of service instances
    ServiceInfo[] list(String type);
     * Returns a list of service infos of the specified type within timeout
     * @param timeout the amount of time it should wait if no service info is found.
    ServiceInfo[] list(String type, Duration timeout);
     * Close properly JmDNS instances
    void close();
