 * Provides a concrete SerialPort which can handle remote (e.g. via rfc2217) or local ports.
public interface SerialPortProvider {
     * Gets the {@link SerialPortIdentifier} if it is available or null otherwise.
     * @param portName The ports name.
     * @return The created {@link SerialPort} or <code>null</code> if the serial port does not exist.
     * @throws UnsupportedCommOperationException
     * @throws PortInUseException
    SerialPortIdentifier getPortIdentifier(URI portName);
     * Gets all protocol types which this provider is able to create.
     * @return The protocol type.
    Stream<ProtocolType> getAcceptedProtocols();
     * Gets all the available {@link SerialPortIdentifier}s for this {@link SerialPortProvider}.
     * Please note: Discovery is not available necessarily, hence the {@link #getPortIdentifier(URI)} must be used in
     * this case.
     * @return The available ports
    Stream<SerialPortIdentifier> getSerialPortIdentifiers();
