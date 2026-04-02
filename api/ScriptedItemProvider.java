import org.openhab.core.items.ItemProvider;
import org.openhab.core.items.ItemUtil;
 * This {@link ItemProvider} keeps items provided by scripts during runtime.
 * This ensures that items are not kept on reboot, but have to be provided by the scripts again.
@Component(immediate = true, service = { ItemProvider.class, ScriptedItemProvider.class })
public class ScriptedItemProvider extends AbstractProvider<Item>
        implements ItemProvider, ManagedProvider<Item, String> {
    private final Logger logger = LoggerFactory.getLogger(ScriptedItemProvider.class);
    private final Map<String, Item> items = new HashMap<>();
        return items.values();
    public @Nullable Item get(String itemName) {
        return items.get(itemName);
    public void add(Item item) {
        if (!ItemUtil.isValidItemName(item.getName())) {
            throw new IllegalArgumentException("The item name '" + item.getName() + "' is invalid.");
        if (items.get(item.getName()) != null) {
                    "Cannot add item, because an item with same name (" + item.getName() + ") already exists.");
        items.put(item.getName(), item);
        notifyListenersAboutAddedElement(item);
    public @Nullable Item update(Item item) {
        Item oldItem = items.get(item.getName());
        if (oldItem != null) {
            notifyListenersAboutUpdatedElement(oldItem, item);
            logger.warn("Could not update item with name '{}', because it does not exist.", item.getName());
        return oldItem;
    public @Nullable Item remove(String itemName) {
        Item item = items.remove(itemName);
            notifyListenersAboutRemovedElement(item);
