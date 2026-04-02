 * A persistence service which can be used to store data from openHAB.
 * This must not necessarily be a local database, a persistence service
 * can also be cloud-based or a simply data-export facility (e.g.
 * for sending data to an IoT (Internet of Things) service.
public interface PersistenceService {
     * Returns the id of this {@link PersistenceService}.
     * This id is used to uniquely identify the {@link PersistenceService}.
     * @return the id to uniquely identify the {@link PersistenceService}.
     * Returns the label of this {@link PersistenceService}.
     * This label provides a user friendly name for the {@link PersistenceService}.
     * @param locale the language to return the label in, or null for the default language
     * @return the label of the {@link PersistenceService}.
     * Stores the current value of the given item.
     * @param item the item which state should be persisted.
    void store(Item item);
     * Stores the current value of the given item under a specified alias.
     * @param alias the alias under which the item should be persisted.
    void store(Item item, @Nullable String alias);
     * Provides default persistence strategies that are used for all items if no user defined configuration is found.
     * This method has been deprecated and {@link #getSuggestedStrategies()} should be used instead. These
     * persistence strategies are no longer applied automatically.
     * @return The suggested persistence strategies
    default List<PersistenceStrategy> getDefaultStrategies() {
     * Provides suggested persistence strategies that can be used in the UI as a suggestion for configuration.
    default List<PersistenceStrategy> getSuggestedStrategies() {
        return getDefaultStrategies();
