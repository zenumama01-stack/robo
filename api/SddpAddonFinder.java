package org.openhab.core.config.discovery.addon.sddp;
import org.openhab.core.config.discovery.DiscoveryService;
import org.openhab.core.config.discovery.sddp.SddpDevice;
import org.openhab.core.config.discovery.sddp.SddpDeviceParticipant;
import org.openhab.core.config.discovery.sddp.SddpDiscoveryService;
 * This is a {@link SddpAddonFinder} for finding suggested Addons via SDDP.
 * Simple Device Discovery Protocol (SDDP) is a simple multicast discovery protocol implemented
 * by many "smart home" devices to allow a controlling agent to easily discover and connect to
 * devices on a local subnet.
 * SDDP was created by Control4, and is quite similar to UPnP's standard Simple Service Discovery
 * Protocol (SSDP), and it serves a virtually identical purpose. SDDP is not a standard protocol
 * and it is not publicly documented.
 * It checks the binding's addon.xml 'match-property' elements for the following SDDP properties:
 * <li>driver</li>
 * <li>host</li>
 * <li>ipAddress</li>
 * <li>macAddress</li>
 * <li>manufacturer</li>
 * <li>model</li>
 * <li>port</li>
 * <li>primaryProxy</li>
 * <li>proxies</li>
 * <li>type</li>
@Component(service = AddonFinder.class, name = SddpAddonFinder.SERVICE_NAME)
public class SddpAddonFinder extends BaseAddonFinder implements SddpDeviceParticipant {
    public static final String SERVICE_TYPE = AddonFinderConstants.SERVICE_TYPE_SDDP;
    public static final String SERVICE_NAME = AddonFinderConstants.SERVICE_NAME_SDDP;
    private static final String DRIVER = "driver";
    private static final String HOST = "host";
    private static final String IP_ADDRESS = "ipAddress";
    private static final String MAC_ADDRESS = "macAddress";
    private static final String MANUFACTURER = "manufacturer";
    private static final String MODEL = "model";
    private static final String PORT = "port";
    private static final String PRIMARY_PROXY = "primaryProxy";
    private static final String PROXIES = "proxies";
    private static final String TYPE = "type";
    private static final Set<String> SUPPORTED_PROPERTIES = Set.of(DRIVER, HOST, IP_ADDRESS, MAC_ADDRESS, MANUFACTURER,
            MODEL, PORT, PRIMARY_PROXY, PROXIES, TYPE);
    private final Logger logger = LoggerFactory.getLogger(SddpAddonFinder.class);
    private final Set<SddpDevice> foundDevices = new HashSet<>();
    private @Nullable SddpDiscoveryService sddpDiscoveryService = null;
    public SddpAddonFinder(
            @Reference(service = DiscoveryService.class, target = "(protocol=sddp)") DiscoveryService discoveryService) {
        if (discoveryService instanceof SddpDiscoveryService sddpDiscoveryService) {
            sddpDiscoveryService.addSddpDeviceParticipant(this);
            this.sddpDiscoveryService = sddpDiscoveryService;
            logger.warn("SddpAddonFinder() DiscoveryService is not an SddpDiscoveryService");
        SddpDiscoveryService sddpDiscoveryService = this.sddpDiscoveryService;
        if (sddpDiscoveryService != null) {
            sddpDiscoveryService.removeSddpDeviceParticipant(this);
            this.sddpDiscoveryService = null;
        foundDevices.clear();
    public void deviceAdded(SddpDevice device) {
        foundDevices.add(device);
    public void deviceRemoved(SddpDevice device) {
        foundDevices.remove(device);
                propertyNames.removeAll(SUPPORTED_PROPERTIES);
                    logger.warn("Add-on '{}' addon.xml file contains unsupported 'match-property' [{}]",
                            candidate.getUID(), String.join(",", propertyNames));
                for (SddpDevice device : foundDevices) {
                    logger.trace("Checking device: {}", device.host);
                    if (propertyMatches(matchProperties, HOST, device.host)
                            && propertyMatches(matchProperties, IP_ADDRESS, device.ipAddress)
                            && propertyMatches(matchProperties, MAC_ADDRESS, device.macAddress)
                            && propertyMatches(matchProperties, MANUFACTURER, device.manufacturer)
                            && propertyMatches(matchProperties, MODEL, device.model)
                            && propertyMatches(matchProperties, PORT, device.port)
                            && propertyMatches(matchProperties, PRIMARY_PROXY, device.primaryProxy)
                            && propertyMatches(matchProperties, PROXIES, device.proxies)
                            && propertyMatches(matchProperties, TYPE, device.type)) {
