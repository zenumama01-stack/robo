 * The MetadataRegistry is the central place, where additional information about items is kept.
 * Metadata can be supplied by {@link MetadataProvider}s, which can provision them from any source
 * they like and also dynamically remove or add data.
public interface MetadataRegistry extends Registry<Metadata, MetadataKey> {
    String INTERNAL_NAMESPACE_PREFIX = "_";
     * Determines whether the given namespace is internal.
     * @param namespace the metadata namespace to check
     * @return {@code true} if the given namespace is internal, {@code false} otherwise
    boolean isInternalNamespace(String namespace);
    Collection<String> getAllNamespaces(String itemname);
     * Remove all metadata of a given item
     * @param itemname the name of the item for which the metadata is to be removed.
    void removeItemMetadata(String itemname);
     * Add element to metadata
     * @param element the element to add (must not be null)
     * @throws UnsupportedOperationException if the metadata namespace has a reserved {@link MetadataProvider} that is
     *             not a {@link ManagedProvider}
    Metadata add(Metadata element);
     * Update element in metadata
     * @param element the element to update (must not be null)
    Metadata update(Metadata element);
     * Remove element from metadata
     * @param key the key of the element to remove (must not be null)
     * @return the removed element, or null if no element with the given key exists
    Metadata remove(MetadataKey key);
