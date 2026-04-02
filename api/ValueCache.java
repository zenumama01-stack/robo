 * The {@link ValueCache} can be used by scripts to share information between subsequent runs of the same script or
 * between scripts (depending on implementation).
public interface ValueCache {
     * Add a new key-value-pair to the cache. If the key is already present, the old value is replaces by the new value.
     * @param key a string used as key
     * @param value an {@code Object} to store with the key
     * @return the old value associated with this key or {@code null} if key didn't exist
    Object put(String key, Object value);
     * Remove a key (and its associated value) from the cache
     * @param key the key to remove
     * @return the previously associated value to this key or {@code null} if key not present
    Object remove(String key);
     * @param key the key of the requested value
     * @return the value associated with the key or {@code null} if key not present
    Object get(String key);
     * Get a value from the cache or create a new key-value-pair from the given supplier
     * @param supplier a supplier that returns a non-null value to be used if the key was not present
     * @return the value associated with the key
    Object get(String key, Supplier<Object> supplier);
     * Attempts to compute a mapping for the specified key and its current mapped value
     * (or null if there is no current mapping).
     * See {@code java.util.Map.compute()} for details.
     * @param key the key of the requested value.
     * @param remappingFunction the remapping function to compute a value.
     * @return the new value associated with the specified key, or null if none
    Object compute(String key, BiFunction<String, @Nullable Object, @Nullable Object> remappingFunction);
