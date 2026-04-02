 * The {@link ManagedProvider} is a specific {@link Provider} that enables to
 * add, remove and update elements at runtime.
public interface ManagedProvider<@NonNull E extends Identifiable<K>, @NonNull K> extends Provider<E> {
     * Adds an element.
    void add(E element);
     * Removes an element and returns the removed element.
     * @param key key of the element that should be removed
     * @return element that was removed, or null if no element with the given
     *         key exists
    E remove(K key);
     * Updates an element.
     * @param element element to be updated
     * @return returns the old element or null if no element with the same key
     *         exists
    E update(E element);
     * Returns an element for the given key or null if no element for the given
     * key exists.
     * @return returns element or null, if no element for the given key exists
    E get(K key);
