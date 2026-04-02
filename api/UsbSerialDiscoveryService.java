package org.openhab.core.config.discovery.usbserial.internal;
import static java.util.stream.Collectors.toSet;
import org.openhab.core.config.discovery.DiscoveryResultBuilder;
import org.openhab.core.config.discovery.usbserial.UsbSerialDiscoveryParticipant;
 * A {@link DiscoveryService} for discovering USB devices with an associated serial port.
 * This discovery service is intended to be used by bindings that support USB devices, but do not directly talk to the
 * USB devices but rather use a serial port for the communication, where the serial port is provided by an operating
 * system driver outside the scope of openHAB. Examples for such USB devices are USB dongles that provide
 * access to wireless networks, like, e.g., Zigbee or Zwave dongles.
 * This discovery service provides functionality for discovering added and removed USB devices and the corresponding
 * serial ports. The actual {@link DiscoveryResult}s are then provided by {@link UsbSerialDiscoveryParticipant}s, which
 * are called by this discovery service whenever new devices are detected or devices are removed. Such
 * {@link UsbSerialDiscoveryParticipant}s should be provided by bindings accessing USB devices via a serial port.
 * This discovery service requires components implementing the interface {@link UsbSerialDiscovery}, which perform the
 * actual serial port and USB device discovery (as this discovery might differ depending on the operating system).
@Component(immediate = true, service = { DiscoveryService.class,
        UsbSerialDiscoveryService.class }, configurationPid = "discovery.usbserial")
public class UsbSerialDiscoveryService extends AbstractDiscoveryService implements UsbSerialDiscoveryListener {
    private final Logger logger = LoggerFactory.getLogger(UsbSerialDiscoveryService.class);
    private static final String THING_PROPERTY_USB_VENDOR_ID = "usb_vendor_id";
    private static final String THING_PROPERTY_USB_PRODUCT_ID = "usb_product_id";
    private final Set<UsbSerialDiscoveryParticipant> discoveryParticipants = new CopyOnWriteArraySet<>();
    private final Set<UsbSerialDeviceInformation> previouslyDiscovered = new CopyOnWriteArraySet<>();
    private final Set<UsbSerialDiscovery> usbSerialDiscoveries = new CopyOnWriteArraySet<>();
    public UsbSerialDiscoveryService() {
    protected void activate(@Nullable Map<String, Object> configProperties) {
    protected void addUsbSerialDiscoveryParticipant(UsbSerialDiscoveryParticipant participant) {
        for (UsbSerialDeviceInformation usbSerialDeviceInformation : previouslyDiscovered) {
            DiscoveryResult result = participant.createResult(usbSerialDeviceInformation);
                thingDiscovered(createDiscoveryResultWithUsbProperties(result, usbSerialDeviceInformation),
                        FrameworkUtil.getBundle(participant.getClass()));
    protected void removeUsbSerialDiscoveryParticipant(UsbSerialDiscoveryParticipant participant) {
        usbSerialDiscoveries.add(usbSerialDiscovery);
            usbSerialDiscovery.startBackgroundScanning();
        usbSerialDiscovery.stopBackgroundScanning();
        usbSerialDiscoveries.remove(usbSerialDiscovery);
        previouslyDiscovered.clear();
        return discoveryParticipants.stream().flatMap(participant -> participant.getSupportedThingTypeUIDs().stream())
                .collect(toSet());
        usbSerialDiscoveries.forEach(UsbSerialDiscovery::doSingleScan);
        usbSerialDiscoveries.forEach(UsbSerialDiscovery::startBackgroundScanning);
        usbSerialDiscoveries.forEach(UsbSerialDiscovery::stopBackgroundScanning);
    public void usbSerialDeviceDiscovered(UsbSerialDeviceInformation usbSerialDeviceInformation) {
        if (previouslyDiscovered.add(usbSerialDeviceInformation)) {
            logger.debug("Discovered new USB-Serial device: {}", usbSerialDeviceInformation);
        for (UsbSerialDiscoveryParticipant participant : discoveryParticipants) {
                thingDiscovered(createDiscoveryResultWithUsbProperties(result, usbSerialDeviceInformation));
    public void usbSerialDeviceRemoved(UsbSerialDeviceInformation usbSerialDeviceInformation) {
        logger.debug("Discovered removal of USB-Serial device: {}", usbSerialDeviceInformation);
        previouslyDiscovered.remove(usbSerialDeviceInformation);
            ThingUID thingUID = participant.getThingUID(usbSerialDeviceInformation);
    private DiscoveryResult createDiscoveryResultWithUsbProperties(DiscoveryResult result,
            UsbSerialDeviceInformation usbSerialDeviceInformation) {
        Map<String, Object> resultProperties = new HashMap<>(result.getProperties());
        resultProperties.put(THING_PROPERTY_USB_VENDOR_ID, usbSerialDeviceInformation.getVendorId());
        resultProperties.put(THING_PROPERTY_USB_PRODUCT_ID, usbSerialDeviceInformation.getProductId());
        return DiscoveryResultBuilder.create(result.getThingUID()).withProperties(resultProperties)
                .withBridge(result.getBridgeUID()).withTTL(result.getTimeToLive()).withLabel(result.getLabel())
                .withRepresentationProperty(result.getRepresentationProperty()).build();
