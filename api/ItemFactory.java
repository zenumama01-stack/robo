 * This Factory creates concrete instances of the known ItemTypes.
public interface ItemFactory {
     * Creates a new Item instance of type <code>itemTypeName</code> and the name <code>itemName</code>
     * @param itemTypeName
     * @return a new Item of type <code>itemTypeName</code> or <code>null</code> if no matching class is known.
    Item createItem(@Nullable String itemTypeName, String itemName);
     * Returns the list of all supported ItemTypes of this Factory.
     * @return the supported ItemTypes
    String[] getSupportedItemTypes();
