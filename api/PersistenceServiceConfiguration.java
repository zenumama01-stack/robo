 * The {@link PersistenceServiceConfiguration} represents the configuration for a persistence service.
public class PersistenceServiceConfiguration implements Identifiable<String> {
    private final List<PersistenceItemConfiguration> configs;
    private final Map<String, String> aliases;
    private final List<PersistenceStrategy> strategies;
    private final List<PersistenceFilter> filters;
    public PersistenceServiceConfiguration(String serviceId, Collection<PersistenceItemConfiguration> configs,
            Map<String, String> aliases, Collection<PersistenceStrategy> strategies,
            Collection<PersistenceFilter> filters) {
        this.configs = List.copyOf(configs);
        this.aliases = Map.copyOf(aliases);
        this.strategies = List.copyOf(strategies);
        this.filters = List.copyOf(filters);
     * Get the item configurations.
     * @return an unmodifiable list of the item configurations
    public List<PersistenceItemConfiguration> getConfigs() {
        return configs;
     * Get the item aliases.
     * @return a map of items to aliases
    public Map<String, String> getAliases() {
        return aliases;
     * Get all defined strategies.
     * @return an unmodifiable list of the defined strategies
    public List<PersistenceStrategy> getStrategies() {
     * Get all defined filters.
     * @return an unmodifiable list of the defined filters
    public List<PersistenceFilter> getFilters() {
