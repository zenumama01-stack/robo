 * The ItemRegistry is the central place, where items are kept in memory and their state
 * is permanently tracked. So any code that requires the current state of items should use
 * this service (instead of trying to keep their own local copy of the items).
 * Items are registered by {@link ItemProvider}s, which can provision them from any source
 * they like and also dynamically remove or add items.
public interface ItemRegistry extends Registry<Item, String> {
     * This method retrieves a single item from the registry.
     * @param name the item name
     * @return the uniquely identified item
     * @throws ItemNotFoundException if no item matches the input
    Item getItem(String name) throws ItemNotFoundException;
     * Search patterns and shortened versions are supported, if they uniquely identify an item
     * @param name the item name, a part of the item name or a search pattern
     * @throws ItemNotUniqueException if multiply items match the input
    Item getItemByPattern(String name) throws ItemNotFoundException, ItemNotUniqueException;
     * This method retrieves all items that are currently available in the registry
     * @return a collection of all available items
    Collection<Item> getItems();
     * This method retrieves all items with the given type
     * @param type item type as defined by {@link ItemFactory}s
     * @return a collection of all items of the given type
    Collection<Item> getItemsOfType(String type);
     * This method retrieves all items that match a given search pattern
     * @return a collection of all items matching the search pattern
    Collection<Item> getItems(String pattern);
     * Returns list of items which contains all of the given tags.
     * @param tags array of tags to be present on the returned items.
     * @return list of items which contains all of the given tags.
    Collection<Item> getItemsByTag(String... tags);
     * Returns list of items with a certain type containing all of the given tags.
    Collection<Item> getItemsByTagAndType(String type, String... tags);
     * @param typeFilter subclass of {@link GenericItem} to filter the resulting list for.
     * @return list of items which contains all of the given tags, which is
     *         filtered by the given type filter.
    <T extends Item> Collection<T> getItemsByTag(Class<T> typeFilter, String... tags);
     * @see ManagedItemProvider#remove(String, boolean)
    Item remove(String itemName, boolean recursive);
     * Called when an item instance has been externally updated in order to inform all registry
     * listeners the change between its old and new state.
    default void notifyListenersAboutItemExternalUpdate(Item oldItem, Item newItem) {
