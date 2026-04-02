 * {@link ManagedMetadataProvider} is an OSGi service interface that allows to add or remove
 * a {@link org.openhab.core.storage.StorageService}.
public interface ManagedMetadataProvider extends ManagedProvider<Metadata, MetadataKey>, MetadataProvider {
    void removeItemMetadata(String name);
