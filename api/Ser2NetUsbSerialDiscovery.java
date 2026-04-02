package org.openhab.core.config.discovery.usbserial.ser2net.internal;
 * A {@link UsbSerialDiscovery} that implements background discovery of RFC2217 by listening to
 * ser2net mDNS service events.
@Component(service = UsbSerialDiscovery.class)
public class Ser2NetUsbSerialDiscovery implements ServiceListener, UsbSerialDiscovery {
    private final Logger logger = LoggerFactory.getLogger(Ser2NetUsbSerialDiscovery.class);
    static final String SERVICE_TYPE = "_iostream._tcp.local.";
    static final String PROPERTY_PROVIDER = "provider";
    static final String PROPERTY_DEVICE_TYPE = "devicetype";
    static final String PROPERTY_GENSIO_STACK = "gensiostack";
    static final String PROPERTY_VENDOR_ID = "idVendor";
    static final String PROPERTY_PRODUCT_ID = "idProduct";
    static final String PROPERTY_SERIAL_NUMBER = "serial";
    static final String PROPERTY_MANUFACTURER = "manufacturer";
    static final String PROPERTY_PRODUCT = "product";
    static final String PROPERTY_INTERFACE_NUMBER = "bInterfaceNumber";
    static final String PROPERTY_INTERFACE = "interface";
    static final String SER2NET = "ser2net";
    static final String SERIALUSB = "serialusb";
    static final String TELNET_RFC2217_TCP = "telnet(rfc2217),tcp";
    static final Duration SINGLE_SCAN_DURATION = Duration.ofSeconds(5);
    static final String SERIAL_PORT_NAME_FORMAT = "rfc2217://%s:%s";
    private volatile boolean notifyListeners = false;
    public Ser2NetUsbSerialDiscovery(final @Reference MDNSClient mdnsClient) {
        Set<UsbSerialDeviceInformation> lastScanResult;
            lastScanResult = Set.copyOf(this.lastScanResult);
        for (UsbSerialDeviceInformation deviceInfo : lastScanResult) {
        notifyListeners = true;
        mdnsClient.addServiceListener(SERVICE_TYPE, this);
        logger.debug("Started ser2net USB-Serial mDNS background discovery");
        notifyListeners = false;
        mdnsClient.removeServiceListener(SERVICE_TYPE, this);
        logger.debug("Stopped ser2net USB-Serial mDNS background discovery");
        logger.debug("Starting ser2net USB-Serial mDNS single discovery scan");
        Set<UsbSerialDeviceInformation> scanResult = Stream.of(mdnsClient.list(SERVICE_TYPE, SINGLE_SCAN_DURATION))
                .map(this::createUsbSerialDeviceInformation) //
                .flatMap(Optional::stream) //
        Set<UsbSerialDeviceInformation> added;
        Set<UsbSerialDeviceInformation> removed;
        Set<UsbSerialDeviceInformation> unchanged;
            added = setDifference(scanResult, lastScanResult);
            removed = setDifference(lastScanResult, scanResult);
            unchanged = setDifference(scanResult, added);
        removed.forEach(this::announceRemovedDevice);
        added.forEach(this::announceAddedDevice);
        unchanged.forEach(this::announceAddedDevice);
        logger.debug("Completed ser2net USB-Serial mDNS single discovery scan");
    private void announceAddedDevice(UsbSerialDeviceInformation deviceInfo) {
    private void announceRemovedDevice(UsbSerialDeviceInformation deviceInfo) {
    public void serviceAdded(@NonNullByDefault({}) ServiceEvent event) {
        // The service isn't resolved yet, so don't try to use it
    public void serviceRemoved(@NonNullByDefault({}) ServiceEvent event) {
        Optional<UsbSerialDeviceInformation> deviceInfo = createUsbSerialDeviceInformation(event.getInfo());
        if (deviceInfo.isPresent()) {
            UsbSerialDeviceInformation removed = deviceInfo.get();
                lastScanResult.remove(removed);
                announceRemovedDevice(removed);
    public void serviceResolved(@NonNullByDefault({}) ServiceEvent event) {
            UsbSerialDeviceInformation added = deviceInfo.get();
                lastScanResult.add(added);
                announceAddedDevice(added);
    private Optional<UsbSerialDeviceInformation> createUsbSerialDeviceInformation(ServiceInfo serviceInfo) {
        String provider = serviceInfo.getPropertyString(PROPERTY_PROVIDER);
        String deviceType = serviceInfo.getPropertyString(PROPERTY_DEVICE_TYPE);
        String gensioStack = serviceInfo.getPropertyString(PROPERTY_GENSIO_STACK);
        // Check ser2net specific properties when present
        if (SER2NET.equals(provider) && (deviceType != null && !SERIALUSB.equals(deviceType))
                || (gensioStack != null && !TELNET_RFC2217_TCP.equals(gensioStack))) {
            logger.debug("Skipping creation of UsbSerialDeviceInformation based on {}", serviceInfo);
            int vendorId = Integer.parseInt(serviceInfo.getPropertyString(PROPERTY_VENDOR_ID), 16);
            int productId = Integer.parseInt(serviceInfo.getPropertyString(PROPERTY_PRODUCT_ID), 16);
            String serialNumber = serviceInfo.getPropertyString(PROPERTY_SERIAL_NUMBER);
            String manufacturer = serviceInfo.getPropertyString(PROPERTY_MANUFACTURER);
            String product = serviceInfo.getPropertyString(PROPERTY_PRODUCT);
            int interfaceNumber = Integer.parseInt(serviceInfo.getPropertyString(PROPERTY_INTERFACE_NUMBER), 16);
            String interfaceDescription = serviceInfo.getPropertyString(PROPERTY_INTERFACE);
            String serialPortName = String.format(SERIAL_PORT_NAME_FORMAT, serviceInfo.getHostAddresses()[0],
                    serviceInfo.getPort());
            UsbSerialDeviceInformation deviceInfo = new UsbSerialDeviceInformation(vendorId, productId, serialNumber,
                    manufacturer, product, interfaceNumber, interfaceDescription, serialPortName).setRemote(true);
            logger.debug("Created {} based on {}", deviceInfo, serviceInfo);
            return Optional.of(deviceInfo);
            logger.debug("Failed to create UsbSerialDeviceInformation based on {}", serviceInfo, e);
