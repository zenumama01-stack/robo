 * This is a helper class that can be added to script scopes. It provides easy access to the current item states.
public class ItemRegistryDelegate implements Map<String, State> {
    public ItemRegistryDelegate(ItemRegistry itemRegistry) {
        return itemRegistry.getAll().size();
        return itemRegistry.getAll().isEmpty();
    public boolean containsKey(@Nullable Object key) {
        if (key instanceof String string) {
                itemRegistry.getItem(string);
    public boolean containsValue(@Nullable Object value) {
    public @Nullable State get(@Nullable Object key) {
        if (key == null) {
        final Item item = itemRegistry.get((String) key);
        if (item == null) {
        return item.getState();
    public @Nullable State put(String key, State value) {
    public @Nullable State remove(@Nullable Object key) {
    public void putAll(Map<? extends String, ? extends State> m) {
    public void clear() {
    public Set<String> keySet() {
        Set<String> keys = new HashSet<>();
        for (Item item : itemRegistry.getAll()) {
            keys.add(item.getName());
    public Collection<State> values() {
        Set<State> values = new HashSet<>();
            values.add(item.getState());
    public Set<java.util.Map.Entry<String, State>> entrySet() {
        Set<Map.Entry<String, State>> entries = new HashSet<>();
            entries.add(Map.entry(item.getName(), item.getState()));
