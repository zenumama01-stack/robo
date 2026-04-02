 * {@link ItemParser} is the interface to implement by any file parser for {@link Item} object.
public interface ItemParser extends ObjectParser<Item> {
     * Parse the provided {@code syntax} string without impacting the item and metadata registries.
     * Get the {@link Item} objects found when parsing the format.
     * @return The {@link Collection} of {@link Item}s.
    Collection<Item> getParsedObjects(String modelName);
     * Get the {@link Metadata} objects found when parsing the format.
     * @return The {@link Collection} of {@link Metadata}.
    Collection<Metadata> getParsedMetadata(String modelName);
     * Get the state formatters found when parsing the format.
     * @return the state formatters as a {@link Map} per item name.
    Map<String, String> getParsedStateFormatters(String modelName);
