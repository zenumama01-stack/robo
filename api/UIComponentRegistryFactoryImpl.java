 * Implementation for a {@link UIComponentRegistryFactory} using a set of {@link ManagedUIComponentProvider}.
 * @author Łukasz Dywicki - Removed explicit dependency on storage providers.
 * @author Jonathan Gilbert - Made providers' collections immutable.
@Component(service = UIComponentRegistryFactory.class, immediate = true)
public class UIComponentRegistryFactoryImpl implements UIComponentRegistryFactory {
    private final Logger logger = LoggerFactory.getLogger(UIComponentRegistryFactoryImpl.class);
    private final ComponentFactory<ManagedUIComponentProvider> providerFactory;
    private final BundleContext bc;
    Map<String, UIComponentRegistryImpl> registries = new ConcurrentHashMap<>();
    Set<ComponentInstance<ManagedUIComponentProvider>> createdProviders = new CopyOnWriteArraySet<>();
    Map<String, Set<UIComponentProvider>> providers = new ConcurrentHashMap<>();
    public UIComponentRegistryFactoryImpl(
            @Reference(target = "(component.factory=org.openhab.core.ui.component.provider.factory)") ComponentFactory<ManagedUIComponentProvider> factory,
            BundleContext bc) {
        this.providerFactory = factory;
        this.bc = bc;
    public UIComponentRegistryImpl getRegistry(String namespace) {
        UIComponentRegistryImpl registry = registries.get(namespace);
            if (!managedProviderAvailable(namespace)) {
                logger.debug("Creating managed provider for '{}'", namespace);
                properties.put(UIComponentProvider.CONFIG_NAMESPACE, namespace);
                ComponentInstance<ManagedUIComponentProvider> instance = this.providerFactory.newInstance(properties);
                createdProviders.add(instance);
            Set<UIComponentProvider> namespaceProviders = this.providers.get(namespace);
            registry = new UIComponentRegistryImpl(namespace, namespaceProviders);
            registries.put(namespace, registry);
        createdProviders.forEach(ComponentInstance::dispose);
    private boolean managedProviderAvailable(String namespace) {
            return bc.getServiceReferences(UIComponentProvider.class, null).stream().map(bc::getService)
                    .anyMatch(s -> namespace.equals(s.getNamespace()) && s instanceof ManagedProvider<?, ?>);
    void addProvider(UIComponentProvider provider) {
        UIComponentRegistryImpl registry = registries.get(provider.getNamespace());
            registry.addProvider(provider);
        registerProvider(provider);
    void removeProvider(UIComponentProvider provider) {
            registry.removeProvider(provider);
        unregisterProvider(provider);
    private void registerProvider(UIComponentProvider provider) {
        Set<UIComponentProvider> existing = providers.get(provider.getNamespace());
        if (existing == null) {
            existing = Set.of();
        Set<UIComponentProvider> updated = new HashSet<>(existing);
        updated.add(provider);
        providers.put(provider.getNamespace(), Set.copyOf(updated));
    private void unregisterProvider(UIComponentProvider provider) {
        if (existing != null && !existing.isEmpty()) {
            updated.remove(provider);
