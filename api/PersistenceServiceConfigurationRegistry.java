 * The {@link PersistenceServiceConfigurationRegistry} is the central place to store persistence service configurations.
 * Configurations are registered through {@link PersistenceServiceConfigurationProvider}.
 * Because the {@link org.openhab.core.persistence.internal.PersistenceManagerImpl} implementation needs to listen to
 * different registries, the {@link PersistenceServiceConfigurationRegistryChangeListener} can be used to add listeners
 * to this registry.
public interface PersistenceServiceConfigurationRegistry extends Registry<PersistenceServiceConfiguration, String> {
    void addRegistryChangeListener(PersistenceServiceConfigurationRegistryChangeListener listener);
    void removeRegistryChangeListener(PersistenceServiceConfigurationRegistryChangeListener listener);
     * Returns a list of persistence services that have configurations defined by multiple providers, e.g. a DSL
     * provider and a managed provider.
     * @return a list of persistence identifiers of currently detected configuration conflicts, empty if no conflicts
    List<String> getServiceConfigurationConflicts();
