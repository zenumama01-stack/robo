 * This class holds the configuration of a persistence strategy for specific items.
 * @author Mark Herwege - Extract alias configuration
public record PersistenceItemConfiguration(List<PersistenceConfig> items, List<PersistenceStrategy> strategies,
        List<PersistenceFilter> filters) {
    public PersistenceItemConfiguration(final List<PersistenceConfig> items,
            @Nullable final List<PersistenceStrategy> strategies, @Nullable final List<PersistenceFilter> filters) {
        this.strategies = Objects.requireNonNullElse(strategies, List.of());
        this.filters = Objects.requireNonNullElse(filters, List.of());
