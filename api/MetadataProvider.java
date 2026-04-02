 * This is a marker interface for metadata provider implementations that should be used to register those as an OSGi
 * @author Mark Herwege - Added reserved namespaces
public interface MetadataProvider extends Provider<Metadata> {
     * A {@link MetadataProvider} implementation can reserve a metadata namespace. Only a single provider for this
     * namespace should provide metadata for this namespace. Updating metadata in this namespace will have to be with
     * this provider, and is refused if the provider is not the {@link ManagedProvider} for the registry. If multiple
     * providers for this namespace are added to the registry, all of them will be considered as reserved. This should
     * be avoided.
     * This is useful if providers calculate metadata and this metadata is not meant to be persisted with a
     * {@link ManagedProvider}. An example is semantics metadata provided by its own provider.
     * Implementations are expected to return an immutable {@link Collection}.
     * The default implementation returns an empty {@link Set}.
     * @return collection reserved namespaces
    public default Collection<String> getReservedNamespaces() {
