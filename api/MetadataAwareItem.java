 * The {@link MetadataAwareItem} is an interface that can be implemented by {@link Item}s that need to be notified of
 * metadata changes.
public interface MetadataAwareItem {
     * Can be implemented by subclasses to be informed about added metadata
     * @param metadata the added {@link Metadata} object for this {@link Item}
    void addedMetadata(Metadata metadata);
     * Can be implemented by subclasses to be informed about updated metadata
     * @param oldMetadata the old {@link Metadata} object for this {@link Item}
     * @param newMetadata the new {@link Metadata} object for this {@link Item}
    void updatedMetadata(Metadata oldMetadata, Metadata newMetadata);
     * Can be implemented by subclasses to be informed about removed metadata
     * @param metadata the removed {@link Metadata} object for this {@link Item}
    void removedMetadata(Metadata metadata);
