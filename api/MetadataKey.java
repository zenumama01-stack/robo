 * This class represents the key of a {@link Metadata} entity.
 * It is a simple combination of a namespace and an item name.
public final class MetadataKey extends AbstractUID {
    MetadataKey() {
     * @param namespace the namespace of this metadata key
     * @param itemName the item name that is associated with this metadata key
    public MetadataKey(String namespace, String itemName) {
        super(namespace, itemName);
     * Provides the item name of this key
     * @return the item name
     * Provides the namespace of this key
     * @return the namespace
