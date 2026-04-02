import org.openhab.core.items.GroupItem;
import org.openhab.core.items.Item;
import org.openhab.core.items.ItemNotFoundException;
import org.openhab.core.items.ItemNotUniqueException;
 * The {@link ProviderItemRegistryDelegate} is wrapping a {@link ItemRegistry} to provide a comfortable way to provide
 * items from scripts without worrying about the need to remove items again when the script is unloaded.
 * Nonetheless, using the {@link #addPermanent(Item)} method it is still possible to add items permanently.
public class ProviderItemRegistryDelegate implements ItemRegistry, ProviderRegistry {
    private final Set<String> items = new HashSet<>();
    private final ScriptedItemProvider scriptedProvider;
    public ProviderItemRegistryDelegate(ItemRegistry itemRegistry, ScriptedItemProvider scriptedProvider) {
    public void addRegistryChangeListener(RegistryChangeListener<Item> listener) {
        itemRegistry.addRegistryChangeListener(listener);
    public Collection<Item> getAll() {
        return itemRegistry.getAll();
    public Stream<Item> stream() {
        return itemRegistry.stream();
    public @Nullable Item get(String key) {
        return itemRegistry.get(key);
    public void removeRegistryChangeListener(RegistryChangeListener<Item> listener) {
        itemRegistry.removeRegistryChangeListener(listener);
    public Item add(Item element) {
        String itemName = element.getName();
        // Check for item already existing here because the item might exist in a different provider, so we need to
        if (get(itemName) != null) {
                    "Cannot add item, because an item with same name (" + itemName + ") already exists.");
        items.add(itemName);
     * Add an item permanently to the registry.
     * This item will be kept in the registry even if the script is unloaded.
     * @param element the item to be added (must not be null)
     * @return the added item
    public Item addPermanent(Item element) {
        return itemRegistry.add(element);
    public @Nullable Item update(Item element) {
        if (items.contains(element.getName())) {
        return itemRegistry.update(element);
    public @Nullable Item remove(String key) {
        if (items.remove(key)) {
        return itemRegistry.remove(key);
    public Item getItem(String name) throws ItemNotFoundException {
        return itemRegistry.getItem(name);
    public Item getItemByPattern(String name) throws ItemNotFoundException, ItemNotUniqueException {
        return itemRegistry.getItemByPattern(name);
    public Collection<Item> getItems() {
        return itemRegistry.getItems();
    public Collection<Item> getItemsOfType(String type) {
        return itemRegistry.getItemsOfType(type);
    public Collection<Item> getItems(String pattern) {
        return itemRegistry.getItems(pattern);
    public Collection<Item> getItemsByTag(String... tags) {
        return itemRegistry.getItemsByTag(tags);
    public Collection<Item> getItemsByTagAndType(String type, String... tags) {
        return itemRegistry.getItemsByTagAndType(type, tags);
    public <T extends Item> Collection<T> getItemsByTag(Class<T> typeFilter, String... tags) {
        return itemRegistry.getItemsByTag(typeFilter, tags);
    public @Nullable Item remove(String itemName, boolean recursive) {
        Item item = get(itemName);
        if (recursive && item instanceof GroupItem groupItem) {
            for (String member : getMemberNamesRecursively(groupItem, getAll())) {
                remove(member);
        if (item != null) {
            remove(item.getName());
        for (String item : items) {
            scriptedProvider.remove(item);
        items.clear();
    private List<String> getMemberNamesRecursively(GroupItem groupItem, Collection<Item> allItems) {
        List<String> memberNames = new ArrayList<>();
        for (Item item : allItems) {
            if (item.getGroupNames().contains(groupItem.getName())) {
                memberNames.add(item.getName());
                if (item instanceof GroupItem groupItem1) {
                    memberNames.addAll(getMemberNamesRecursively(groupItem1, allItems));
        return memberNames;
