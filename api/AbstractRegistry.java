import java.util.function.BiConsumer;
 * The {@link AbstractRegistry} is an abstract implementation of the {@link Registry} interface, that can be used as
 * base class for {@link Registry} implementations.
 * @author Stefan Bußweiler - Migration to new event mechanism
 * @author Victor Toni - provide elements as {@link Stream}
 * @author Kai Kreuzer - switched to parameterized logging
 * @author Hilbrand Bouwkamp - Made protected fields private and added new methods to give access.
 * @author Chris Jackson - Ensure managed provider is only unset by current provider
 * @param <E> type of the element
 * @param <K> type of the key
 * @param <P> type of the provider
public abstract class AbstractRegistry<@NonNull E extends Identifiable<K>, @NonNull K, @NonNull P extends Provider<E>>
        implements ProviderChangeListener<E>, Registry<E, K> {
    private final Logger logger = LoggerFactory.getLogger(AbstractRegistry.class);
    private final @Nullable Class<P> providerClazz;
    private @Nullable CompletableFuture<ServiceTracker<P, P>> providerTrackerFuture;
    private final ReentrantReadWriteLock elementLock = new ReentrantReadWriteLock();
    private final ReentrantReadWriteLock.ReadLock elementReadLock = elementLock.readLock();
    private final ReentrantReadWriteLock.WriteLock elementWriteLock = elementLock.writeLock();
    private final Map<Provider<E>, Collection<E>> providerToElements = new HashMap<>();
    private final Map<E, Provider<E>> elementToProvider = new HashMap<>();
    private final Map<K, E> identifierToElement = new HashMap<>();
    private final Set<E> elements = new HashSet<>();
    private final Collection<RegistryChangeListener<E>> listeners = new CopyOnWriteArraySet<>();
    private @Nullable ManagedProvider<E, K> managedProvider = null;
    private @Nullable ReadyService readyService;
     * @param providerClazz the class of the providers (see e.g. {@link AbstractRegistry#addProvider(Provider)}), null
     *            if no providers should be tracked automatically after activation
    protected AbstractRegistry(final @Nullable Class<P> providerClazz) {
        this.providerClazz = providerClazz;
         * The handlers for 'add' and 'remove' the services implementing the provider class (cardinality is
         * multiple) rely on an active component.
         * To grant that the add and remove functions are called only for an active component, we use a provider
         * tracker.
         * Do not open the tracker in the activation method itself as this could lead to circular dependency errors.
        if (providerClazz != null) {
            Class<P> providerClazz = this.providerClazz;
            providerTrackerFuture = CompletableFuture.supplyAsync(() -> {
                ServiceTracker<P, P> providerTracker = new ProviderTracker(context, providerClazz);
                providerTracker.open();
                return providerTracker;
        CompletableFuture<ServiceTracker<P, P>> future = providerTrackerFuture;
            future.join().close();
    public void waitForCompletedAsyncActivationTasks() {
            future.join();
    private final class ProviderTracker extends ServiceTracker<P, P> {
         * @param context the bundle context to lookup services
         * @param providerClazz the class that implementing services should be tracked
        public ProviderTracker(final BundleContext context, final Class<P> providerClazz) {
            super(context, providerClazz.getName(), null);
        public P addingService(@Nullable ServiceReference<P> reference) {
            final P service = context.getService(reference);
            addProvider(service);
        public void removedService(@Nullable ServiceReference<P> reference, P service) {
            removeProvider(service);
    public void added(Provider<E> provider, E element) {
        elementWriteLock.lock();
            final Collection<E> providerElements = providerToElements.get(provider);
            if (providerElements == null) {
                logger.debug("Cannot add \"{}\" with key \"{}\". Provider \"{}\" unknown.",
                        element.getClass().getSimpleName(), element.getUID(), provider.getClass().getSimpleName());
            if (!added(provider, element, providerElements)) {
            elementWriteLock.unlock();
     * Handle an element that has been added for a provider.
     * This method must only be called if the write lock for elements has been locked!
     * @param provider the provider that provides the element
     * @param providerElements the collection that holds the elements of the provider
     * @return indication if the element has been added
    private boolean added(Provider<E> provider, E element, Collection<E> providerElements) {
        final K uid = element.getUID();
        E existingElement = identifierToElement.get(uid);
        if (existingElement != null) {
            Provider<E> existingElementProvider = elementToProvider.get(existingElement);
            String elementClassName = element.getClass().getSimpleName();
            if ("ActionType".equals(elementClassName) || "Metadata".equals(elementClassName)) {
                        "Cannot add \"{}\" with key \"{}\". It exists already from provider \"{}\"! Failed to add a second with the same UID from provider \"{}\"!",
                        elementClassName, uid,
                        existingElementProvider != null ? existingElementProvider.getClass().getSimpleName() : null,
                        provider.getClass().getSimpleName());
            onAddElement(element);
            logger.warn("Cannot add \"{}\" with key \"{}\": {}", element.getClass().getSimpleName(), uid,
                    ex.getMessage(), logger.isDebugEnabled() ? ex : null);
        identifierToElement.put(element.getUID(), element);
        elementToProvider.put(element, provider);
        providerElements.add(element);
        elements.add(element);
    public void addRegistryChangeListener(RegistryChangeListener<E> listener) {
        elementReadLock.lock();
            return new HashSet<>(elements);
            elementReadLock.unlock();
    public Stream<E> stream() {
        return getAll().stream();
    public void removed(Provider<E> provider, E element) {
        final @Nullable E existingElement;
            // The given "element" might not be the live instance but loaded from storage.
            // Use the identifier to operate on the "real" element.
            existingElement = identifierToElement.get(uid);
            if (existingElement == null) {
                logger.debug("Cannot remove \"{}\" with key \"{}\" from provider \"{}\" because it does not exist!",
                        element.getClass().getSimpleName(), uid, provider.getClass().getSimpleName());
            Provider<E> elementProvider = elementToProvider.get(existingElement);
            if (elementProvider != null && !elementProvider.equals(provider)) {
                        "Provider '{}' is not allowed to remove element '{}' with key '{}' from the registry because it was added by provider '{}'.",
                        provider.getClass().getSimpleName(), element.getClass().getSimpleName(), uid,
                        elementProvider.getClass().getSimpleName());
                onRemoveElement(existingElement);
                logger.warn("Cannot remove \"{}\" with key \"{}\": {}", element.getClass().getSimpleName(), uid,
            identifierToElement.remove(uid);
            elementToProvider.remove(existingElement);
            Collection<E> providerElements = providerToElements.get(provider);
            if (providerElements != null) {
                providerElements.remove(existingElement);
            elements.remove(existingElement);
        notifyListenersAboutRemovedElement(existingElement);
    public void removeRegistryChangeListener(RegistryChangeListener<E> listener) {
    public void updated(Provider<E> provider, E oldElement, E element) {
        final K uidOld = oldElement.getUID();
        if (!uidOld.equals(uid)) {
            logger.debug("Received update event for elements that UID differ (old: \"{}\", new: \"{}\"). Ignore event.",
                    uidOld, uid);
                logger.debug("Cannot update \"{}\" with key \"{}\" for provider \"{}\" because it does not exist!",
                beforeUpdateElement(existingElement);
                onUpdateElement(oldElement, element);
                logger.warn("Cannot update \"{}\" with key \"{}\": {}", element.getClass().getSimpleName(), uid,
            identifierToElement.put(uid, element);
            return identifierToElement.get(key);
     * This method retrieves an Entry with the provider and the element for the key from the registry.
     * @param key key of the element
     * @return provider and element entry or null if no element was found
    protected @Nullable Entry<Provider<E>, E> getValueAndProvider(K key) {
            final @Nullable E element = identifierToElement.get(key);
            final Provider<E> provider = elementToProvider.get(element);
            return element == null || provider == null ? null : Map.entry(provider, element);
    public E add(E element) {
        ManagedProvider<E, K> mp = managedProvider;
        if (mp == null) {
        mp.add(element);
        return mp.update(element);
        return mp.remove(key);
    protected void notifyListeners(E element, EventType eventType) {
        for (RegistryChangeListener<E> listener : this.listeners) {
                        listener.added(element);
                        listener.removed(element);
            } catch (Throwable throwable) {
                logger.error("Cannot inform the listener \"{}\" about the \"{}\" event: {}", listener, eventType.name(),
                        throwable.getMessage(), throwable);
    protected void notifyListeners(E oldElement, E element, EventType eventType) {
                        listener.updated(oldElement, element);
    protected void addProvider(Provider<E> provider) {
        final Collection<E> elementsOfAddedProvider = provider.getAll();
        final Collection<E> elementsAdded = new HashSet<>(elementsOfAddedProvider.size());
            if (providerToElements.get(provider) != null) {
                logger.warn("Cannot add provider \"{}\" because it already exists.",
            provider.addProviderChangeListener(this);
            final HashSet<E> providerElements = new HashSet<>();
            providerToElements.put(provider, providerElements);
            for (E element : elementsOfAddedProvider) {
                if (added(provider, element, providerElements)) {
                    elementsAdded.add(element);
        elementsAdded.forEach(this::notifyListenersAboutAddedElement);
        if (provider instanceof ManagedProvider && providerClazz instanceof Class clazz
                && readyService instanceof ReadyService rs) {
            rs.markReady(new ReadyMarker("managed", clazz.getSimpleName().replace("Provider", "").toLowerCase()));
        logger.debug("Provider \"{}\" has been added.", provider.getClass().getName());
     * This method retrieves the provider of an element from the registry.
     * @return provider or null if no provider was found
    protected @Nullable Provider<E> getProvider(K key) {
            return element == null ? null : elementToProvider.get(element);
     * @param element the element
    public @Nullable Provider<E> getProvider(E element) {
            return elementToProvider.get(element);
     * This method traverses over all elements of a provider in the registry and calls the consumer with each element.
     * The traversal over the elements is done while holding a lock for the respective internal collections.
     * If you use this method, please ensure not execution time consuming stuff as it will block any other usage of that
     * collections.
     * You should also not call third party code that could e.g. access the registry itself again. This could lead to a
     * dead lock and hard finding bugs.
     * The {@link #getAll()} and {@link #stream()} method will operate on a copy and so no lock is hold.
     * @param provider provider to traverse elements of
     * @param consumer function to call with element
    protected void forEach(Provider<E> provider, Consumer<E> consumer) {
                providerElements.forEach(consumer);
     * This method traverses over all elements in the registry and calls the consumer with each element.
    protected void forEach(Consumer<E> consumer) {
            elements.forEach(consumer);
     * This method traverses over all elements in the registry and calls the consumer with the provider of the
     * element as the first parameter and the element as the second argument.
     * @param consumer function to call with the provider and element
    protected void forEach(BiConsumer<Provider<E>, E> consumer) {
            for (final Entry<Provider<E>, Collection<E>> providerEntries : providerToElements.entrySet()) {
                final Provider<E> provider = providerEntries.getKey();
                providerEntries.getValue().forEach(element -> consumer.accept(provider, element));
    protected @Nullable ManagedProvider<E, K> getManagedProvider() {
        return managedProvider;
    protected void setManagedProvider(ManagedProvider<E, K> provider) {
        managedProvider = provider;
    protected void unsetManagedProvider(ManagedProvider<E, K> provider) {
        if (managedProvider != null && managedProvider.equals(provider)) {
            managedProvider = null;
     * This method is called before an element is added. The implementing class
     * can override this method to perform initialization logic or check the
     * validity of the element.
     * To keep custom logic on the inheritance chain, you must call always the super implementation first.
     * If the method throws an {@link IllegalArgumentException} the element will not be added.
     * @param element element to be added
     * @throws IllegalArgumentException if the element is invalid and should not be added
    protected void onAddElement(E element) throws IllegalArgumentException {
        // can be overridden by sub classes
     * This method is called before an element is removed. The implementing
     * class can override this method to perform specific logic.
     * @param element element to be removed
    protected void onRemoveElement(E element) {
     * This method is called before an element is updated. The implementing
     * @param existingElement the previously existing element (as held in the element cache)
    protected void beforeUpdateElement(E existingElement) {
     * class can override this method to perform specific logic or check the
     * validity of the updated element.
     * @param oldElement old element (before update, as given by the provider)
     * @param element updated element (after update)
     *            <p>
     *            If the method throws an {@link IllegalArgumentException} the element will not be updated.
     * @throws IllegalArgumentException if the updated element is invalid and should not be updated
    protected void onUpdateElement(E oldElement, E element) throws IllegalArgumentException {
    protected void removeProvider(Provider<E> provider) {
        final Collection<E> removedElements = new LinkedList<>();
            final Collection<E> providerElements = providerToElements.remove(provider);
                logger.warn("Cannot remove provider \"{}\" because it is unknown.",
            for (final E element : providerElements) {
                    onRemoveElement(element);
                            "Removal of \"{}\" with key \"{}\" should be prevented but we need to remove the element as the provider \"{}\" is gone: {}",
                            element.getClass().getSimpleName(), element.getUID(), provider.getClass().getSimpleName(),
                removedElements.add(element);
                elements.remove(element);
                elementToProvider.remove(element);
                identifierToElement.remove(element.getUID());
        removedElements.forEach(this::notifyListenersAboutRemovedElement);
        provider.removeProviderChangeListener(this);
        logger.debug("Provider \"{}\" has been removed.", provider.getClass().getSimpleName());
    protected @Nullable EventPublisher getEventPublisher() {
        return this.eventPublisher;
        this.readyService = null;
     * This method can be used in a subclass in order to post events through the openHAB events bus. A common
     * use case is to notify event subscribers about an element which has been added/removed/updated to the registry.
        if (eventPublisher instanceof EventPublisher ep) {
                logger.error("Cannot post event of type \"{}\".", event.getType(), ex);
