 * {@link RegistryChangeListener} can be added to {@link Registry} services, to
 * listen for changes.
 * @param <E> type of the element in the registry
public interface RegistryChangeListener<E> {
    void added(E element);
    void removed(E element);
    void updated(E oldElement, E element);
