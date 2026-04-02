package org.openhab.core.config.discovery.addon.mdns;
import javax.jmdns.ServiceEvent;
import javax.jmdns.ServiceInfo;
import javax.jmdns.ServiceListener;
import org.openhab.core.io.transport.mdns.MDNSClient;
 * This is a {@link MDNSAddonFinder} for finding suggested add-ons via mDNS. This finder requires a
 * {@code mdnsServiceType} parameter to be present in the add-on info discovery method.
 * @author Mark Herwege - refactor to allow uninstall
 * @author Mark Herwege - change to discovery method schema
@Component(service = AddonFinder.class, name = MDNSAddonFinder.SERVICE_NAME)
public class MDNSAddonFinder extends BaseAddonFinder implements ServiceListener {
    public static final String SERVICE_TYPE = SERVICE_TYPE_MDNS;
    public static final String SERVICE_NAME = SERVICE_NAME_MDNS;
    public static final String MDNS_SERVICE_TYPE = "mdnsServiceType";
    private static final String NAME = "name";
    private static final String APPLICATION = "application";
    private final Logger logger = LoggerFactory.getLogger(MDNSAddonFinder.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool(SERVICE_NAME);
    private final Map<String, ServiceInfo> services = new ConcurrentHashMap<>();
    private MDNSClient mdnsClient;
    public MDNSAddonFinder(@Reference MDNSClient mdnsClient) {
        this.mdnsClient = mdnsClient;
     * Adds the given mDNS service to the set of discovered services.
     * @param service the mDNS service to be added.
     * @param isResolved indicates if mDNS has fully resolved the service information.
    public void addService(ServiceInfo service, boolean isResolved) {
        String qualifiedName = service.getQualifiedName();
        if (isResolved || !services.containsKey(qualifiedName)) {
            if (services.put(qualifiedName, service) == null) {
                logger.trace("Added service: {}", qualifiedName);
        services.clear();
        unsetAddonCandidates();
        // Remove listeners for all service types that are no longer in candidates
        addonCandidates.stream().filter(c -> !candidates.contains(c))
                .forEach(c -> c.getDiscoveryMethods().stream().filter(m -> SERVICE_TYPE.equals(m.getServiceType()))
                        .filter(m -> !getMdnsServiceType(m).isEmpty())
                        .forEach(m -> mdnsClient.removeServiceListener(getMdnsServiceType(m), this)));
        // Add listeners for all service types in candidates
        addonCandidates
                        .filter(m -> !getMdnsServiceType(m).isEmpty()).forEach(m -> {
                            String serviceType = getMdnsServiceType(m);
                            mdnsClient.addServiceListener(serviceType, this);
                            scheduler.submit(() -> mdnsClient.list(serviceType));
    public void unsetAddonCandidates() {
        addonCandidates.forEach(c -> c.getDiscoveryMethods().stream()
                .filter(m -> SERVICE_TYPE.equals(m.getServiceType())).filter(m -> !getMdnsServiceType(m).isEmpty())
        super.unsetAddonCandidates();
        Set<AddonInfo> result = new HashSet<>();
                Map<String, Pattern> matchProperties = method.getMatchProperties().stream()
                        .collect(Collectors.toMap(AddonMatchProperty::getName, AddonMatchProperty::getPattern));
                Set<String> matchPropertyKeys = matchProperties.keySet().stream()
                        .filter(property -> (!NAME.equals(property) && !APPLICATION.equals(property)))
                        .collect(Collectors.toSet());
                for (ServiceInfo service : services.values()) {
                    logger.trace("Checking service: {}/{}", service.getQualifiedName(), service.getNiceTextString());
                    if (getMdnsServiceType(method).equals(service.getType())
                            && propertyMatches(matchProperties, NAME, service.getName())
                            && propertyMatches(matchProperties, APPLICATION, service.getApplication())
                            && matchPropertyKeys.stream().allMatch(
                                    name -> propertyMatches(matchProperties, name, service.getPropertyString(name)))) {
                        result.add(candidate);
    private String getMdnsServiceType(AddonDiscoveryMethod method) {
        String param = method.getParameters().stream().filter(p -> MDNS_SERVICE_TYPE.equals(p.getName()))
                .map(AddonParameter::getValue).findFirst().orElse("");
        return param == null ? "" : param;
     * ************ MDNSClient call-back methods ************
    public void serviceAdded(@Nullable ServiceEvent event) {
        if (event != null) {
            ServiceInfo service = event.getInfo();
            if (service != null) {
                addService(service, false);
    public void serviceRemoved(@Nullable ServiceEvent event) {
    public void serviceResolved(@Nullable ServiceEvent event) {
                addService(service, true);
