 * This exception is thrown by the {@link ItemRegistry} if an item could
 * not be found.
public class ItemNotFoundException extends ItemLookupException {
    public ItemNotFoundException(String name) {
        super("Item '" + name + "' could not be found in the item registry");
    private static final long serialVersionUID = -3720784568250902711L;
