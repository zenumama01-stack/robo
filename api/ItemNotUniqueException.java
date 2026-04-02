 * This exception can be thrown whenever a search pattern does not uniquely identify
 * an item. The list of matching items must be made available through this exception.
public class ItemNotUniqueException extends ItemLookupException {
    private static final long serialVersionUID = 5154625234283910124L;
    private final Collection<Item> matchingItems;
    public ItemNotUniqueException(String string, Collection<Item> items) {
        super("Item cannot be uniquely identified by '" + string + "'");
        this.matchingItems = items;
     * Returns all items that match the search pattern
     * @return collection of items matching the search pattern
    public Collection<Item> getMatchingItems() {
        return matchingItems;
