public class SerialPortUtil {
    private static final String GNU_IO_RXTX_SERIAL_PORTS = "gnu.io.rxtx.SerialPorts";
    private static synchronized boolean isSerialPortsKeySet() {
        return System.getProperties().containsKey(GNU_IO_RXTX_SERIAL_PORTS);
    public static synchronized CommPortIdentifier getPortIdentifier(String port) throws NoSuchPortException {
        if ((System.getProperty("os.name").toLowerCase().indexOf("linux") != -1)) {
            appendSerialPortProperty(port);
        return CommPortIdentifier.getPortIdentifier(port);
     * Registers the given port as system property {@value #GNU_IO_RXTX_SERIAL_PORTS}.
     * The method is capable of extending the system property, if any other ports are already registered.
     * @param port the port to be registered
    private static synchronized void appendSerialPortProperty(String port) {
        String serialPortsProperty = System.getProperty(GNU_IO_RXTX_SERIAL_PORTS);
        String newValue = initSerialPort(port, serialPortsProperty);
        if (newValue != null) {
            System.setProperty(GNU_IO_RXTX_SERIAL_PORTS, newValue);
     * Scans for available port identifiers by calling RXTX without using the ({@value #GNU_IO_RXTX_SERIAL_PORTS}
     * property. Finds port identifiers based on operating system and distribution.
     * @return the scanned port identifiers
    public static synchronized Stream<CommPortIdentifier> getPortIdentifiersUsingScan() {
        Enumeration<CommPortIdentifier> identifiers;
        if (isSerialPortsKeySet()) {
            // Save the existing serial ports property
            String value = System.getProperty(GNU_IO_RXTX_SERIAL_PORTS);
            // Clear the property so library scans the ports
            System.clearProperty(GNU_IO_RXTX_SERIAL_PORTS);
            identifiers = CommPortIdentifier.getPortIdentifiers();
            // Restore the existing serial ports property
                System.setProperty(GNU_IO_RXTX_SERIAL_PORTS, value);
        // Save the Enumeration to a new list so the result is thread safe
        return Collections.list(identifiers).stream();
     * Get the port identifiers for the ports in the system property by calling RXTX while using the
     * ({@value #GNU_IO_RXTX_SERIAL_PORTS} property.
     * @return the port identifiers for the ports defined in the {@value #GNU_IO_RXTX_SERIAL_PORTS} property
    public static synchronized Stream<CommPortIdentifier> getPortIdentifiersUsingProperty() {
            return Collections.list(CommPortIdentifier.getPortIdentifiers()).stream();
    static @Nullable String initSerialPort(String port, @Nullable String serialPortsProperty) {
        String pathSeparator = File.pathSeparator;
        Set<String> serialPorts;
        if (serialPortsProperty != null) {
            serialPorts = Stream.of(serialPortsProperty.split(pathSeparator)).collect(Collectors.toSet());
            serialPorts = new HashSet<>();
        if (serialPorts.add(port)) {
            return String.join(pathSeparator, serialPorts); // see RXTXCommDriver#addSpecifiedPorts
