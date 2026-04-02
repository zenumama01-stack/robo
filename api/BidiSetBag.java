package org.openhab.core.automation.module.script.rulesupport.internal.loader;
import java.util.concurrent.locks.ReentrantReadWriteLock;
 * Bidirectional bag of unique elements. A map allowing multiple, unique values to be stored against a single key.
 * Provides optimized lookup of values for a key, as well as keys referencing a value.
 * @author Jonathan Gilbert - Initial contribution
 * @author Jan N. Klug - Make implementation thread-safe
 * @param <K> Type of Key
 * @param <V> Type of Value
public class BidiSetBag<K, V> {
    private final ReentrantReadWriteLock lock = new ReentrantReadWriteLock();
    private final Map<K, Set<V>> keyToValues = new HashMap<>();
    private final Map<V, Set<K>> valueToKeys = new HashMap<>();
    public void put(K key, V value) {
        lock.writeLock().lock();
            keyToValues.computeIfAbsent(key, k -> new HashSet<>()).add(value);
            valueToKeys.computeIfAbsent(value, v -> new HashSet<>()).add(key);
            lock.writeLock().unlock();
    public Set<V> getValues(K key) {
        lock.readLock().lock();
            Set<V> values = keyToValues.getOrDefault(key, Set.of());
            return Collections.unmodifiableSet(values);
            lock.readLock().unlock();
    public Set<K> getKeys(V value) {
            Set<K> keys = valueToKeys.getOrDefault(value, Set.of());
            return Collections.unmodifiableSet(keys);
    public Set<V> removeKey(K key) {
            Set<V> values = keyToValues.remove(key);
                for (V value : values) {
                    valueToKeys.computeIfPresent(value, (k, v) -> {
                        v.remove(key);
    public Set<K> removeValue(V value) {
            Set<K> keys = valueToKeys.remove(value);
            if (keys != null) {
                for (K key : keys) {
                    keyToValues.computeIfPresent(key, (k, v) -> {
                        v.remove(value);
