 * Interface for a serial port manager.
public interface SerialPortManager {
     * Gets a serial port identifier for a given name.
     * @param name the name
     * @return a serial port identifier or null
    default @Nullable SerialPortIdentifier getIdentifier(final String name) {
        final Optional<SerialPortIdentifier> opt = getIdentifiers().filter(id -> id.getName().equals(name)).findFirst();
        if (opt.isPresent()) {
            return opt.get();
     * Gets the discovered serial port identifiers.
     * {@link SerialPortProvider}s may not be able to discover any or all identifiers.
     * When the port name is known, the preferred way to get an identifier is by using {@link #getIdentifier(String)}.
     * @return stream of discovered serial port identifiers
    Stream<SerialPortIdentifier> getIdentifiers();
