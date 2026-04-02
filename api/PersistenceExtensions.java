package org.openhab.core.persistence.extensions;
import java.math.MathContext;
import org.openhab.core.util.Statistics;
 * for using persistence services
 * @author Gaël L'hopital - Add deltaSince, lastUpdate, evolutionRate
 * @author Jan N. Klug - Added sumSince
 * @author John Cocula - Added sumSince
 * @author Jan N. Klug - Added interval methods and refactoring
 * @author Mark Herwege - Changed return types to State for some interval methods to also return unit
 * @author Mark Herwege - Extended for future dates
 * @author Mark Herwege - lastChange and nextChange methods
 * @author Mark Herwege - handle persisted GroupItem with QuantityType
 * @author Mark Herwege - add median methods
 * @author Mark Herwege - use item lastChange and lastUpdate methods if not in peristence
 * @author Mark Herwege - add Riemann sum methods
 * @author Jörg Sautter - use Instant instead of ZonedDateTime in Riemann sum methods
 * @author Mark Herwege - handle timeseries update
public class PersistenceExtensions {
    private static @Nullable PersistenceManager manager;
    private static @Nullable PersistenceServiceRegistry registry;
    private static @Nullable PersistenceServiceConfigurationRegistry configRegistry;
    private static @Nullable TimeZoneProvider timeZoneProvider;
    public static enum RiemannType {
        LEFT,
        MIDPOINT,
        RIGHT,
        TRAPEZOIDAL
    public PersistenceExtensions(@Reference PersistenceManager manager, @Reference PersistenceServiceRegistry registry,
            @Reference PersistenceServiceConfigurationRegistry configRegistry,
            @Reference TimeZoneProvider timeZoneProvider) {
        PersistenceExtensions.manager = manager;
        PersistenceExtensions.registry = registry;
        PersistenceExtensions.configRegistry = configRegistry;
        PersistenceExtensions.timeZoneProvider = timeZoneProvider;
     * Persists the state of a given <code>item</code> through the default persistence service.
     * @param item the item to store
    public static void persist(Item item) {
        internalPersist(item, null);
     * Persists the state of a given <code>item</code> through a {@link PersistenceService} identified
     * by the <code>serviceId</code>.
     * @param serviceId the name of the {@link PersistenceService} to use
    public static void persist(Item item, @Nullable String serviceId) {
        internalPersist(item, serviceId);
    private static void internalPersist(Item item, @Nullable String serviceId) {
        String effectiveServiceId = serviceId == null ? getDefaultServiceId() : serviceId;
        PersistenceService service = getService(effectiveServiceId);
            service.store(item, getAlias(item, effectiveServiceId));
                manager.handleExternalPersistenceDataChange(service, item);
        LoggerFactory.getLogger(PersistenceExtensions.class)
                .warn("There is no persistence service registered with the id '{}'", effectiveServiceId);
     * Persists a <code>state</code> at a given <code>timestamp</code> of an <code>item</code> through the default
     * persistence service.
     * @param timestamp the date for the item state to be stored
     * @param state the state to be stored
    public static void persist(Item item, ZonedDateTime timestamp, State state) {
        internalPersist(item, timestamp, state, null);
     * Persists a <code>state</code> at a given <code>timestamp</code> of an <code>item</code> through a
     * {@link PersistenceService} identified by the <code>serviceId</code>.
    public static void persist(Item item, ZonedDateTime timestamp, State state, @Nullable String serviceId) {
        internalPersist(item, timestamp, state, serviceId);
    private static void internalPersist(Item item, ZonedDateTime timestamp, State state, @Nullable String serviceId) {
        if (service instanceof ModifiablePersistenceService modifiableService) {
            modifiableService.store(item, timestamp, state, getAlias(item, effectiveServiceId));
                .warn("There is no modifiable persistence service registered with the id '{}'", effectiveServiceId);
     * @param stateString the state to be stored
    public static void persist(Item item, ZonedDateTime timestamp, String stateString) {
        internalPersist(item, timestamp, stateString, null);
    public static void persist(Item item, ZonedDateTime timestamp, String stateString, @Nullable String serviceId) {
        internalPersist(item, timestamp, stateString, serviceId);
    private static void internalPersist(Item item, ZonedDateTime timestamp, String stateString,
            @Nullable String serviceId) {
            LoggerFactory.getLogger(PersistenceExtensions.class).warn("State '{}' cannot be parsed for item '{}'.",
                    stateString, item.getName());
     * Persists a <code>timeSeries</code> of an <code>item</code> through the default persistence service.
     * @param timeSeries the timeSeries of states to be stored
    public static void persist(Item item, TimeSeries timeSeries) {
        internalPersist(item, timeSeries, null);
     * Persists a <code>timeSeries</code> of an <code>item</code> through a {@link PersistenceService} identified by the
     * <code>serviceId</code>.
    public static void persist(Item item, TimeSeries timeSeries, @Nullable String serviceId) {
        internalPersist(item, timeSeries, serviceId);
    private static void internalPersist(Item item, TimeSeries timeSeries, @Nullable String serviceId) {
        if (effectiveServiceId == null || timeSeries.size() == 0) {
        TimeZoneProvider tzProvider = timeZoneProvider;
        ZoneId timeZone = tzProvider != null ? tzProvider.getTimeZone() : ZoneId.systemDefault();
            if (timeSeries.getPolicy() == TimeSeries.Policy.REPLACE) {
                internalRemoveAllStatesBetween(item, timeSeries.getBegin().atZone(timeZone),
                        timeSeries.getEnd().atZone(timeZone), modifiableService, getAlias(item, effectiveServiceId));
            String alias = getAlias(item, effectiveServiceId);
            timeSeries.getStates()
                    .forEach(s -> modifiableService.store(item, s.timestamp().atZone(timeZone), s.state(), alias));
     * Retrieves the historic item for a given <code>item</code> at a certain point in time through the default
     * This method has been deprecated and {@link #persistedState(Item, ZonedDateTime)} should be used instead.
     * @param item the item for which to retrieve the historic item
     * @param timestamp the point in time for which the historic item should be retrieved
     * @return the historic item at the given point in time, or <code>null</code> if no historic item could be found,
     *         the default persistence service is not available or does not refer to a
     *         {@link QueryablePersistenceService}
    public static @Nullable HistoricItem historicState(Item item, ZonedDateTime timestamp) {
        LoggerFactory.getLogger(PersistenceExtensions.class).info(
                "The historicState method has been deprecated and will be removed in a future version, use persistedState instead.");
        return internalPersistedState(item, timestamp, null);
     * Retrieves the historic item for a given <code>item</code> at a certain point in time through a
     * This method has been deprecated and {@link #persistedState(Item, ZonedDateTime, String)} should be used instead.
     * @return the historic item at the given point in time, or <code>null</code> if no historic item could be found or
     *         if the provided <code>serviceId</code> does not refer to an available
    public static @Nullable HistoricItem historicState(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalPersistedState(item, timestamp, serviceId);
     * Retrieves the persisted item for a given <code>item</code> at a certain point in time through the default
     * @param item the item for which to retrieve the persisted item
     * @param timestamp the point in time for which the persisted item should be retrieved
     * @return the historic item at the given point in time, or <code>null</code> if no persisted item could be found,
    public static @Nullable HistoricItem persistedState(Item item, ZonedDateTime timestamp) {
     * Retrieves the persisted item for a given <code>item</code> at a certain point in time through a
     * @return the persisted item at the given point in time, or <code>null</code> if no persisted item could be found
     *         or if the provided <code>serviceId</code> does not refer to an available
    public static @Nullable HistoricItem persistedState(Item item, ZonedDateTime timestamp,
    private static @Nullable HistoricItem internalPersistedState(Item item, @Nullable ZonedDateTime timestamp,
        if (timestamp == null) {
        if (service instanceof QueryablePersistenceService qService) {
            filter.setEndDate(timestamp);
            filter.setItemName(item.getName());
            filter.setPageSize(1);
            filter.setOrdering(Ordering.DESCENDING);
            Iterable<HistoricItem> result = qService.query(filter, alias);
                return result.iterator().next();
                    .warn("There is no queryable persistence service registered with the id '{}'", effectiveServiceId);
     * Query the last historic update time of a given <code>item</code>. The default persistence service is used.
     * Note the {@link Item#getLastStateUpdate()} is generally preferred to get the last update time of an item.
     * @param item the item for which the last historic update time is to be returned
     * @return point in time of the last historic update to <code>item</code>, <code>null</code> if there are no
     *         historic persisted updates, the state has changed since the last update or the default persistence
     *         service is not available or not a {@link QueryablePersistenceService}
    public static @Nullable ZonedDateTime lastUpdate(Item item) {
        return internalAdjacentUpdate(item, false, null);
     * Query for the last historic update time of a given <code>item</code>.
     *         historic persisted updates, the state has changed since the last update or if persistence service given
     *         by <code>serviceId</code> does not refer to an available {@link QueryablePersistenceService}
    public static @Nullable ZonedDateTime lastUpdate(Item item, @Nullable String serviceId) {
        return internalAdjacentUpdate(item, false, serviceId);
     * Query the first future update time of a given <code>item</code>. The default persistence service is used.
     * @param item the item for which the first future update time is to be returned
     * @return point in time of the first future update to <code>item</code>, or <code>null</code> if there are no
     *         future persisted updates or the default persistence service is not available or not a
    public static @Nullable ZonedDateTime nextUpdate(Item item) {
        return internalAdjacentUpdate(item, true, null);
     * Query for the first future update time of a given <code>item</code>.
     *         future persisted updates or if persistence service given by <code>serviceId</code> does not refer to an
     *         available {@link QueryablePersistenceService}
    public static @Nullable ZonedDateTime nextUpdate(Item item, @Nullable String serviceId) {
        return internalAdjacentUpdate(item, true, serviceId);
    private static @Nullable ZonedDateTime internalAdjacentUpdate(Item item, boolean forward,
        return internalAdjacent(item, forward, false, serviceId);
     * Query the last historic change time of a given <code>item</code>. The default persistence service is used.
     * Note the {@link Item#getLastStateChange()} is generally preferred to get the last state change time of an item.
     * @param item the item for which the last historic change time is to be returned
     * @return point in time of the last historic change to <code>item</code>, <code>null</code> if there are no
     *         historic persisted changes, the state has changed since the last update or the default persistence
    public static @Nullable ZonedDateTime lastChange(Item item) {
        return internalAdjacentChange(item, false, null);
     * Query for the last historic change time of a given <code>item</code>.
     * @return point in time of the last historic change to <code>item</code> <code>null</code> if there are no
     *         historic persisted changes, the state has changed since the last update or if persistence service given
    public static @Nullable ZonedDateTime lastChange(Item item, @Nullable String serviceId) {
        return internalAdjacentChange(item, false, serviceId);
     * Query the first future change time of a given <code>item</code>. The default persistence service is used.
     * @param item the item for which the first future change time is to be returned
     * @return point in time of the first future change to <code>item</code>, or <code>null</code> if there are no
     *         future persisted changes or the default persistence service is not available or not a
    public static @Nullable ZonedDateTime nextChange(Item item) {
        return internalAdjacentChange(item, true, null);
     * Query for the first future change time of a given <code>item</code>.
     *         future persisted changes or if persistence service given by <code>serviceId</code> does not refer to an
    public static @Nullable ZonedDateTime nextChange(Item item, @Nullable String serviceId) {
        return internalAdjacentChange(item, true, serviceId);
    private static @Nullable ZonedDateTime internalAdjacentChange(Item item, boolean forward,
        return internalAdjacent(item, forward, true, serviceId);
    private static @Nullable ZonedDateTime internalAdjacent(Item item, boolean forward, boolean skipEqual,
            if (forward) {
                filter.setBeginDate(ZonedDateTime.now());
                filter.setEndDate(ZonedDateTime.now());
            filter.setOrdering(forward ? Ordering.ASCENDING : Ordering.DESCENDING);
            filter.setPageSize(skipEqual ? 1000 : 1);
            int startPage = 0;
            filter.setPageNumber(startPage);
            Iterator<HistoricItem> itemIterator = qService.query(filter, alias).iterator();
            if (!itemIterator.hasNext()) {
            int itemCount = 0;
            if (!skipEqual) {
                HistoricItem historicItem = itemIterator.next();
                if (!forward && !historicItem.getState().equals(state)) {
                    // Last persisted state value different from current state value, so it must have updated
                    // since last persist. We do not know when from persistence, so get it from the item.
                    return item.getLastStateUpdate();
                return historicItem.getTimestamp();
                if (!historicItem.getState().equals(state)) {
                    // Persisted state value different from current state value, so it must have changed, but we
                    // do not know when looking backward in persistence. Get it from the item.
                    return forward ? historicItem.getTimestamp() : item.getLastStateChange();
                while (historicItem.getState().equals(state) && itemIterator.hasNext()) {
                    HistoricItem nextHistoricItem = itemIterator.next();
                    if (!nextHistoricItem.getState().equals(state)) {
                        return forward ? nextHistoricItem.getTimestamp() : historicItem.getTimestamp();
                    historicItem = nextHistoricItem;
                    if (itemCount == filter.getPageSize()) {
                        itemCount = 0;
                        filter.setPageNumber(++startPage);
                        itemIterator = qService.query(filter, alias).iterator();
     * Returns the previous state of a given <code>item</code>.
     * Note the {@link Item#getLastState()} is generally preferred to get the previous state of an item.
     * @param item the item to get the previous state value for
     * @return the previous state or <code>null</code> if no previous state could be found, or if the default
     *         persistence service is not configured or does not refer to a {@link QueryablePersistenceService}
    public static @Nullable HistoricItem previousState(Item item) {
        return internalAdjacentState(item, false, false, null);
     * @param skipEqual if true, skips equal state values and searches the first state not equal the current state
    public static @Nullable HistoricItem previousState(Item item, boolean skipEqual) {
        return internalAdjacentState(item, skipEqual, false, null);
     * The {@link PersistenceService} identified by the <code>serviceId</code> is used.
    public static @Nullable HistoricItem previousState(Item item, @Nullable String serviceId) {
        return internalAdjacentState(item, false, false, serviceId);
     * @param skipEqual if <code>true</code>, skips equal state values and searches the first state not equal the
     *            current state
     * @return the previous state or <code>null</code> if no previous state could be found, or if the given
     *         <code>serviceId</code> is not available or does not refer to a {@link QueryablePersistenceService}
    public static @Nullable HistoricItem previousState(Item item, boolean skipEqual, @Nullable String serviceId) {
        return internalAdjacentState(item, skipEqual, false, serviceId);
     * Returns the next state of a given <code>item</code>.
     * @param item the item to get the next state value for
     * @return the next state or <code>null</code> if no next state could be found, or if the default
    public static @Nullable HistoricItem nextState(Item item) {
        return internalAdjacentState(item, false, true, null);
    public static @Nullable HistoricItem nextState(Item item, boolean skipEqual) {
        return internalAdjacentState(item, skipEqual, true, null);
    public static @Nullable HistoricItem nextState(Item item, @Nullable String serviceId) {
        return internalAdjacentState(item, false, true, serviceId);
     * @return the next state or <code>null</code> if no next state could be found, or if the given
    public static @Nullable HistoricItem nextState(Item item, boolean skipEqual, @Nullable String serviceId) {
        return internalAdjacentState(item, skipEqual, true, serviceId);
    private static @Nullable HistoricItem internalAdjacentState(Item item, boolean skipEqual, boolean forward,
            while (itemIterator.hasNext()) {
                if (!skipEqual || !historicItem.getState().equals(item.getState())) {
                    return historicItem;
     * Checks if the state of a given <code>item</code> has changed since a certain point in time.
     * The default persistence service is used.
     * @param item the item to check for state changes
     * @param timestamp the point in time to start the check
     * @return <code>true</code> if item state has changed, <code>false</code> if it has not changed, <code>null</code>
     *         if <code>timestamp</code> is in the future, if the default persistence service is not available or does
     *         not refer to a {@link QueryablePersistenceService}
    public static @Nullable Boolean changedSince(Item item, ZonedDateTime timestamp) {
        return internalChangedBetween(item, timestamp, null, null);
     * Checks if the state of a given <code>item</code> will change by a certain point in time.
     * @param timestamp the point in time to end the check
     * @return <code>true</code> if item state will change, <code>false</code> if it will not change, <code>null</code>
     *         if <code>timestamp></code> is in the past, if the default persistence service is not available or does
    public static @Nullable Boolean changedUntil(Item item, ZonedDateTime timestamp) {
        return internalChangedBetween(item, null, timestamp, null);
     * Checks if the state of a given <code>item</code> changes between two points in time.
     * @return <code>true</code> if item state changes, <code>false</code> if the item does not change in
     *         the given interval, <code>null</code> if <code>begin</code> is after <code>end</code>, if the default
     *         persistence does not refer to a {@link QueryablePersistenceService}, or <code>null</code> if the default
     *         persistence service is not available
    public static @Nullable Boolean changedBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalChangedBetween(item, begin, end, null);
     * @return <code>true</code> if item state has changed, or <code>false</code> if it has not changed,
     *         <code>null</code> if <code>timestamp</code> is in the future, if the provided <code>serviceId</code> does
     *         not refer to an available {@link QueryablePersistenceService}
    public static @Nullable Boolean changedSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalChangedBetween(item, timestamp, null, serviceId);
     * @return <code>true</code> if item state will change, or <code>false</code> if it will not change,
     *         <code>null</code> if <code>timestamp</code> is in the past, if the provided <code>serviceId</code> does
    public static @Nullable Boolean changedUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalChangedBetween(item, null, timestamp, serviceId);
     * @param begin the point in time to start the check
     * @param end the point in time to stop the check
     * @return <code>true</code> if item state changed or <code>false</code> if the item does not change
     *         in the given interval, <code>null</code> if <code>begin</code> is after <code>end</code>, if the given
     *         <code>serviceId</code> does not refer to a {@link QueryablePersistenceService}
    public static @Nullable Boolean changedBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalChangedBetween(item, begin, end, serviceId);
    private static @Nullable Boolean internalChangedBetween(Item item, @Nullable ZonedDateTime begin,
            @Nullable ZonedDateTime end, @Nullable String serviceId) {
        Iterable<HistoricItem> result = internalGetAllStatesBetween(item, begin, end, effectiveServiceId);
            HistoricItem itemThen = internalPersistedState(item, begin, effectiveServiceId);
            if (itemThen == null) {
                // Can't get the state at the start time
                // If we've got results more recent than this, it must have changed
                return it.hasNext();
            State state = itemThen.getState();
                HistoricItem hItem = it.next();
                if (!hItem.getState().equals(state)) {
                state = hItem.getState();
     * Checks if the state of a given <code>item</code> has been updated since a certain point in time.
     * @param item the item to check for state updates
     * @return <code>true</code> if item state was updated, <code>false</code> if the item has not been updated since
     *         <code>timestamp</code>, <code>null</code> if <code>timestamp</code> is in the future, if the default
    public static @Nullable Boolean updatedSince(Item item, ZonedDateTime timestamp) {
        return internalUpdatedBetween(item, timestamp, null, null);
     * Checks if the state of a given <code>item</code> will be updated until a certain point in time.
     * @return <code>true</code> if item state is updated, <code>false</code> if the item is not updated until
     *         <code>timestamp</code>, <code>null</code> if <code>timestamp</code> is in the past, if the default
    public static @Nullable Boolean updatedUntil(Item item, ZonedDateTime timestamp) {
        return internalUpdatedBetween(item, null, timestamp, null);
     * Checks if the state of a given <code>item</code> has been updated between two points in time.
     * @return <code>true</code> if item state was updated, <code>false</code> if the item has not been updated in
    public static @Nullable Boolean updatedBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalUpdatedBetween(item, begin, end, null);
     * @return <code>true</code> if item state was updated or <code>false</code> if the item has not been updated
     *         since <code>timestamp</code>, <code>null</code> if <code>timestamp</code> is in the future, if the given
    public static @Nullable Boolean updatedSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalUpdatedBetween(item, timestamp, null, serviceId);
     * @return <code>true</code> if item state was updated or <code>false</code> if the item is not updated
     *         since <code>timestamp</code>, <code>null</code> if <code>timestamp</code> is in the past, if the given
    public static @Nullable Boolean updatedUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalUpdatedBetween(item, null, timestamp, serviceId);
     * Checks if the state of a given <code>item</code> is updated between two points in time.
    public static @Nullable Boolean updatedBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalUpdatedBetween(item, begin, end, serviceId);
    private static @Nullable Boolean internalUpdatedBetween(Item item, @Nullable ZonedDateTime begin,
            return result.iterator().hasNext();
     * Gets the historic item with the maximum value of the state of a given <code>item</code> since
     * a certain point in time. The default persistence service is used.
     * @param item the item to get the maximum state value for
     * @return a historic item with the maximum state value since the given point in time, a
     *         {@link HistoricItem} constructed from the <code>item</code>'s state if <code>item</code>'s state is the
     *         maximum value, <code>null</code> if <code>timestamp</code> is in the future or if the default
     *         persistence service does not refer to a {@link QueryablePersistenceService}
    public static @Nullable HistoricItem maximumSince(Item item, ZonedDateTime timestamp) {
        return internalMaximumBetween(item, timestamp, null, null);
     * Gets the historic item with the maximum value of the state of a given <code>item</code> until
     * @return a historic item with the maximum state value until the given point in time, a
     *         maximum value, <code>null</code> if <code>timestamp</code> is in the past or if the default
    public static @Nullable HistoricItem maximumUntil(Item item, ZonedDateTime timestamp) {
        return internalMaximumBetween(item, null, timestamp, null);
     * Gets the historic item with the maximum value of the state of a given <code>item</code> between two points in
     * time. The default persistence service is used.
     * @return a {@link HistoricItem} with the maximum state value between two points in time, a
     *         {@link HistoricItem} constructed from the <code>item</code>'s state if no persisted states found, or
     *         <code>null</code> if <code>begin</code> is after <code>end</end> or if the default persistence service
     *         does not refer to an available{@link QueryablePersistenceService}
    public static @Nullable HistoricItem maximumBetween(final Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalMaximumBetween(item, begin, end, null);
     * a certain point in time. The {@link PersistenceService} identified by the <code>serviceId</code> is used.
     * @return a {@link HistoricItem} with the maximum state value since the given point in time, a
     *         maximum value, <code>null</code> if <code>timestamp</code> is in the future or if the given
     *         <code>serviceId</code> does not refer to an available {@link QueryablePersistenceService}
    public static @Nullable HistoricItem maximumSince(final Item item, ZonedDateTime timestamp,
        return internalMaximumBetween(item, timestamp, null, serviceId);
     * @return a {@link HistoricItem} with the maximum state value until the given point in time, a
     *         maximum value, <code>null</code> if <code>timestamp</code> is in the past or if the given
    public static @Nullable HistoricItem maximumUntil(final Item item, ZonedDateTime timestamp,
        return internalMaximumBetween(item, null, timestamp, serviceId);
     * time. The {@link PersistenceService} identified by the <code>serviceId</code> is used.
     *         <code>null</code> if <code>begin</code> is after <code>end</end> or if the given <code>serviceId</code>
     *         does not refer to an available {@link QueryablePersistenceService}
    public static @Nullable HistoricItem maximumBetween(final Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalMaximumBetween(item, begin, end, serviceId);
    private static @Nullable HistoricItem internalMaximumBetween(final Item item, @Nullable ZonedDateTime begin,
        Iterable<HistoricItem> result = getAllStatesBetweenWithBoundaries(item, begin, end, effectiveServiceId);
        HistoricItem maximumHistoricItem = null;
        Item baseItem = item instanceof GroupItem groupItem ? groupItem.getBaseItem() : item;
        Unit<?> unit = (baseItem instanceof NumberItem numberItem) ? numberItem.getUnit() : null;
        DecimalType maximum = null;
            DecimalType value = getPersistedValue(historicItem, unit);
                if (maximum == null || value.compareTo(maximum) > 0) {
                    maximum = value;
                    maximumHistoricItem = historicItem;
        return historicItemOrCurrentState(item, maximumHistoricItem);
     * Gets the historic item with the minimum value of the state of a given <code>item</code> since
     * @param item the item to get the minimum state value for
     * @param timestamp the point in time from which to search for the minimum state value
     * @return a historic item with the minimum state value since the given point in time, a
     *         minimum value, <code>null</code> if <code>timestamp</code> is in the future or if the default
    public static @Nullable HistoricItem minimumSince(Item item, ZonedDateTime timestamp) {
        return internalMinimumBetween(item, timestamp, null, null);
     * Gets the historic item with the minimum value of the state of a given <code>item</code> until
     * @param timestamp the point in time to which to search for the minimum state value
     * @return a historic item with the minimum state value until the given point in time, a
     *         minimum value, <code>null</code> if <code>timestamp</code> is in the past or if the default
    public static @Nullable HistoricItem minimumUntil(Item item, ZonedDateTime timestamp) {
        return internalMinimumBetween(item, null, timestamp, null);
     * Gets the historic item with the minimum value of the state of a given <code>item</code> between
     * two certain points in time. The default persistence service is used.
     * @param begin the beginning point in time
     * @param end the ending point in time to
     * @return a {@link HistoricItem} with the minimum state value between two points in time, a
    public static @Nullable HistoricItem minimumBetween(final Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalMinimumBetween(item, begin, end, null);
     * @return a {@link HistoricItem} with the minimum state value since the given point in time, a
     *         minimum value, <code>null</code> if <code>timestamp</code> is in the future or if the given
    public static @Nullable HistoricItem minimumSince(final Item item, ZonedDateTime timestamp,
        return internalMinimumBetween(item, timestamp, null, serviceId);
     * @return a {@link HistoricItem} with the minimum state value until the given point in time, a
     *         minimum value, <code>null</code> if <code>timestamp</code> is in the past or if the given
    public static @Nullable HistoricItem minimumUntil(final Item item, ZonedDateTime timestamp,
        return internalMinimumBetween(item, null, timestamp, serviceId);
     * two certain points in time. The {@link PersistenceService} identified by the <code>serviceId</code> is used.
     * @param end the end point in time to
    public static @Nullable HistoricItem minimumBetween(final Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalMinimumBetween(item, begin, end, serviceId);
    private static @Nullable HistoricItem internalMinimumBetween(final Item item, @Nullable ZonedDateTime begin,
        HistoricItem minimumHistoricItem = null;
        DecimalType minimum = null;
                if (minimum == null || value.compareTo(minimum) < 0) {
                    minimum = value;
                    minimumHistoricItem = historicItem;
        return historicItemOrCurrentState(item, minimumHistoricItem);
     * Gets the variance of the state of the given {@link Item} since a certain point in time.
     * A left approximation type is used for the Riemann sum.
     * The default {@link PersistenceService} is used.
     * @param item the {@link Item} to get the variance for
     * @param timestamp the point in time from which to compute the variance
     * @return the variance between then and now, or <code>null</code> if <code>timestamp</code> is in the future, if
     *         there is no default persistence service available, or it is not a {@link QueryablePersistenceService}, or
     *         if there is no persisted state for the given <code>item</code> at the given <code>timestamp</code>
    public static @Nullable State varianceSince(Item item, ZonedDateTime timestamp) {
        return internalVarianceBetween(item, timestamp, null, null, null);
     * @param type LEFT, RIGHT, MIDPOINT or TRAPEZOIDAL representing approximation types for Riemann sums
    public static @Nullable State varianceSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalVarianceBetween(item, timestamp, null, type, null);
     * Gets the variance of the state of the given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to compute the variance
     * @return the variance between now and then, or <code>null</code> if <code>timestamp</code> is in the past, if
    public static @Nullable State varianceUntil(Item item, ZonedDateTime timestamp) {
        return internalVarianceBetween(item, null, timestamp, null, null);
    public static @Nullable State varianceUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalVarianceBetween(item, null, timestamp, type, null);
     * Gets the variance of the state of the given {@link Item} between two points in time.
     * @param begin the point in time from which to compute
     * @param end the end time for the computation
     * @return the variance between both points of time, or <code>null</code> if <code>begin</code> is after
     *         <code>end</code>, if the persistence service given by
     *         <code>serviceId</code> is not available, or it is not a {@link QueryablePersistenceService}, or it is not
     *         a {@link QueryablePersistenceService}, or if there is no persisted state for the
     *         given <code>item</code> between <code>begin</code> and <code>end</code>
    public static @Nullable State varianceBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalVarianceBetween(item, begin, end, null, serviceId);
     * Gets the variance of the state of the given {@link Item} between two certain point in time.
     * @param begin the point in time from which to compute the variance
     *         <code>end</code>, if there is no default persistence service available, or it is not a
     *         {@link QueryablePersistenceService}, or if there is no persisted state for the
            @Nullable RiemannType type) {
        return internalVarianceBetween(item, begin, end, type, null);
     *         the persistence service given by <code>serviceId</code> is not available, or it is not a
     *         {@link QueryablePersistenceService}, or if there is no persisted state for the given <code>item</code> at
     *         the given <code>timestamp</code>
    public static @Nullable State varianceSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalVarianceBetween(item, timestamp, null, null, serviceId);
    public static @Nullable State varianceSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalVarianceBetween(item, timestamp, null, type, serviceId);
     * @return the variance between now and then, or <code>null</code> if <code>timestamp</code> is in the past, if the
     *         persistence service given by <code>serviceId</code> is not available, or it is not a
    public static @Nullable State varianceUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalVarianceBetween(item, null, timestamp, null, serviceId);
    public static @Nullable State varianceUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalVarianceBetween(item, null, timestamp, type, serviceId);
    public static @Nullable State varianceBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalVarianceBetween(item, begin, end, null, null);
            @Nullable RiemannType type, @Nullable String serviceId) {
        return internalVarianceBetween(item, begin, end, type, serviceId);
    private static @Nullable State internalVarianceBetween(Item item, @Nullable ZonedDateTime begin,
            @Nullable ZonedDateTime end, @Nullable RiemannType type, @Nullable String serviceId) {
        ZonedDateTime now = ZonedDateTime.now();
        ZonedDateTime beginTime = Objects.requireNonNullElse(begin, now);
        ZonedDateTime endTime = Objects.requireNonNullElse(end, now);
        // Remove initial part of history that does not have any values persisted
        if (beginTime.isBefore(now)) {
                beginTime = it.next().getTimestamp();
            it = result.iterator();
        Unit<?> unit = (baseItem instanceof NumberItem numberItem)
                && (numberItem.getUnit() instanceof Unit<?> numberItemUnit) ? numberItemUnit.getSystemUnit() : null;
        BigDecimal average = average(beginTime, endTime, it, unit, type);
        if (average != null) {
            BigDecimal sum = BigDecimal.ZERO;
                    sum = sum.add(value.toBigDecimal().subtract(average, MathContext.DECIMAL64).pow(2,
                            MathContext.DECIMAL64));
            // avoid division by zero
                BigDecimal variance = sum.divide(BigDecimal.valueOf(count), MathContext.DECIMAL64);
                    return new QuantityType<>(variance, unit.multiply(unit));
                return new DecimalType(variance);
     * Gets the standard deviation of the state of the given {@link Item} since a certain point in time.
     * <b>Note:</b> If you need variance and standard deviation at the same time do not query both as it is a costly
     * operation. Get the variance only, it is the squared deviation.
     * @param item the {@link Item} to get the standard deviation for
     * @param timestamp the point in time from which to compute the standard deviation
     * @return the standard deviation between then and now, or <code>null</code> if <code>timestamp</code> is in the
     *         future, if there is no default persistence service available, or it is not a
    public static @Nullable State deviationSince(Item item, ZonedDateTime timestamp) {
        return internalDeviationBetween(item, timestamp, null, null, null);
    public static @Nullable State deviationSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalDeviationBetween(item, timestamp, null, type, null);
     * Gets the standard deviation of the state of the given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to compute the standard deviation
     * @return the standard deviation between now and then, or <code>null</code> if <code>timestamp</code> is in the
     *         past, if there is no default persistence service available, or it is not a
    public static @Nullable State deviationUntil(Item item, ZonedDateTime timestamp) {
    public static @Nullable State deviationUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
     * Gets the standard deviation of the state of the given {@link Item} between two points in time.
     * @return the standard deviation between both points of time, or <code>null</code> if <code>begin</code> is after
    public static @Nullable State deviationBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalDeviationBetween(item, begin, end, null, null);
    public static @Nullable State deviationBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalDeviationBetween(item, begin, end, type, null);
     *         future, if the persistence service given by <code>serviceId</code> is not available, or it is not a
    public static @Nullable State deviationSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalDeviationBetween(item, timestamp, null, null, serviceId);
    public static @Nullable State deviationSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalDeviationBetween(item, timestamp, null, type, serviceId);
     *         past, if the persistence service given by <code>serviceId</code> is not available, or it is not a
    public static @Nullable State deviationUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalDeviationBetween(item, null, timestamp, null, serviceId);
    public static @Nullable State deviationUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalDeviationBetween(item, null, timestamp, type, serviceId);
        return internalDeviationBetween(item, begin, end, null, serviceId);
        return internalDeviationBetween(item, begin, end, type, serviceId);
    private static @Nullable State internalDeviationBetween(Item item, @Nullable ZonedDateTime begin,
        State variance = internalVarianceBetween(item, begin, end, type, effectiveServiceId);
        if (variance != null) {
            DecimalType dt = variance.as(DecimalType.class);
            // avoid ArithmeticException if variance is less than zero
            if (dt != null && DecimalType.ZERO.compareTo(dt) <= 0) {
                BigDecimal deviation = dt.toBigDecimal().sqrt(MathContext.DECIMAL64);
                        && (numberItem.getUnit() instanceof Unit<?> numberItemUnit) ? numberItemUnit.getSystemUnit()
                    return new QuantityType<>(deviation, unit);
                    return new DecimalType(deviation);
     * Gets the average value of the state of a given {@link Item} since a certain point in time.
     * @param item the {@link Item} to get the average value for
     * @param timestamp the point in time from which to search for the average value
     * @return the average value since <code>timestamp</code> or <code>null</code> if no
     *         previous states could be found or if the default persistence service does not refer to an available
     *         {@link QueryablePersistenceService}. The current state is included in the calculation.
    public static @Nullable State averageSince(Item item, ZonedDateTime timestamp) {
        return internalAverageBetween(item, timestamp, null, null, null);
    public static @Nullable State averageSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalAverageBetween(item, timestamp, null, type, null);
     * Gets the average value of the state of a given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to search for the average value
     * @return the average value until <code>timestamp</code> or <code>null</code> if no
     *         future states could be found or if the default persistence service does not refer to an available
    public static @Nullable State averageUntil(Item item, ZonedDateTime timestamp) {
        return internalAverageBetween(item, null, timestamp, null, null);
    public static @Nullable State averageUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalAverageBetween(item, null, timestamp, type, null);
     * Gets the average value of the state of a given {@link Item} between two certain points in time.
     * @param begin the point in time from which to start the summation
     * @param end the point in time to which to start the summation
     * @return the average value between <code>begin</code> and <code>end</code> or <code>null</code> if no
     *         states could be found or if the default persistence service does not refer to an available
     *         {@link QueryablePersistenceService}.
    public static @Nullable State averageBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalAverageBetween(item, begin, end, null, null);
    public static @Nullable State averageBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalAverageBetween(item, begin, end, type, null);
     * @return the average value since <code>timestamp</code>, or <code>null</code> if no
     *         previous states could be found or if the persistence service given by <code>serviceId</code> does not
     *         refer to an available {@link QueryablePersistenceService}. The current state is included in the
     *         calculation.
    public static @Nullable State averageSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalAverageBetween(item, timestamp, null, null, serviceId);
    public static @Nullable State averageSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalAverageBetween(item, timestamp, null, type, serviceId);
     * @return the average value until <code>timestamp</code>, or <code>null</code> if no
     *         future states could be found or if the persistence service given by <code>serviceId</code> does not
    public static @Nullable State averageUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalAverageBetween(item, null, timestamp, null, serviceId);
    public static @Nullable State averageUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalAverageBetween(item, null, timestamp, type, serviceId);
     * @return the average value between <code>begin</code> and <code>end</code>, or <code>null</code> if no
     *         states could be found or if the persistence service given by <code>serviceId</code> does not
     *         refer to an available {@link QueryablePersistenceService}
        return internalAverageBetween(item, begin, end, null, serviceId);
        return internalAverageBetween(item, begin, end, type, serviceId);
    private static @Nullable State internalAverageBetween(Item item, @Nullable ZonedDateTime begin,
        if (beginTime.isEqual(endTime)) {
            HistoricItem historicItem = internalPersistedState(item, beginTime, effectiveServiceId);
            return historicItem != null ? historicItem.getState() : null;
        Unit<?> unit = baseItem instanceof NumberItem numberItem ? numberItem.getUnit() : null;
        if (average == null) {
            return new QuantityType<>(average, unit);
        return new DecimalType(average);
    private static @Nullable BigDecimal average(ZonedDateTime begin, ZonedDateTime end, Iterator<HistoricItem> it,
            @Nullable Unit<?> unit, @Nullable RiemannType type) {
        BigDecimal sum = riemannSum(begin.toInstant(), end.toInstant(), it, unit, type);
        BigDecimal totalDuration = BigDecimal.valueOf(Duration.between(begin, end).toMillis());
        if (totalDuration.signum() == 0) {
        return sum.divide(totalDuration, MathContext.DECIMAL64);
     * Gets the Riemann sum of the states of a given {@link Item} since a certain point in time.
     * This can be used as an approximation for integrating the curve represented by discrete values.
     * The time dimension in the result is in seconds, therefore if you do not use QuantityType results, you may have to
     * multiply or divide to get the result in the expected scale.
     * @param item the {@link Item} to get the riemannSum value for
     * @param timestamp the point in time from which to search for the riemannSum value
     * @return the Riemann sum since <code>timestamp</code> or <code>null</code> if no
    public static @Nullable State riemannSumSince(Item item, ZonedDateTime timestamp) {
        return internalRiemannSumBetween(item, timestamp, null, null, null);
    public static @Nullable State riemannSumSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalRiemannSumBetween(item, timestamp, null, type, null);
     * Gets the Riemann sum of the states of a given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to search for the riemannSum value
     * @return the Riemann sum until <code>timestamp</code> or <code>null</code> if no
    public static @Nullable State riemannSumUntil(Item item, ZonedDateTime timestamp) {
        return internalRiemannSumBetween(item, null, timestamp, null, null);
    public static @Nullable State riemannSumUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type) {
        return internalRiemannSumBetween(item, null, timestamp, type, null);
     * Gets the Riemann sum of the states of a given {@link Item} between two certain points in time.
     * @return the Riemann sum between <code>begin</code> and <code>end</code> or <code>null</code> if no
    public static @Nullable State riemannSumBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalRiemannSumBetween(item, begin, end, null, null);
    public static @Nullable State riemannSumBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalRiemannSumBetween(item, begin, end, type, null);
     * @return the Riemann sum since <code>timestamp</code>, or <code>null</code> if no
    public static @Nullable State riemannSumSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalRiemannSumBetween(item, timestamp, null, null, serviceId);
    public static @Nullable State riemannSumSince(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalRiemannSumBetween(item, timestamp, null, type, serviceId);
     * @return the Riemann sum until <code>timestamp</code>, or <code>null</code> if no
    public static @Nullable State riemannSumUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalRiemannSumBetween(item, null, timestamp, null, serviceId);
    public static @Nullable State riemannSumUntil(Item item, ZonedDateTime timestamp, @Nullable RiemannType type,
        return internalRiemannSumBetween(item, null, timestamp, type, serviceId);
     * @return the Riemann sum between <code>begin</code> and <code>end</code>, or <code>null</code> if no
        return internalRiemannSumBetween(item, begin, end, null, serviceId);
        return internalRiemannSumBetween(item, begin, end, type, serviceId);
    private static @Nullable State internalRiemannSumBetween(Item item, @Nullable ZonedDateTime begin,
        BigDecimal sum = riemannSum(beginTime.toInstant(), endTime.toInstant(), it, unit, type).scaleByPowerOfTen(-3);
            return new QuantityType<>(sum, unit.multiply(Units.SECOND));
        return new DecimalType(sum);
    private static BigDecimal riemannSum(Instant begin, Instant end, Iterator<HistoricItem> it, @Nullable Unit<?> unit,
        RiemannType riemannType = type == null ? RiemannType.LEFT : type;
        HistoricItem prevItem = null;
        HistoricItem nextItem;
        DecimalType prevState = null;
        DecimalType nextState;
        Instant prevInstant = null;
        Instant nextInstant;
        Duration prevDuration = Duration.ZERO;
        Duration nextDuration;
        boolean midpointStartBucket = true; // The start and end buckets for the midpoint calculation should be
                                            // considered for the full length, this flag is used to find the start
                                            // bucket
        if ((riemannType == RiemannType.MIDPOINT) && it.hasNext()) {
            prevItem = it.next();
            prevInstant = prevItem.getInstant();
            prevState = getPersistedValue(prevItem, unit);
            nextItem = it.next();
            nextInstant = nextItem.getInstant();
            BigDecimal weight = BigDecimal.ZERO;
            BigDecimal value = BigDecimal.ZERO;
            switch (riemannType) {
                case LEFT:
                    if (prevItem != null) {
                        if (prevState != null) {
                            value = prevState.toBigDecimal();
                            weight = BigDecimal.valueOf(Duration.between(prevInstant, nextInstant).toMillis());
                    prevItem = nextItem;
                    prevInstant = nextInstant;
                case RIGHT:
                    nextState = getPersistedValue(nextItem, unit);
                    if (nextState != null) {
                        value = nextState.toBigDecimal();
                        if (prevItem == null) {
                            weight = BigDecimal.valueOf(Duration.between(begin, nextInstant).toMillis());
                case TRAPEZOIDAL:
                        if (prevState != null && nextState != null) {
                            value = prevState.toBigDecimal().add(nextState.toBigDecimal())
                                    .divide(BigDecimal.valueOf(2));
                case MIDPOINT:
                        DecimalType currentState = getPersistedValue(prevItem, unit);
                        if (currentState != null) {
                            value = currentState.toBigDecimal();
                            if (midpointStartBucket && !prevDuration.isZero() && prevState != null) {
                                // Add half of the start bucket with the start value (left approximation)
                                sum = sum.add(prevState.toBigDecimal()
                                        .multiply(BigDecimal.valueOf(prevDuration.toMillis() / 2)));
                                midpointStartBucket = false;
                            nextDuration = Duration.between(prevInstant, nextInstant);
                            weight = prevDuration.isZero() || nextDuration.isZero() ? BigDecimal.ZERO
                                    : BigDecimal.valueOf(prevDuration.plus(nextDuration).toMillis() / 2);
                            if (!nextDuration.isZero()) {
                                prevDuration = nextDuration;
                            prevState = currentState;
            sum = sum.add(value.multiply(weight));
        if ((riemannType == RiemannType.MIDPOINT) && (prevItem != null)) {
            // Add half of the end bucket with the end value (right approximation)
            DecimalType dtState = getPersistedValue(prevItem, unit);
            if (dtState != null) {
                BigDecimal value = dtState.toBigDecimal();
                BigDecimal weight = BigDecimal.valueOf(prevDuration.toMillis() / 2);
        return sum;
     * Gets the median value of the state of a given {@link Item} since a certain point in time.
     * @param item the {@link Item} to get the median value for
     * @param timestamp the point in time from which to search for the median value
     * @return the median value since <code>timestamp</code> or <code>null</code> if no
    public static @Nullable State medianSince(Item item, ZonedDateTime timestamp) {
        return internalMedianBetween(item, timestamp, null, null);
     * Gets the median value of the state of a given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to search for the median value
     * @return the median value until <code>timestamp</code> or <code>null</code> if no
    public static @Nullable State medianUntil(Item item, ZonedDateTime timestamp) {
        return internalMedianBetween(item, null, timestamp, null);
     * Gets the median value of the state of a given {@link Item} between two certain points in time.
     * @return the median value between <code>begin</code> and <code>end</code> or <code>null</code> if no
    public static @Nullable State medianBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalMedianBetween(item, begin, end, null);
     * @return the median value since <code>timestamp</code>, or <code>null</code> if no
    public static @Nullable State medianSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalMedianBetween(item, timestamp, null, serviceId);
     * @return the median value until <code>timestamp</code>, or <code>null</code> if no
    public static @Nullable State medianUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalMedianBetween(item, null, timestamp, serviceId);
     * @return the median value between <code>begin</code> and <code>end</code>, or <code>null</code> if no
    public static @Nullable State medianBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalMedianBetween(item, begin, end, serviceId);
    private static @Nullable State internalMedianBetween(Item item, @Nullable ZonedDateTime begin,
        Iterable<HistoricItem> result = internalGetAllStatesBetween(item, beginTime, endTime, effectiveServiceId);
        List<BigDecimal> resultList = new ArrayList<>();
        result.forEach(hi -> {
            DecimalType dtState = getPersistedValue(hi, unit);
                resultList.add(dtState.toBigDecimal());
        BigDecimal median = Statistics.median(resultList);
        if (median != null) {
                return new QuantityType<>(median, unit);
                return new DecimalType(median);
     * Gets the sum of the state of a given <code>item</code> since a certain point in time.
     * This method does not calculate a Riemann sum and therefore cannot be used as an approximation for the integral
     * value.
     * @param item the item for which we will sum its persisted state values since <code>timestamp</code>
     * @param timestamp the point in time from which to start the summation
     * @return the sum of the state values since <code>timestamp</code>, or null if <code>timestamp</code> is in the
     *         future or the default persistence service does not refer to a {@link QueryablePersistenceService}
    public @Nullable static State sumSince(Item item, ZonedDateTime timestamp) {
        return internalSumBetween(item, timestamp, null, null);
     * Gets the sum of the state of a given <code>item</code> until a certain point in time.
     * @param item the item for which we will sum its persisted state values to <code>timestamp</code>
     * @param timestamp the point in time to which to start the summation
     * @return the sum of the state values until <code>timestamp</code>, or null if <code>timestamp</code> is in the
     *         past or the default persistence service does not refer to a {@link QueryablePersistenceService}
    public @Nullable static State sumUntil(Item item, ZonedDateTime timestamp) {
        return internalSumBetween(item, null, timestamp, null);
     * Gets the sum of the state of a given <code>item</code> between two certain points in time.
     * @param item the item for which we will sum its persisted state values between <code>begin</code> and
     *            <code>end</code>
     * @return the sum of the state values between the given points in time, or null if <code>begin</code> is after
     *         <code>end</code> or if the default persistence service does not refer to a
    public @Nullable static State sumBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalSumBetween(item, begin, end, null);
     *         future or <code>serviceId</code> does not refer to a {@link QueryablePersistenceService}
    public @Nullable static State sumSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalSumBetween(item, timestamp, null, serviceId);
     *         past or <code>serviceId</code> does not refer to a {@link QueryablePersistenceService}
    public @Nullable static State sumUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalSumBetween(item, null, timestamp, serviceId);
     *         <code>end</code> or <code>serviceId</code> does not refer to a {@link QueryablePersistenceService}
    public @Nullable static State sumBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalSumBetween(item, begin, end, serviceId);
    private @Nullable static State internalSumBetween(Item item, @Nullable ZonedDateTime begin,
                    sum = sum.add(value.toBigDecimal());
                return new QuantityType<>(sum, unit);
     * Gets the difference value of the state of a given <code>item</code> since a certain point in time.
     * @param item the item to get the delta state value for
     * @param timestamp the point in time from which to compute the delta
     * @return the difference between now and then, or <code>null</code> if there is no default persistence
     *         service available, the default persistence service is not a {@link QueryablePersistenceService}, or if
     *         there is no persisted state for the given <code>item</code> at the given <code>timestamp</code> available
     *         in the default persistence service
    public static @Nullable State deltaSince(Item item, ZonedDateTime timestamp) {
        return internalDeltaBetween(item, timestamp, null, null);
     * Gets the difference value of the state of a given <code>item</code> until a certain point in time.
     * @param timestamp the point in time to which to compute the delta
     * @return the difference between then and now, or <code>null</code> if there is no default persistence
    public static @Nullable State deltaUntil(Item item, ZonedDateTime timestamp) {
        return internalDeltaBetween(item, null, timestamp, null);
     * Gets the difference value of the state of a given <code>item</code> between two points in time.
     * @param item the item to get the delta for
     * @param end the end point in time
     * @return the difference between end and begin, or <code>null</code> if the default persistence service does not
     *         refer to an available {@link QueryablePersistenceService}, or if there is no persisted state for the
     *         given <code>item</code> for the given points in time
    public static @Nullable State deltaBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalDeltaBetween(item, begin, end, null);
     * @return the difference between now and then, or <code>null</code> if the given serviceId does not refer to an
     *         available {@link QueryablePersistenceService}, or if there is no persisted state for the given
     *         <code>item</code> at the given <code>timestamp</code> using the persistence service named
     *         <code>serviceId</code>
    public static @Nullable State deltaSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalDeltaBetween(item, timestamp, null, serviceId);
     * @return the difference between then and now, or <code>null</code> if the given serviceId does not refer to an
    public static @Nullable State deltaUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalDeltaBetween(item, null, timestamp, serviceId);
     * @return the difference between end and begin, or <code>null</code> if the given serviceId does not refer to an
     *         <code>item</code> at the given points in time
    public static @Nullable State deltaBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalDeltaBetween(item, begin, end, serviceId);
    private static @Nullable State internalDeltaBetween(Item item, @Nullable ZonedDateTime begin,
        HistoricItem itemStart = internalPersistedState(item, begin, effectiveServiceId);
        HistoricItem itemStop = internalPersistedState(item, end, effectiveServiceId);
        DecimalType valueStart = null;
        if (itemStart != null) {
            valueStart = getPersistedValue(itemStart, unit);
        DecimalType valueStop = null;
        if (itemStop != null) {
            valueStop = getPersistedValue(itemStop, unit);
        if (begin == null && end != null && end.isAfter(ZonedDateTime.now())) {
            valueStart = getItemValue(item);
        if (begin != null && end == null && begin.isBefore(ZonedDateTime.now())) {
            valueStop = getItemValue(item);
        if (valueStart != null && valueStop != null) {
            BigDecimal delta = valueStop.toBigDecimal().subtract(valueStart.toBigDecimal());
            return (unit != null) ? new QuantityType<>(delta, unit) : new DecimalType(delta);
     * Gets the evolution rate of the state of a given {@link Item} since a certain point in time.
     * This method has been deprecated and {@link #evolutionRateSince(Item, ZonedDateTime)} should be used instead.
     * @param item the item to get the evolution rate value for
     * @param timestamp the point in time from which to compute the evolution rate
     * @return the evolution rate in percent (positive and negative) between now and then, or <code>null</code> if
     *         there is no default persistence service available, the default persistence service is not a
     *         the given <code>timestamp</code>, or if there is a state but it is zero (which would cause a
     *         divide-by-zero error)
    public static @Nullable DecimalType evolutionRate(Item item, ZonedDateTime timestamp) {
                "The evolutionRate method has been deprecated and will be removed in a future version, use evolutionRateSince instead.");
        return internalEvolutionRateBetween(item, timestamp, null, null);
    public static @Nullable DecimalType evolutionRateSince(Item item, ZonedDateTime timestamp) {
     * Gets the evolution rate of the state of a given {@link Item} until a certain point in time.
     * @param timestamp the point in time to which to compute the evolution rate
     * @return the evolution rate in percent (positive and negative) between then and now, or <code>null</code> if
    public static @Nullable DecimalType evolutionRateUntil(Item item, ZonedDateTime timestamp) {
        return internalEvolutionRateBetween(item, null, timestamp, null);
     * Gets the evolution rate of the state of a given {@link Item} between two points in time.
     * This method has been deprecated and {@link #evolutionRateBetween(Item, ZonedDateTime, ZonedDateTime)} should be
     * used instead.
     * @return the evolution rate in percent (positive and negative) in the given interval, or <code>null</code> if
     *         {@link QueryablePersistenceService}, or if there are no persisted state for the given <code>item</code>
     *         at the given interval, or if there is a state but it is zero (which would cause a
    public static @Nullable DecimalType evolutionRate(Item item, ZonedDateTime begin, ZonedDateTime end) {
                "The evolutionRate method has been deprecated and will be removed in a future version, use evolutionRateBetween instead.");
        return internalEvolutionRateBetween(item, begin, end, null);
    public static @Nullable DecimalType evolutionRateBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
     * This method has been deprecated and {@link #evolutionRateSince(Item, ZonedDateTime, String)} should be used
     * instead.
     * @param item the {@link Item} to get the evolution rate value for
     *         the persistence service given by <code>serviceId</code> is not available or is not a
     *         {@link QueryablePersistenceService}, or if there is no persisted state for the given
     *         <code>item</code> at the given <code>timestamp</code> using the persistence service given by
     *         <code>serviceId</code>, or if there is a state but it is zero (which would cause a divide-by-zero
     *         error)
    public static @Nullable DecimalType evolutionRate(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalEvolutionRateBetween(item, timestamp, null, serviceId);
    public static @Nullable DecimalType evolutionRateSince(Item item, ZonedDateTime timestamp,
    public static @Nullable DecimalType evolutionRateUntil(Item item, ZonedDateTime timestamp,
        return internalEvolutionRateBetween(item, null, timestamp, serviceId);
     * This method has been deprecated and {@link #evolutionRateBetween(Item, ZonedDateTime, ZonedDateTime, String)}
     * should be used instead.
     *         <code>item</code> at the given <code>begin</code> and <code>end</code> using the persistence service
     *         given by <code>serviceId</code>, or if there is a state but it is zero (which would cause a
    public static @Nullable DecimalType evolutionRate(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalEvolutionRateBetween(item, begin, end, serviceId);
    public static @Nullable DecimalType evolutionRateBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
    private static @Nullable DecimalType internalEvolutionRateBetween(Item item, @Nullable ZonedDateTime begin,
        if (valueStart != null && valueStop != null && !valueStart.equals(DecimalType.ZERO)) {
            return new DecimalType(valueStop.toBigDecimal().subtract(valueStart.toBigDecimal())
                    .divide(valueStart.toBigDecimal(), MathContext.DECIMAL64).movePointRight(2));
     * Gets the number of available historic data points of a given {@link Item} from a point in time until now.
     * @param item the {@link Item} to query
     * @param timestamp the beginning point in time
     * @return the number of values persisted for this item, <code>null</code> if <code>timestamp</code> is in the
     *         future, if the default persistence service is not available or does not refer to a
    public static @Nullable Long countSince(Item item, ZonedDateTime timestamp) {
        return internalCountBetween(item, timestamp, null, null);
     * Gets the number of available data points of a given {@link Item} from now to a point in time.
     * @param timestamp the ending point in time
     *         past, if the default persistence service is not available or does not refer to a
    public static @Nullable Long countUntil(Item item, ZonedDateTime timestamp) {
        return internalCountBetween(item, null, timestamp, null);
     * Gets the number of available data points of a given {@link Item} between two points in time.
     * @return the number of values persisted for this item, <code>null</code> if <code>begin</code> is after
     *         <code>end</code>, if the default persistence service is not available or does not refer to a
    public static @Nullable Long countBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalCountBetween(item, begin, end, null);
     *         future, if the persistence service is not available or does not refer to a
    public static @Nullable Long countSince(Item item, ZonedDateTime begin, @Nullable String serviceId) {
        return internalCountBetween(item, begin, null, serviceId);
     *         past, if the persistence service is not available or does not refer to a
    public static @Nullable Long countUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        return internalCountBetween(item, null, timestamp, serviceId);
     *         <code>end</code>, if the persistence service is not available or does not refer to a
    public static @Nullable Long countBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalCountBetween(item, begin, end, serviceId);
    private static @Nullable Long internalCountBetween(Item item, @Nullable ZonedDateTime begin,
            if (result instanceof Collection<?> collection) {
                return Long.valueOf(collection.size());
                return StreamSupport.stream(result.spliterator(), false).count();
     * Gets the number of changes in historic data points of a given {@link Item} from a point in time until now.
     * @return the number of state changes for this item, <code>null</code>
     *         if the default persistence service is not available or does not refer to a
    public static @Nullable Long countStateChangesSince(Item item, ZonedDateTime timestamp) {
        return internalCountStateChangesBetween(item, timestamp, null, null);
     * Gets the number of changes in data points of a given {@link Item} from now until a point in time.
    public static @Nullable Long countStateChangesUntil(Item item, ZonedDateTime timestamp) {
        return internalCountStateChangesBetween(item, null, timestamp, null);
     * Gets the number of changes in data points of a given {@link Item} between two points in time.
    public static @Nullable Long countStateChangesBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        return internalCountStateChangesBetween(item, begin, end, null);
     *         if the persistence service is not available or does not refer to a
    public static @Nullable Long countStateChangesSince(Item item, ZonedDateTime timestamp,
        return internalCountStateChangesBetween(item, timestamp, null, serviceId);
    public static @Nullable Long countStateChangesUntil(Item item, ZonedDateTime timestamp,
        return internalCountStateChangesBetween(item, null, timestamp, serviceId);
    public static @Nullable Long countStateChangesBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        return internalCountStateChangesBetween(item, begin, end, serviceId);
    private static @Nullable Long internalCountStateChangesBetween(Item item, @Nullable ZonedDateTime begin,
            if (!it.hasNext()) {
                return Long.valueOf(0);
            State previousState = it.next().getState();
                if (!state.equals(previousState)) {
                    previousState = state;
     * Retrieves the historic items for a given <code>item</code> since a certain point in time.
     * @param timestamp the point in time from which to retrieve the states
     * @return the historic items since the given point in time, or <code>null</code>
    public static @Nullable Iterable<HistoricItem> getAllStatesSince(Item item, ZonedDateTime timestamp) {
        return internalGetAllStatesBetween(item, timestamp, null, null);
     * Retrieves the future items for a given <code>item</code> until a certain point in time.
     * @param item the item for which to retrieve the future item
     * @param timestamp the point in time to which to retrieve the states
     * @return the future items to the given point in time, or <code>null</code>
    public static @Nullable Iterable<HistoricItem> getAllStatesUntil(Item item, ZonedDateTime timestamp) {
        return internalGetAllStatesBetween(item, null, timestamp, null);
     * Retrieves the historic items for a given <code>item</code> between two points in time.
     * @param begin the point in time from which to retrieve the states
     * @param end the point in time to which to retrieve the states
     * @return the historic items between the given points in time, or <code>null</code>
    public static @Nullable Iterable<HistoricItem> getAllStatesBetween(Item item, ZonedDateTime begin,
            ZonedDateTime end) {
        return internalGetAllStatesBetween(item, begin, end, null);
     * Retrieves the historic items for a given <code>item</code> since a certain point in time
     * through a {@link PersistenceService} identified by the <code>serviceId</code>.
    public static @Nullable Iterable<HistoricItem> getAllStatesSince(Item item, ZonedDateTime timestamp,
        return internalGetAllStatesBetween(item, timestamp, null, serviceId);
     * Retrieves the future items for a given <code>item</code> until a certain point in time
    public static @Nullable Iterable<HistoricItem> getAllStatesUntil(Item item, ZonedDateTime timestamp,
        return internalGetAllStatesBetween(item, null, timestamp, serviceId);
     * Retrieves the historic items for a given <code>item</code> between two points in time
            ZonedDateTime end, @Nullable String serviceId) {
        return internalGetAllStatesBetween(item, begin, end, serviceId);
    private static @Nullable Iterable<HistoricItem> internalGetAllStatesBetween(Item item,
            @Nullable ZonedDateTime begin, @Nullable ZonedDateTime end, @Nullable String serviceId) {
            if ((begin == null && end == null) || (begin != null && end == null && begin.isAfter(now))
                    || (begin == null && end != null && end.isBefore(now))) {
                LoggerFactory.getLogger(PersistenceExtensions.class).warn(
                        "Querying persistence service with open begin and/or end not allowed: begin {}, end {}, now {}",
                        begin, end, now);
            if (begin != null) {
                filter.setBeginDate(begin);
                filter.setBeginDate(now);
            if (end != null) {
                filter.setEndDate(end);
                filter.setEndDate(now);
            return qService.query(filter, alias);
     * Removes from persistence the historic items for a given <code>item</code> since a certain point in time.
     * This will only have effect if the p{@link PersistenceService} is a {@link ModifiablePersistenceService}.
     * @param item the item for which to remove the historic item
     * @param timestamp the point in time from which to remove the states
    public static void removeAllStatesSince(Item item, ZonedDateTime timestamp) {
        internalRemoveAllStatesBetween(item, timestamp, null, null);
     * Removes from persistence the future items for a given <code>item</code> until a certain point in time.
     * @param item the item for which to remove the future item
     * @param timestamp the point in time to which to remove the states
    public static void removeAllStatesUntil(Item item, ZonedDateTime timestamp) {
        internalRemoveAllStatesBetween(item, null, timestamp, null);
     * Removes from persistence the historic items for a given <code>item</code> between two points in time.
     * @param begin the point in time from which to remove the states
     * @param end the point in time to which to remove the states
    public static void removeAllStatesBetween(Item item, ZonedDateTime begin, ZonedDateTime end) {
        internalRemoveAllStatesBetween(item, begin, end, null);
     * Removes from persistence the historic items for a given <code>item</code> since a certain point in time
    public static void removeAllStatesSince(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        internalRemoveAllStatesBetween(item, timestamp, null, serviceId);
     * Removes from persistence the future items for a given <code>item</code> until a certain point in time
    public static void removeAllStatesUntil(Item item, ZonedDateTime timestamp, @Nullable String serviceId) {
        internalRemoveAllStatesBetween(item, null, timestamp, serviceId);
     * Removes from persistence the historic items for a given <code>item</code> beetween two points in time
    public static void removeAllStatesBetween(Item item, ZonedDateTime begin, ZonedDateTime end,
        internalRemoveAllStatesBetween(item, begin, end, serviceId);
    private static void internalRemoveAllStatesBetween(Item item, @Nullable ZonedDateTime begin,
        if (service instanceof ModifiablePersistenceService mService) {
            internalRemoveAllStatesBetween(item, begin, end, mService, getAlias(item, effectiveServiceId));
                manager.handleExternalPersistenceDataChange(mService, item);
            @Nullable ZonedDateTime end, ModifiablePersistenceService mService, @Nullable String alias) {
    private static @Nullable Iterable<HistoricItem> getAllStatesBetweenWithBoundaries(Item item,
        Iterable<HistoricItem> betweenItems = internalGetAllStatesBetween(item, begin, end, serviceId);
                || (begin == null && end != null && end.isBefore(now))
                || (begin != null && end != null && end.isBefore(begin))) {
        List<HistoricItem> betweenItemsList = new ArrayList<>();
        if (betweenItems != null) {
            for (HistoricItem historicItem : betweenItems) {
                betweenItemsList.add(historicItem);
        // add HistoricItem at begin
        if (betweenItemsList.isEmpty() || !betweenItemsList.getFirst().getTimestamp().equals(begin)) {
            HistoricItem first = beginTime.equals(now) ? historicItemOrCurrentState(item, null)
                    : internalPersistedState(item, beginTime, serviceId);
            if (first != null) {
                first = new RetimedHistoricItem(first, beginTime);
                betweenItemsList.addFirst(first);
        // add HistoricItem at end
        if (betweenItemsList.isEmpty() || !betweenItemsList.getLast().getTimestamp().equals(end)) {
            HistoricItem last = endTime.equals(now) ? historicItemOrCurrentState(item, null)
                    : internalPersistedState(item, endTime, serviceId);
            if (last != null) {
                last = new RetimedHistoricItem(last, endTime);
                betweenItemsList.add(last);
        return !betweenItemsList.isEmpty() ? betweenItemsList : null;
    private static @Nullable PersistenceService getService(String serviceId) {
        PersistenceServiceRegistry reg = registry;
        return reg != null ? reg.get(serviceId) : null;
    private static @Nullable String getDefaultServiceId() {
        if (reg != null) {
            String id = reg.getDefaultId();
                        .warn("There is no default persistence service configured!");
                    .warn("PersistenceServiceRegistryImpl is not available!");
    private static @Nullable String getAlias(Item item, String serviceId) {
        PersistenceServiceConfigurationRegistry reg = configRegistry;
            PersistenceServiceConfiguration config = reg.get(serviceId);
            return config != null ? config.getAliases().get(item.getName()) : null;
    private static @Nullable DecimalType getItemValue(Item item) {
        if (baseItem instanceof NumberItem numberItem) {
            Unit<?> unit = numberItem.getUnit();
                QuantityType<?> qt = item.getStateAs(QuantityType.class);
                qt = (qt != null) ? qt.toUnit(unit) : qt;
                if (qt != null) {
                    return new DecimalType(qt.toBigDecimal());
        return item.getStateAs(DecimalType.class);
    private static @Nullable DecimalType getPersistedValue(HistoricItem historicItem, @Nullable Unit<?> unit) {
            if (state instanceof QuantityType<?> qtState) {
                qtState = qtState.toUnit(unit);
                if (qtState != null) {
                    state = qtState;
                            "Unit of state {} at time {} retrieved from persistence not compatible with item unit {} for item {}",
                            state, historicItem.getTimestamp(), unit, historicItem.getName());
                        "Item {} is QuantityType but state {} at time {} retrieved from persistence has no unit",
                        historicItem.getName(), historicItem.getState(), historicItem.getTimestamp());
        return state.as(DecimalType.class);
    private static @Nullable HistoricItem historicItemOrCurrentState(Item item, @Nullable HistoricItem historicItem) {
        if (historicItem == null) {
            // there are no historic states we couldn't determine a value, construct a HistoricItem from the current
            return new HistoricItem() {
                    return ZonedDateTime.now();
                    return item.getName();
    private static class RetimedHistoricItem implements HistoricItem {
        private final HistoricItem originItem;
        private final ZonedDateTime timestamp;
        public RetimedHistoricItem(HistoricItem originItem, ZonedDateTime timestamp) {
            this.originItem = originItem;
            return originItem.getState();
            return originItem.getName();
            return "RetimedHistoricItem [originItem=" + originItem + ", timestamp=" + timestamp + "]";
