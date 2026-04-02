 * {@link ProviderChangeListener} can be added to {@link Provider} services, to
 * listen for changes. The {@link AbstractRegistry} implements a {@link ProviderChangeListener} and subscribes itself to
 * every added {@link Provider}.
 * @param <E> type of the element from the provider
public interface ProviderChangeListener<@NonNull E> {
    void added(Provider<E> provider, E element);
    void removed(Provider<E> provider, E element);
     * @param element the element that has been updated
    void updated(Provider<E> provider, E oldelement, E element);
