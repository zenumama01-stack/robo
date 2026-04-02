 * This interface defines how to use JmDNS based service discovery
 * to register and unregister services on Bonjour/MDNS
 * @author Victor Belov - Initial contribution
public interface MDNSService {
     * This method registers a service to be announced through Bonjour/MDNS
     * @param description the {@link ServiceDescription} instance with all details to identify the service
    void registerService(ServiceDescription description);
     * This method unregisters a service not to be announced through Bonjour/MDNS
