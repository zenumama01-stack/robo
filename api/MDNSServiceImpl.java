 * This class starts the JmDNS and implements interface to register and
 * unregister services.
public class MDNSServiceImpl implements MDNSService {
    private final Logger logger = LoggerFactory.getLogger(MDNSServiceImpl.class);
    private @Nullable MDNSClient mdnsClient;
    private final Set<ServiceDescription> servicesToRegisterQueue = new CopyOnWriteArraySet<>();
    protected void setMDNSClient(MDNSClient client) {
        this.mdnsClient = client;
        // register queued services
        if (!servicesToRegisterQueue.isEmpty()) {
            Executors.newSingleThreadExecutor().execute(() -> {
                logger.debug("Registering {} queued services", servicesToRegisterQueue.size());
                for (ServiceDescription description : servicesToRegisterQueue) {
                        MDNSClient localClient = mdnsClient;
                        if (localClient != null) {
                            localClient.registerService(description);
                        logger.error("{}", e.getMessage());
                        logger.debug("Not registering service {}, because service is already deactivated!",
                                description.serviceType);
                servicesToRegisterQueue.clear();
    protected void unsetMDNSClient(MDNSClient mdnsClient) {
        this.mdnsClient = null;
        mdnsClient.unregisterAllServices();
    public void registerService(final ServiceDescription description) {
        if (localClient == null) {
            // queue the service to register it as soon as the mDNS client is available
            servicesToRegisterQueue.add(description);
        if (mdnsClient != null) {
            mdnsClient.unregisterService(description);
     * This method unregisters all services from Bonjour/MDNS
    protected void unregisterAllServices() {
        unregisterAllServices();
            mdnsClient.close();
            logger.debug("mDNS service has been stopped");
