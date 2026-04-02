package org.openhab.core.config.discovery.addon.usb;
import static org.openhab.core.config.discovery.addon.AddonFinderConstants.SERVICE_NAME_USB;
import static org.openhab.core.config.discovery.addon.AddonFinderConstants.SERVICE_TYPE_USB;
import org.openhab.core.config.discovery.usbserial.UsbSerialDeviceInformation;
import org.openhab.core.config.discovery.usbserial.UsbSerialDiscovery;
import org.openhab.core.config.discovery.usbserial.UsbSerialDiscoveryListener;
 * This is a {@link AddonFinder} for finding suggested add-ons related to USB devices.
 * It supports the following values for the 'match-property' 'name' element:
 * <li>product - match on the product description text
 * <li>manufacturer - match on the device manufacturer text
 * <li>chipId - match on the chip vendor id plus product id
 * <li>remote - match on whether the device is connected remotely or locally
@Component(service = AddonFinder.class, name = UsbAddonFinder.SERVICE_NAME)
public class UsbAddonFinder extends BaseAddonFinder implements UsbSerialDiscoveryListener {
    public static final String SERVICE_TYPE = SERVICE_TYPE_USB;
    public static final String SERVICE_NAME = SERVICE_NAME_USB;
     * Supported 'match-property' names
    public static final String PRODUCT = "product";
    public static final String MANUFACTURER = "manufacturer";
    public static final String CHIP_ID = "chipId";
    public static final String REMOTE = "remote";
    public static final Set<String> SUPPORTED_PROPERTIES = Set.of(PRODUCT, MANUFACTURER, CHIP_ID, REMOTE);
    private final Logger logger = LoggerFactory.getLogger(UsbAddonFinder.class);
    private final Map<Long, UsbSerialDeviceInformation> usbDeviceInformations = new HashMap<>();
    protected void addUsbSerialDiscovery(UsbSerialDiscovery usbSerialDiscovery) {
        usbSerialDiscovery.registerDiscoveryListener(this);
    protected synchronized void removeUsbSerialDiscovery(UsbSerialDiscovery usbSerialDiscovery) {
        usbSerialDiscovery.unregisterDiscoveryListener(this);
                    for (UsbSerialDeviceInformation device : usbDeviceInformations.values()) {
                        logger.trace("Checking device: {}", device);
                        if (propertyMatches(matchProperties, PRODUCT, device.getProduct())
                                && propertyMatches(matchProperties, MANUFACTURER, device.getManufacturer())
                                && propertyMatches(matchProperties, CHIP_ID,
                                        getChipId(device.getVendorId(), device.getProductId()))
                                && propertyMatches(matchProperties, REMOTE, String.valueOf(device.getRemote()))) {
    private String getChipId(int vendorId, int productId) {
        return String.format("%04x:%04x", vendorId, productId);
     * Create a unique 33 bit integer map hash key comprising the remote flag in the upper bit, the vendorId in the
     * middle 16 bits, and the productId in the lower 16 bits.
    private long keyOf(UsbSerialDeviceInformation deviceInfo) {
        return (deviceInfo.getRemote() ? 0x1_0000_0000L : 0) + (deviceInfo.getVendorId() * 0x1_0000L)
                + deviceInfo.getProductId();
     * Add the discovered USB device information record to our internal map. If there is already an entry in the map
     * then merge the two sets of data.
     * @param discoveredInfo the newly discovered USB device information.
    public void usbSerialDeviceDiscovered(UsbSerialDeviceInformation discoveredInfo) {
        UsbSerialDeviceInformation targetInfo = discoveredInfo;
            UsbSerialDeviceInformation existingInfo = usbDeviceInformations.get(keyOf(targetInfo));
            if (existingInfo != null) {
                boolean isMerging = false;
                String product = existingInfo.getProduct();
                if (product != null) {
                    product = discoveredInfo.getProduct();
                    isMerging = true;
                String manufacturer = existingInfo.getManufacturer();
                if (manufacturer != null) {
                    manufacturer = discoveredInfo.getManufacturer();
                String serialNumber = existingInfo.getSerialNumber();
                if (serialNumber != null) {
                    serialNumber = discoveredInfo.getSerialNumber();
                boolean remote = existingInfo.getRemote();
                if (remote == discoveredInfo.getRemote()) {
                if (isMerging) {
                    targetInfo = new UsbSerialDeviceInformation(discoveredInfo.getVendorId(),
                            discoveredInfo.getProductId(), serialNumber, manufacturer, product,
                            discoveredInfo.getInterfaceNumber(), discoveredInfo.getInterfaceDescription(),
                            discoveredInfo.getSerialPort()).setRemote(remote);
            usbDeviceInformations.put(keyOf(targetInfo), targetInfo);
    public synchronized void usbSerialDeviceRemoved(UsbSerialDeviceInformation removedInfo) {
        usbDeviceInformations.remove(keyOf(removedInfo));
