 * Registers all {@link SerialPortProvider}s which can be accessed here.
@Component(service = SerialPortRegistry.class)
public class SerialPortRegistry {
    private final Collection<SerialPortProvider> portCreators = new HashSet<>();
     * Registers a {@link SerialPortProvider}.
     * @param creator
    @Reference(cardinality = ReferenceCardinality.AT_LEAST_ONE, policy = ReferencePolicy.DYNAMIC, policyOption = ReferencePolicyOption.GREEDY)
    protected void registerSerialPortCreator(SerialPortProvider creator) {
        synchronized (portCreators) {
            portCreators.add(creator);
    protected void unregisterSerialPortCreator(SerialPortProvider creator) {
            portCreators.remove(creator);
     * Gets the best applicable {@link SerialPortProvider} for the given <code>portName</code>
     * @param portName The port's name.
     * @return all possible {@link SerialPortProvider}. If no provider is available an empty collection is returned
    public Collection<SerialPortProvider> getPortProvidersForPortName(URI portName) {
        final String scheme = portName.getScheme();
        final PathType pathType = PathType.fromURI(portName);
        final Predicate<SerialPortProvider> filter;
            // Get port providers which accept exactly the port with its scheme.
            filter = provider -> provider.getAcceptedProtocols().anyMatch(prot -> prot.getScheme().equals(scheme));
            // Get port providers which accept the same type (local, net)
            filter = provider -> provider.getAcceptedProtocols().anyMatch(prot -> prot.getPathType().equals(pathType));
        return portCreators.stream().filter(filter).toList();
    public Collection<SerialPortProvider> getPortCreators() {
            return Set.copyOf(portCreators);
