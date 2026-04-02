 * The {@link Registry} interface represents a registry for elements of the type
 * E. The concrete sub interfaces are registered as OSGi services.
 * @author Kai Kreuzer - added null annotations
 * @param <E> type of the elements in the registry
public interface Registry<@NonNull E extends Identifiable<K>, @NonNull K> {
     * Adds a {@link RegistryChangeListener} to the registry.
     * @param listener registry change listener
    void addRegistryChangeListener(RegistryChangeListener<E> listener);
     * Returns a collection of all elements in the registry.
     * @return collection of all elements in the registry
     * Returns a stream of all elements in the registry.
     * @return stream of all elements in the registry
    Stream<E> stream();
     * This method retrieves a single element from the registry.
     * @return element or null if no element was found
     * Removes a {@link RegistryChangeListener} from the registry.
    void removeRegistryChangeListener(RegistryChangeListener<E> listener);
     * Adds the given element to the according {@link ManagedProvider}.
     * @param element element to be added (must not be null)
     * @return the added element or newly created object of the same type
     * @throws IllegalStateException if no ManagedProvider is available
    E add(E element);
     * Updates the given element at the according {@link ManagedProvider}.
     * @param element element to be updated (must not be null)
     * Removes the given element from the according {@link ManagedProvider}.
     * @param key key of the element (must not be null)
