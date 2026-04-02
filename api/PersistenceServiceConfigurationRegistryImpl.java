 * The {@link PersistenceServiceConfigurationRegistryImpl} implements the
 * {@link PersistenceServiceConfigurationRegistry}
@Component(immediate = true, service = PersistenceServiceConfigurationRegistry.class)
public class PersistenceServiceConfigurationRegistryImpl
        extends AbstractRegistry<PersistenceServiceConfiguration, String, PersistenceServiceConfigurationProvider>
        implements PersistenceServiceConfigurationRegistry {
    private final Logger logger = LoggerFactory.getLogger(PersistenceServiceConfigurationRegistryImpl.class);
    private final Map<String, Provider<PersistenceServiceConfiguration>> serviceToProvider = new ConcurrentHashMap<>();
    private final Map<String, Set<Provider<PersistenceServiceConfiguration>>> serviceToAllProviders = new ConcurrentHashMap<>();
    private final Set<PersistenceServiceConfigurationRegistryChangeListener> registryChangeListeners = new CopyOnWriteArraySet<>();
    public PersistenceServiceConfigurationRegistryImpl() {
        super(PersistenceServiceConfigurationProvider.class);
    public void added(Provider<PersistenceServiceConfiguration> provider, PersistenceServiceConfiguration element) {
        String elementUID = element.getUID();
        Set<Provider<PersistenceServiceConfiguration>> providers = serviceToAllProviders.getOrDefault(elementUID,
                new CopyOnWriteArraySet<>());
        providers.add(provider);
        serviceToAllProviders.put(elementUID, providers);
        Provider<PersistenceServiceConfiguration> existingProvider = serviceToProvider.get(elementUID);
        if (existingProvider != null) {
            String newProvider = provider instanceof ManagedPersistenceServiceConfigurationProvider ? "managed" : "DSL";
            String oldProvider = existingProvider instanceof ManagedPersistenceServiceConfigurationProvider ? "managed"
                    : "DSL";
                    "Tried to add strategy container with serviceId '{}' from {} provider, but it was already added before from {} provider.",
                    elementUID, newProvider, oldProvider);
            serviceToProvider.put(elementUID, provider);
    public void removed(Provider<PersistenceServiceConfiguration> provider, PersistenceServiceConfiguration element) {
        Set<Provider<PersistenceServiceConfiguration>> providers = serviceToAllProviders.get(elementUID);
        if (providers != null) {
            providers.remove(provider);
            if (providers.isEmpty()) {
                serviceToAllProviders.remove(elementUID);
        if (!provider.equals(serviceToProvider.getOrDefault(elementUID, provider))) {
            logger.warn("Tried to remove strategy container with serviceId '{}', but it was added by another provider.",
                    element.getUID());
            super.removed(provider, element);
            if (providers != null && !providers.isEmpty()) {
                Provider<PersistenceServiceConfiguration> alternateProvider = providers.stream().findAny().get();
                PersistenceServiceConfiguration alternateElement = alternateProvider.getAll().stream()
                        .filter(e -> elementUID.equals(e.getUID())).findAny().orElse(null);
                if (alternateElement != null) {
                    super.added(alternateProvider, alternateElement);
                    serviceToProvider.put(elementUID, alternateProvider);
                serviceToProvider.remove(elementUID);
    public void updated(Provider<PersistenceServiceConfiguration> provider, PersistenceServiceConfiguration oldelement,
            PersistenceServiceConfiguration element) {
        if (!provider.equals(serviceToProvider.getOrDefault(element.getUID(), provider))) {
            logger.warn("Tried to update strategy container with serviceId '{}', but it was added by another provider.",
            super.updated(provider, oldelement, element);
    protected void notifyListenersAboutAddedElement(PersistenceServiceConfiguration element) {
        registryChangeListeners.forEach(listener -> listener.added(element));
    protected void notifyListenersAboutRemovedElement(PersistenceServiceConfiguration element) {
        registryChangeListeners.forEach(listener -> listener.removed(element));
    protected void notifyListenersAboutUpdatedElement(PersistenceServiceConfiguration oldElement,
        registryChangeListeners.forEach(listener -> listener.updated(oldElement, element));
    public void addRegistryChangeListener(PersistenceServiceConfigurationRegistryChangeListener listener) {
        registryChangeListeners.add(listener);
    public void removeRegistryChangeListener(PersistenceServiceConfigurationRegistryChangeListener listener) {
        registryChangeListeners.remove(listener);
    protected void setManagedProvider(ManagedPersistenceServiceConfigurationProvider provider) {
        super.setManagedProvider(provider);
    protected void unsetManagedProvider(ManagedPersistenceServiceConfigurationProvider provider) {
        super.unsetManagedProvider(provider);
    public List<String> getServiceConfigurationConflicts() {
        return serviceToAllProviders.entrySet().stream().filter(entry -> entry.getValue().size() > 1)
                .map(entry -> entry.getKey()).toList();
