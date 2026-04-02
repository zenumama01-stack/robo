 * The {@link PersistenceFilter} is the base class for implementing persistence filters.
public abstract class PersistenceFilter {
    public PersistenceFilter(final String name) {
     * Get the name of this filter
     * @return a unique name
     * Apply this filter to an item
     * @param item the item to check
     * @return true if the filter allows persisting this value
    public abstract boolean apply(Item item);
     * Notify filter that item was persisted
     * @param item the persisted item
    public abstract void persisted(Item item);
        result = prime * result + name.hashCode();
    public boolean equals(final @Nullable Object obj) {
        if (!(obj instanceof PersistenceFilter)) {
        final PersistenceFilter other = (PersistenceFilter) obj;
        return Objects.equals(name, other.name);
        return String.format("%s [name=%s]", getClass().getSimpleName(), name);
