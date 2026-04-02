 * Creates a new {@link ItemBuilder} which is based on all available {@link ItemFactory}s.
public interface ItemBuilderFactory {
     * Create a new {@link ItemBuilder}, which is initialized by the given item.
     * @param item the template to initialize the builder with
     * @return an ItemBuilder instance
    ItemBuilder newItemBuilder(Item item);
     * @param itemType the item type to create
     * @param itemName the name of the item to create
    ItemBuilder newItemBuilder(String itemType, String itemName);
