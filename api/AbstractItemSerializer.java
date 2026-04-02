package org.openhab.core.items.fileconverter;
 * {@link AbstractItemSerializer} is the base class for any {@link Item} serializer.
public abstract class AbstractItemSerializer implements ItemSerializer {
    public AbstractItemSerializer() {
    public record ConfigParameter(String name, Object value) {
     * Get the list of available channel links for an item, sorted by natural order of their channel UID.
     * @param metadata a collection of metadata
     * @return the sorted list of metadata representing the channel links for this item
    protected List<Metadata> getChannelLinks(Collection<Metadata> metadata, String itemName) {
        return metadata.stream().filter(
                md -> "channel".equals(md.getUID().getNamespace()) && md.getUID().getItemName().equals(itemName))
                .sorted((md1, md2) -> {
                    return md1.getValue().compareTo(md2.getValue());
     * Get the list of available metadata for an item, sorted by natural order of their namespaces.
     * The "semantics" and "channel" namespaces are ignored.
     * @return the sorted list of metadata for this item
    protected List<Metadata> getMetadata(Collection<Metadata> metadata, String itemName) {
        return metadata.stream()
                .filter(md -> !"semantics".equals(md.getUID().getNamespace())
                        && !"channel".equals(md.getUID().getNamespace()) && md.getUID().getItemName().equals(itemName))
                    return md1.getUID().getNamespace().compareTo(md2.getUID().getNamespace());
     * Get the list of configuration parameters for a metadata, sorted by natural order of their names
     * with the exception of the "stateDescription" namespace where "min", "max" and "step" parameters
     * are provided at first in this order.
     * @param metadata the metadata
     * @return a sorted list of configuration parameters for the metadata
    protected List<ConfigParameter> getConfigurationParameters(Metadata metadata) {
        String namespace = metadata.getUID().getNamespace();
        Map<String, Object> configParams = metadata.getConfiguration();
        List<String> paramNames = configParams.keySet().stream().sorted((key1, key2) -> {
            if ("stateDescription".equals(namespace)) {
                if ("min".equals(key1)) {
                } else if ("min".equals(key2)) {
                } else if ("max".equals(key1)) {
                } else if ("max".equals(key2)) {
                } else if ("step".equals(key1)) {
                } else if ("step".equals(key2)) {
            return key1.compareTo(key2);
        for (String paramName : paramNames) {
            Object value = configParams.get(paramName);
     * Get the default state pattern for an item.
     * @return the default state pattern of null if no default
    protected @Nullable String getDefaultStatePattern(Item item) {
        String pattern = null;
                pattern = getDefaultStatePattern(baseItem);
            pattern = "%.0f %unit%";
                    pattern = "%s";
                    pattern = "%1$tY-%1$tm-%1$td %1$tH:%1$tM:%1$tS";
                    pattern = "%.0f";
