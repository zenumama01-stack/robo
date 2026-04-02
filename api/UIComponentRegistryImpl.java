 * Implementation of a {@link UIComponentRegistry} using a {@link ManagedUIComponentProvider}.
 * It is instantiated by the {@link UIComponentRegistryFactoryImpl}.
 * @author Łukasz Dywicki - Support for dynamic registration of providers
public class UIComponentRegistryImpl extends AbstractRegistry<RootUIComponent, String, ManagedUIComponentProvider>
        implements UIComponentRegistry {
     * Constructs a UI component registry for the specified namespace.
     * @param namespace UI components namespace of this registry
    public UIComponentRegistryImpl(String namespace, @Nullable Set<UIComponentProvider> providers) {
            for (Provider<RootUIComponent> provider : providers) {
    public void addProvider(Provider<RootUIComponent> provider) {
        if (getManagedProvider() == null && provider instanceof ManagedProvider) {
            setManagedProvider((ManagedProvider<RootUIComponent, String>) provider);
    public void removeProvider(Provider<RootUIComponent> provider) {
        if (getManagedProvider() != null && provider instanceof ManagedProvider) {
            unsetManagedProvider((ManagedProvider<RootUIComponent, String>) provider);
        super.removeProvider(provider);
