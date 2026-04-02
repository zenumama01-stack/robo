 * A {@link Provider} provides elements of a determined type and the subinterfaces
 * are registered as OSGi services. Providers are tracked by {@link Registry} services, which collect all elements from
 * different providers of the same
public interface Provider<@NonNull E> {
     * Adds a {@link ProviderChangeListener} which must be notified if there are
     * changes concerning the elements provided by the {@link Provider}.
    void addProviderChangeListener(ProviderChangeListener<E> listener);
     * Returns a collection of all elements.
     * @return collection of all elements
    Collection<E> getAll();
     * Removes a {@link ProviderChangeListener}.
     * @param listener the listener to be removed.
    void removeProviderChangeListener(ProviderChangeListener<E> listener);
