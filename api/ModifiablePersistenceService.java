 * This class provides an interface to the a {@link PersistenceService} to allow data to be stored
 * at a specific time. This allows bindings that interface to devices that store data internally,
 * and then periodically provide it to the server to be accommodated.
public interface ModifiablePersistenceService extends QueryablePersistenceService {
     * Stores the historic item value. This allows the item, time and value to be specified.
     * Adding data with the same time as an existing record should update the current record value rather than adding a
     * new record.
     * Implementors should keep in mind that all registered {@link PersistenceService}s are called synchronously. Hence
     * long running operations should be processed asynchronously. E.g. <code>store</code> adds things to a queue which
     * is processed by some asynchronous workers (Quartz Job, Thread, etc.).
     * @param item the data to be stored
     * @param date the date of the record
     * @param state the state to be recorded
    void store(Item item, ZonedDateTime date, State state);
     * Stores the historic item value under a specified alias. This allows the item, time and value to be specified.
    void store(Item item, ZonedDateTime date, State state, @Nullable String alias);
     * Removes data associated with an item from a persistence service.
     * If all data is removed for the specified item, the persistence service should free any resources associated with
     * the item (e.g. remove any tables or delete files from the storage).
     * If the persistence service implementing this method supports aliases for item names, the default implementation
     * of {@link #remove(FilterCriteria, String)} should be overriden as well.
     * @param filter the filter to apply to the data removal. ItemName can not be null.
     * @return true if the query executed successfully
     * @throws IllegalArgumentException if item name is null.
    boolean remove(FilterCriteria filter) throws IllegalArgumentException;
     * Persistence services supporting aliases, and relying on lookups in the item registry, should override the default
     * implementation from this interface.
     * @param alias for item name in database
    default boolean remove(FilterCriteria filter, @Nullable String alias) throws IllegalArgumentException {
        // Default implementation changes the filter to have the alias as itemName.
        // This gives correct results as long as the persistence service does not rely on a lookup in the item registry
        // (in which case the item will not be found).
            FilterCriteria aliasFilter = new FilterCriteria(filter).setItemName(alias);
            return remove(aliasFilter);
        return remove(filter);
