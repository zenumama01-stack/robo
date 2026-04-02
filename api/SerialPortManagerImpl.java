 * Specific serial port manager implementation.
 * @author Markus Rathgeb - Respect the possible failure of port identifier creation
public class SerialPortManagerImpl implements SerialPortManager {
    private final Logger logger = LoggerFactory.getLogger(SerialPortManagerImpl.class);
    private final SerialPortRegistry registry;
    public SerialPortManagerImpl(final @Reference SerialPortRegistry registry) {
    public Stream<SerialPortIdentifier> getIdentifiers() {
        return registry.getPortCreators().stream().flatMap(provider -> {
                return provider.getSerialPortIdentifiers();
            } catch (final UnsatisfiedLinkError error) {
                 * At the time of writing every serial implementation needs some native code.
                 * So missing some native code for a specific platform is a potential error and we should not
                 * break the whole handling just because one of the provider miss some of that code.
                logger.warn("The provider \"{}\" miss some native code support.", provider.getClass().getSimpleName(),
                logger.warn("The provider \"{}\" cannot provide its serial port identifiers.",
                        provider.getClass().getSimpleName(), ex);
    public @Nullable SerialPortIdentifier getIdentifier(String name) {
        final URI portUri = URI.create(name);
        for (final SerialPortProvider provider : registry.getPortProvidersForPortName(portUri)) {
                return provider.getPortIdentifier(portUri);
                logger.warn("The provider \"{}\" cannot provide a serial port itendifier for \"{}\".",
                        provider.getClass().getSimpleName(), name, ex);
        logger.warn("No SerialPortProvider found for: {}", name);
