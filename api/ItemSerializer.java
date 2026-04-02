 * {@link ItemSerializer} is the interface to implement by any file generator for {@link Item} object.
public interface ItemSerializer extends ObjectSerializer<Item> {
     * Specify the {@link List} of {@link Item}s (including {@link Metadata} and channel links) to be serialized and
     * associate them with an identifier.
     * @param id the identifier of the {@link Item} format generation.
     * @param items the {@link List} of {@link Item}s to serialize.
     * @param metadata the provided {@link Collection} of {@link Metadata} for the {@link Item}s (including channel
     *            links).
     * @param stateFormatters a {@link Map} of {@link Item} name and state formatter for each {@link Item}. Callers
     *            should pass an empty map if no state formatters are provided.
    void setItemsToBeSerialized(String id, List<Item> items, Collection<Metadata> metadata,
            Map<String, String> stateFormatters, boolean hideDefaultParameters);
