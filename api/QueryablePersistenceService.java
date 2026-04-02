 * A queryable persistence service which can be used to store and retrieve
 * data from openHAB. This is most likely some kind of database system.
 * @author Chris Jackson - Added getItems method
 * @author Mark Herwege - Added methods to retrieve lastUpdate, lastChange and lastState from persistence
public interface QueryablePersistenceService extends PersistenceService {
     * Queries the {@link PersistenceService} for historic data with a given {@link FilterCriteria}.
     * If the persistence service implementing this class supports using aliases for item names, the default
     * implementation of {@link #query(FilterCriteria, String)} should be overridden as well.
     * @param filter the filter to apply to the query
     * @return a time series of items
    Iterable<HistoricItem> query(FilterCriteria filter);
     * If the persistence service implementing this interface supports aliases and relies on item registry lookups, the
     * default implementation should be overridden to query the database with the aliased name.
    default Iterable<HistoricItem> query(FilterCriteria filter, @Nullable String alias) {
        // Default implementation changes the filter to have the alias as itemName and sets it back in the returned
        // result.
        String itemName = filter.getItemName();
        if (itemName != null && alias != null) {
            return StreamSupport.stream(query(aliasFilter).spliterator(), false).map(hi -> new HistoricItem() {
                public Instant getInstant() {
                    return hi.getInstant();
                    return hi.getTimestamp();
                    return hi.getState();
        return query(filter);
     * Returns a set of {@link PersistenceItemInfo} about items that are stored in the persistence service. This allows
     * the persistence service to return information about items that are no longer available as an
     * {@link org.openhab.core.items.Item} in openHAB. If it is not possible to retrieve the information or it would be
     * too expensive to do so an {@link UnsupportedOperationException} should be thrown.
     * Note that this method will return the names for items as stored in persistence. If aliases are used, the calling
     * method is responsible for mapping back to the real item name.
     * @return a set of information about the persisted items
     * @throws UnsupportedOperationException if the operation is not supported or would be too expensive to perform
    default Set<PersistenceItemInfo> getItemInfo() throws UnsupportedOperationException {
        throw new UnsupportedOperationException("getItemInfo not supported for persistence service");
     * Returns {@link PersistenceItemInfo} for an item stored in the persistence service. Null can be returned when the
     * item is not found in the persistence service. If it is not possible to retrieve the information or it would be
     * The default implementation will query the persistence service for the last value in the persistence store and
     * set the latest timestamp, leaving the {@link PersistenceItemInfo} earliest timestamp and count equal to null.
     * Note that the method should return the alias for the item in its response if an alias is being used.
     * @param itemName the real openHAB name as it appears in the item registry
     * @param alias for item name in database or null if no alias defined
     * @return information about the persisted item
    default @Nullable PersistenceItemInfo getItemInfo(String itemName, @Nullable String alias)
            throws UnsupportedOperationException {
        PersistedItem item = persistedItem(itemName, alias);
        Date latest = Date.from(item.getInstant());
        Integer count = null; // If we found the item, we do not know how many are in the store
                return alias != null ? alias : itemName;
                return latest;
     * Returns a {@link PersistedItem} representing the persisted state, last update and change timestamps and previous
     * persisted state. This can be used to restore the full state of an item.
     * The default implementation only queries the service for the last persisted state and does not attempt to look
     * further back to find the last change timestamp and previous persisted state. This avoids potential performance
     * issues if this method is not overriden in the specific persistence service. Persistence services should override
     * this default implementation with a more complete and efficient algorithm.
     * @param itemName name of item
     * @param alias alias of item
     * @return a {@link PersistedItem} or null if the item has not been persisted
    default @Nullable PersistedItem persistedItem(String itemName, @Nullable String alias) {
        State currentState;
        Instant lastUpdate;
        FilterCriteria filter = new FilterCriteria().setItemName(itemName).setEndDate(ZonedDateTime.now())
                .setOrdering(Ordering.DESCENDING).setPageSize(1).setPageNumber(0);
        Iterator<HistoricItem> it = query(filter, alias).iterator();
            currentState = historicItem.getState();
            lastUpdate = historicItem.getInstant();
        final State state = currentState;
        final Instant lastStateUpdate = lastUpdate;
        return new PersistedItem() {
                return lastStateUpdate;
                return lastStateUpdate.atZone(ZoneId.systemDefault());
            public @Nullable ZonedDateTime getLastStateChange() {
            public @Nullable State getLastState() {
