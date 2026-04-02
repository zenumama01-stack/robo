import java.util.concurrent.ConcurrentMap;
 * This is a simple expiring and reloading multiple key-value-pair cache implementation. The value expires after the
 * specified duration has passed since the item was created, or the most recent replacement of the value.
 * @author Martin van Wingerden - Added constructor accepting Duration and putIfAbsentAndGet
 * @param <K> the type of the key
public class ExpiringCacheMap<K, V> {
    private final Logger logger = LoggerFactory.getLogger(ExpiringCacheMap.class);
    private final ConcurrentMap<K, ExpiringCache<@Nullable V>> items;
    public ExpiringCacheMap(Duration expiry) {
        this(expiry.toMillis());
    public ExpiringCacheMap(long expiry) {
        this.items = new ConcurrentHashMap<>();
     * Creates an {@link ExpiringCache} and adds it to the cache.
     * @param key the key with which the specified value is to be associated
     * @param action the action for the item to be associated with the specified key to retrieve/calculate the value
    public void put(K key, Supplier<@Nullable V> action) {
        put(key, new ExpiringCache<>(expiry, action));
     * Adds an {@link ExpiringCache} to the cache.
     * @param item the item to be associated with the specified key
    public void put(K key, ExpiringCache<@Nullable V> item) {
            throw new IllegalArgumentException("Item cannot be added as key is null.");
        items.put(key, item);
     * If the specified key is not already associated with a value, associate it with the given {@link ExpiringCache}.
    public void putIfAbsent(K key, ExpiringCache<V> item) {
        items.putIfAbsent(key, item);
     * If the specified key is not already associated, associate it with the given action.
     * Note that this method has the overhead of actually calling/performing the action
     * @return the (cached) value for the specified key
    public @Nullable V putIfAbsentAndGet(K key, Supplier<V> action) {
        return putIfAbsentAndGet(key, new ExpiringCache<>(expiry, action));
    public @Nullable V putIfAbsentAndGet(K key, ExpiringCache<V> item) {
        putIfAbsent(key, item);
     * Puts a new value into the cache if the specified key is present.
     * @param key the key whose value in the cache is to be updated
    public void putValue(K key, @Nullable V value) {
        final ExpiringCache<@Nullable V> item = items.get(key);
            throw new IllegalArgumentException(String.format("No item found for key '%s' .", key));
            item.putValue(value);
     * @return true if the cache contains a value for the specified key
    public boolean containsKey(K key) {
        return items.containsKey(key);
     * Removes the item associated with the given key from the cache.
     * @param key the key whose associated value is to be removed
        items.remove(key);
     * Discards all items from the cache.
     * Returns a set of all keys.
     * @return the set of all keys
    public synchronized Set<K> keys() {
        return new LinkedHashSet<>(items.keySet());
     * Returns the value associated with the given key - possibly from the cache, if it is still valid.
     * @param key the key whose associated value is to be returned
     * @return the value associated with the given key, or null if there is no cached value for the given key
            logger.debug("No item for key '{}' found", key);
            return item.getValue();
     * Returns a collection of all values - possibly from the cache, if they are still valid.
     * @return the collection of all values
    public synchronized Collection<@Nullable V> values() {
        final Collection<@Nullable V> values = new LinkedList<>();
        for (final ExpiringCache<@Nullable V> item : items.values()) {
            values.add(item.getValue());
     * Invalidates the value associated with the given key in the cache.
     * @param key the key whose associated value is to be invalidated
    public synchronized void invalidate(K key) {
            item.invalidateValue();
     * Invalidates all values in the cache.
    public synchronized void invalidateAll() {
        items.values().forEach(ExpiringCache::invalidateValue);
     * Refreshes and returns the value associated with the given key in the cache.
     * @param key the key whose associated value is to be refreshed
    public synchronized @Nullable V refresh(K key) {
            return item.refreshValue();
     * Refreshes and returns a collection of all new values in the cache.
    public synchronized Collection<@Nullable V> refreshAll() {
            values.add(item.refreshValue());
