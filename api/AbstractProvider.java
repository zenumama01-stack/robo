 * {@link AbstractProvider} can be used as base class for {@link Provider} implementations. It supports the registration
 * and notification of listeners.
 *            type of the provided elements
public abstract class AbstractProvider<@NonNull E> implements Provider<E> {
    private Logger logger = LoggerFactory.getLogger(AbstractProvider.class);
    protected List<ProviderChangeListener<E>> listeners = new CopyOnWriteArrayList<>();
    private void notifyListeners(@Nullable E oldElement, E element, EventType eventType) {
        for (ProviderChangeListener<E> listener : this.listeners) {
                        listener.added(this, element);
                        listener.updated(this, oldElement != null ? oldElement : element, element);
                logger.error("Could not inform the listener '{}' about the '{}' event!: {}", listener, eventType.name(),
    private void notifyListeners(E element, EventType eventType) {
        notifyListeners(null, element, eventType);
    protected void notifyListenersAboutAddedElement(E element) {
        notifyListeners(element, EventType.ADDED);
    protected void notifyListenersAboutRemovedElement(E element) {
        notifyListeners(element, EventType.REMOVED);
    protected void notifyListenersAboutUpdatedElement(E oldElement, E element) {
        notifyListeners(oldElement, element, EventType.UPDATED);
