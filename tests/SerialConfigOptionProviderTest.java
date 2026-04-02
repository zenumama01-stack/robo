import static org.openhab.core.config.serial.internal.SerialConfigOptionProvider.SERIAL_PORT;
 * Unit tests for the {@link SerialConfigOptionProvider}.
public class SerialConfigOptionProviderTest {
    private static final String DEV_TTY_S1 = "/dev/ttyS1";
    private static final String DEV_TTY_S2 = "/dev/ttyS2";
    private static final String DEV_TTY_S3 = "/dev/ttyS3";
    private static final String RFC2217_IPV4 = "rfc2217://1.1.1.1:1000";
    private static final String RFC2217_IPV6 = "rfc2217://[0:0:0:0:0:ffff:0202:0202]:2222";
            "product1", 0x1, "interface1", RFC2217_IPV4);
            "product2", 0x2, "interface2", RFC2217_IPV6);
    private UsbSerialDeviceInformation usb3 = new UsbSerialDeviceInformation(0x300, 0x333, "serial3", "manufacturer3",
            "product3", 0x3, "interface3", DEV_TTY_S3);
    private @Mock @NonNullByDefault({}) SerialPortManager serialPortManagerMock;
    private @Mock @NonNullByDefault({}) UsbSerialDiscovery usbSerialDiscoveryMock;
    private @Mock @NonNullByDefault({}) SerialPortIdentifier serialPortIdentifier1Mock;
    private @Mock @NonNullByDefault({}) SerialPortIdentifier serialPortIdentifier2Mock;
    private @Mock @NonNullByDefault({}) SerialPortIdentifier serialPortIdentifier3Mock;
    private @NonNullByDefault({}) SerialConfigOptionProvider provider;
        provider = new SerialConfigOptionProvider(serialPortManagerMock);
        when(serialPortIdentifier1Mock.getName()).thenReturn(DEV_TTY_S1);
        when(serialPortIdentifier2Mock.getName()).thenReturn(DEV_TTY_S2);
        when(serialPortIdentifier3Mock.getName()).thenReturn(DEV_TTY_S3);
    private void assertParameterOptions(String... serialPortIdentifiers) {
        Collection<ParameterOption> actual = provider.getParameterOptions(URI.create("uri"), "serialPort", SERIAL_PORT,
        Collection<ParameterOption> expected = Arrays.stream(serialPortIdentifiers)
                .map(id -> new ParameterOption(id, id)).toList();
        assertThat(actual, is(expected));
    public void noSerialPortIdentifiers() {
        when(serialPortManagerMock.getIdentifiers()).thenReturn(Stream.of());
        assertParameterOptions();
    public void serialPortManagerIdentifiersOnly() {
        when(serialPortManagerMock.getIdentifiers())
                .thenReturn(Stream.of(serialPortIdentifier1Mock, serialPortIdentifier2Mock));
        assertParameterOptions(DEV_TTY_S1, DEV_TTY_S2);
    public void discoveredIdentifiersOnly() {
        provider.addUsbSerialDiscovery(usbSerialDiscoveryMock);
        provider.usbSerialDeviceDiscovered(usb1);
        provider.usbSerialDeviceDiscovered(usb2);
        assertParameterOptions(RFC2217_IPV4, RFC2217_IPV6);
    public void serialPortManagerAndDiscoveredIdentifiers() {
        assertParameterOptions(DEV_TTY_S1, DEV_TTY_S2, RFC2217_IPV4, RFC2217_IPV6);
    public void removedDevicesAreRemoved() {
        assertParameterOptions(RFC2217_IPV4);
        provider.usbSerialDeviceRemoved(usb1);
    public void discoveryRemovalClearsDiscoveryResults() {
        provider.usbSerialDeviceDiscovered(usb3);
        assertParameterOptions(RFC2217_IPV4, RFC2217_IPV6, DEV_TTY_S3);
        provider.removeUsbSerialDiscovery(usbSerialDiscoveryMock);
    public void serialPortIdentifiersAreUnique() {
        when(serialPortManagerMock.getIdentifiers()).thenReturn(Stream.of(serialPortIdentifier3Mock));
        assertParameterOptions(DEV_TTY_S3);
    public void nullResultIfContextDoesNotMatch() {
        Collection<ParameterOption> actual = provider.getParameterOptions(URI.create("uri"), "serialPort",
                "otherContext", null);
        assertThat(actual, is(nullValue()));
