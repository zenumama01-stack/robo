 * This is the main implementing class of the {@link MetadataRegistry} interface. It
 * keeps track of all declared metadata of all metadata providers.
 * @author Mark Herwege - semantics namespace not in managed provider
@Component(immediate = true, service = MetadataRegistry.class)
public class MetadataRegistryImpl extends AbstractRegistry<Metadata, MetadataKey, MetadataProvider>
        implements MetadataRegistry {
    private final Logger logger = LoggerFactory.getLogger(MetadataRegistryImpl.class);
    private final Map<String, Set<MetadataProvider>> reservedNamespaces = new ConcurrentHashMap<>();
    public MetadataRegistryImpl(final @Reference ReadyService readyService) {
        super(MetadataProvider.class);
        return namespace.startsWith(INTERNAL_NAMESPACE_PREFIX);
     * Provides all namespaces of a particular item
     * @param itemname the name of the item for which the namespaces should be searched.
        return stream().map(Metadata::getUID).filter(key -> key.getItemName().equals(itemname))
                .map(MetadataKey::getNamespace).collect(Collectors.toSet());
    protected void setManagedProvider(ManagedMetadataProvider provider) {
    protected void unsetManagedProvider(ManagedMetadataProvider managedProvider) {
    public void removeItemMetadata(String itemName) {
        ManagedMetadataProvider mp = (ManagedMetadataProvider) getManagedProvider();
        if (mp != null) {
            mp.removeItemMetadata(itemName);
        String namespace = element.getUID().getNamespace();
        Set<MetadataProvider> providers = reservedNamespaces.get(namespace);
        if (providers == null || providers.isEmpty()
                || providers.stream().anyMatch(p -> p.equals(getManagedProvider()))) {
            return super.add(element);
        throw new UnsupportedOperationException("Cannot add metadata to '" + namespace + "' namespace");
        throw new UnsupportedOperationException("Cannot update metadata in '" + namespace + "' namespace");
        String namespace = key.getNamespace();
            return super.remove(key);
        throw new UnsupportedOperationException("Cannot remove metadata from '" + namespace + "' namespace");
    protected void addProvider(Provider<Metadata> provider) {
        if (provider instanceof MetadataProvider metadataProvider) {
            metadataProvider.getReservedNamespaces().stream().forEach(namespace -> {
                Set<MetadataProvider> currentProviders = reservedNamespaces.getOrDefault(namespace, Set.of());
                if (!currentProviders.isEmpty()) {
                    logger.debug("Multiple metadata providers are reserving namespace '{}', there should only be one.",
                            namespace);
                Set<MetadataProvider> providers = Stream
                        .concat(currentProviders.stream(), Set.of(metadataProvider).stream())
                reservedNamespaces.put(namespace, providers);
    protected void removeProvider(Provider<Metadata> provider) {
                Set<MetadataProvider> providers = reservedNamespaces.getOrDefault(namespace, Set.of()).stream()
                        .filter(p -> !provider.equals(p)).collect(Collectors.toSet());
                    reservedNamespaces.remove(namespace);
