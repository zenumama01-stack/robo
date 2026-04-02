package org.openhab.core.persistence.internal;
import static org.openhab.core.persistence.FilterCriteria.Ordering.ASCENDING;
import static org.openhab.core.persistence.strategy.PersistenceStrategy.Globals.*;
import org.openhab.core.common.SafeCaller;
import org.openhab.core.items.StateChangeListener;
import org.openhab.core.items.TimeSeriesListener;
import org.openhab.core.persistence.PersistedItem;
import org.openhab.core.persistence.registry.PersistenceServiceConfigurationRegistryChangeListener;
 * This class implements a persistence manager to manage all persistence services etc.
 * @author Markus Rathgeb - Separation of persistence core and model, drop Quartz usage.
 * @author Jan N. Klug - Refactored to use service configuration registry
 * @author Jan N. Klug - Added time series support
 * @author Mark Herwege - Added restoring lastState, lastStateChange and lastStateUpdate
 * @author Mark Herwege - Fix and enhance handling of time series and external persistence updates
@Component(immediate = true, service = PersistenceManager.class)
public class PersistenceManagerImpl implements ItemRegistryChangeListener, StateChangeListener, ReadyTracker,
        PersistenceServiceConfigurationRegistryChangeListener, TimeSeriesListener, PersistenceManager {
    private static final String PERSISTENCE_SOURCE = "org.openhab.core.persistence";
    private final Logger logger = LoggerFactory.getLogger(PersistenceManagerImpl.class);
    private final ReadyMarker marker = new ReadyMarker("persistence", "restore");
    // the scheduler used for timer events
    private final CronScheduler cronScheduler;
    private final SafeCaller safeCaller;
    private volatile boolean started = false;
    private final Map<String, PersistenceServiceContainer> persistenceServiceContainers = new ConcurrentHashMap<>();
    public PersistenceManagerImpl(final @Reference CronScheduler cronScheduler, final @Reference Scheduler scheduler,
            final @Reference ItemRegistry itemRegistry, final @Reference SafeCaller safeCaller,
            final @Reference ReadyService readyService,
            final @Reference PersistenceServiceConfigurationRegistry persistenceServiceConfigurationRegistry) {
        this.cronScheduler = cronScheduler;
        this.safeCaller = safeCaller;
        persistenceServiceConfigurationRegistry.addRegistryChangeListener(this);
        itemRegistry.removeRegistryChangeListener(this);
        persistenceServiceConfigurationRegistry.removeRegistryChangeListener(this);
        persistenceServiceContainers.values().forEach(PersistenceServiceContainer::cancelPersistJobs);
        persistenceServiceContainers.values().forEach(PersistenceServiceContainer::cancelForecastJobs);
        // remove item state change listeners
        itemRegistry.stream().filter(GenericItem.class::isInstance)
                .forEach(item -> ((GenericItem) item).removeStateChangeListener(this));
    protected void addPersistenceService(PersistenceService persistenceService) {
        String serviceId = persistenceService.getId();
        logger.debug("Initializing {} persistence service.", serviceId);
        PersistenceServiceContainer container = new PersistenceServiceContainer(persistenceService,
                persistenceServiceConfigurationRegistry.get(serviceId));
        PersistenceServiceContainer oldContainer = persistenceServiceContainers.put(serviceId, container);
        if (oldContainer != null) { // cancel all jobs if the persistence service is set and an old configuration is
                                    // already present
            oldContainer.cancelPersistJobs();
            oldContainer.cancelForecastJobs();
            startEventHandling(container);
    protected void removePersistenceService(PersistenceService persistenceService) {
        PersistenceServiceContainer container = persistenceServiceContainers.remove(persistenceService.getId());
            container.cancelPersistJobs();
            container.cancelForecastJobs();
     * Calls all persistence services which use change or update policy for the given item
     * @param item the item to persist
     * @param changed true, if it has the change strategy, false otherwise
    private void handleStateEvent(Item item, boolean changed) {
        PersistenceStrategy changeStrategy = changed ? PersistenceStrategy.Globals.CHANGE
                : PersistenceStrategy.Globals.UPDATE;
        persistenceServiceContainers.values().forEach(container -> storeItem(container, item, changeStrategy));
    private void storeItem(PersistenceServiceContainer container, Item item, PersistenceStrategy changeStrategy) {
        container.getMatchingConfigurations(changeStrategy).filter(itemConfig -> appliesToItem(itemConfig, item))
                .filter(itemConfig -> itemConfig.filters().stream().allMatch(filter -> filter.apply(item)))
                .forEach(itemConfig -> {
                    itemConfig.filters().forEach(filter -> filter.persisted(item));
                    container.getPersistenceService().store(item, container.getAlias(item));
     * Checks if a given persistence configuration entry is relevant for an item
     * @param itemConfig the persistence configuration entry
     * @param item to check if the configuration applies to
     * @return true, if the configuration applies to the item
    private boolean appliesToItem(PersistenceItemConfiguration itemConfig, Item item) {
        boolean applies = false;
        for (PersistenceConfig itemCfg : itemConfig.items()) {
            if (itemCfg instanceof PersistenceAllConfig) {
                applies = true;
            } else if (itemCfg instanceof PersistenceItemConfig persistenceItemConfig) {
                if (item.getName().equals(persistenceItemConfig.getItem())) {
            } else if (itemCfg instanceof PersistenceItemExcludeConfig persistenceItemExcludeConfig) {
                if (item.getName().equals(persistenceItemExcludeConfig.getItem())) {
            } else if (itemCfg instanceof PersistenceGroupConfig persistenceGroupConfig) {
                    Item gItem = itemRegistry.getItem(persistenceGroupConfig.getGroup());
                    if (gItem instanceof GroupItem gItem2 && gItem2.getAllStateMembers().contains(item)) {
            } else if (itemCfg instanceof PersistenceGroupExcludeConfig persistenceGroupExcludeConfig) {
                    Item gItem = itemRegistry.getItem(persistenceGroupExcludeConfig.getGroup());
        return applies;
     * Retrieves all items for which the persistence configuration applies to.
     * @param config the persistence configuration entry
     * @return all items that this configuration applies to
    private Iterable<Item> getAllItems(PersistenceItemConfiguration config) {
        Set<Item> excludeItems = new HashSet<>();
        for (Object itemCfg : config.items()) {
                items.addAll(itemRegistry.getItems());
                String itemName = persistenceItemConfig.getItem();
                    items.add(itemRegistry.getItem(itemName));
                    logger.debug("Item '{}' does not exist.", itemName);
                String groupName = persistenceGroupConfig.getGroup();
                    Item gItem = itemRegistry.getItem(groupName);
                    if (gItem instanceof GroupItem groupItem) {
                        items.addAll(groupItem.getAllStateMembers());
                    logger.debug("Item group '{}' does not exist.", groupName);
            } else if (itemCfg instanceof PersistenceItemExcludeConfig persistenceItemConfig) {
                    excludeItems.add(itemRegistry.getItem(itemName));
            } else if (itemCfg instanceof PersistenceGroupExcludeConfig persistenceGroupConfig) {
                        excludeItems.addAll(groupItem.getAllStateMembers());
        items.removeAll(excludeItems);
    private void startEventHandling(PersistenceServiceContainer serviceContainer) {
        serviceContainer.restoreStatesAndScheduleForecastJobs();
        serviceContainer.schedulePersistJobs();
    // ItemStateChangeListener methods
        addPersistenceListeners(oldItemNames);
        addToPersistenceServiceContainer(oldItemNames);
    public void addPersistenceListeners(Collection<String> oldItemNames) {
        itemRegistry.getItems().forEach(this::addItemToPersistenceListeners);
    public void addToPersistenceServiceContainer(Collection<String> oldItemNames) {
        itemRegistry.getItems().forEach(this::addItemToPersistenceServiceContainer);
    public void added(Item item) {
        addItemToPersistenceListeners(item);
        addItemToPersistenceServiceContainer(item);
    public void addItemToPersistenceServiceContainer(Item item) {
        persistenceServiceContainers.values().forEach(container -> container.addItem(item));
    public void addItemToPersistenceListeners(Item item) {
            genericItem.addStateChangeListener(this);
            genericItem.addTimeSeriesListener(this);
    public void removed(Item item) {
        persistenceServiceContainers.values().forEach(container -> container.removeItem(item.getName()));
            genericItem.removeStateChangeListener(this);
            genericItem.removeTimeSeriesListener(this);
    public void updated(Item oldItem, Item item) {
        removed(oldItem);
        added(item);
    public void stateChanged(Item item, State oldState, State newState) {
        handleStateEvent(item, true);
    public void stateUpdated(Item item, State state) {
        handleStateEvent(item, false);
    public void timeSeriesUpdated(Item item, TimeSeries timeSeries) {
        if (timeSeries.size() == 0) {
            // discard empty time series
        persistenceServiceContainers.values().stream()
                .filter(psc -> psc.persistenceService instanceof ModifiablePersistenceService)
                .forEach(container -> Stream
                        .concat(container.getMatchingConfigurations(UPDATE),
                                container.getMatchingConfigurations(FORECAST))
                        .distinct().filter(itemConfig -> appliesToItem(itemConfig, item)).forEach(itemConfig -> {
                            ModifiablePersistenceService service = (ModifiablePersistenceService) container
                                    .getPersistenceService();
                            // remove old values if replace selected
                                ZonedDateTime begin = timeSeries.getBegin().atZone(ZoneId.systemDefault());
                                ZonedDateTime end = timeSeries.getEnd().atZone(ZoneId.systemDefault());
                                FilterCriteria removeFilter = new FilterCriteria().setItemName(item.getName())
                                        .setBeginDate(begin).setEndDate(end);
                                service.remove(removeFilter, container.getAlias(item));
                                ScheduledCompletableFuture<?> forecastJob = container.forecastJobs.get(item.getName());
                                if (forecastJob != null && forecastJob.getScheduledTime().isAfter(begin)
                                        && forecastJob.getScheduledTime().isBefore(end)) {
                                    forecastJob.cancel(true);
                                    container.forecastJobs.remove(item.getName());
                            // store time series
                            timeSeries.getStates().forEach(e -> service.store(item,
                                    e.timestamp().atZone(ZoneId.systemDefault()), e.state(), container.getAlias(item)));
                            // update item states in the future
                            timeSeries.getStates().filter(s -> s.timestamp().isAfter(now)).findFirst().ifPresent(s -> {
                                if (forecastJob == null || forecastJob.getScheduledTime()
                                        .isAfter(s.timestamp().atZone(ZoneId.systemDefault()))) {
                                    container.scheduleNextForecastForItem(item, s.timestamp(), s.state());
                            // update current item state if last entry in the past in time series is after last update
                            // of item
                            timeSeries.getStates().filter(s -> s.timestamp().isBefore(now))
                                    .max(Comparator.comparing(TimeSeries.Entry::timestamp)).ifPresent(s -> {
                                        ZonedDateTime lastStateUpdate = item.getLastStateUpdate();
                                        ZonedDateTime timestamp = s.timestamp().atZone(ZoneId.systemDefault());
                                        if (lastStateUpdate == null || timestamp.isAfter(lastStateUpdate)) {
                                            container.restoreItemStateFromTimeSeriesEntry(item, timestamp, s.state());
        ExecutorService scheduler = Executors.newSingleThreadExecutor(new NamedThreadFactory("persistenceManager"));
        scheduler.submit(() -> {
            allItemsChanged(Set.of());
            persistenceServiceContainers.values().forEach(this::startEventHandling);
            itemRegistry.addRegistryChangeListener(this);
    public void added(PersistenceServiceConfiguration element) {
        PersistenceServiceContainer container = persistenceServiceContainers.get(element.getUID());
            container.setConfiguration(element);
    public void removed(PersistenceServiceConfiguration element) {
            container.setConfiguration(null);
    public void updated(PersistenceServiceConfiguration oldElement, PersistenceServiceConfiguration element) {
        // no need to remove before, configuration is overwritten if possible
        added(element);
    public void handleExternalPersistenceDataChange(PersistenceService persistenceService, Item item) {
        if (!(persistenceService instanceof QueryablePersistenceService)) {
                .filter(container -> container.persistenceService.equals(persistenceService) && Stream
                                Stream.concat(container.getMatchingConfigurations(CHANGE),
                                        container.getMatchingConfigurations(FORECAST)))
                        .distinct().anyMatch(itemConf -> appliesToItem(itemConf, item)))
                .forEach(container -> {
                    container.restoreItemStateFromPersistenceUpdate(item);
                    container.scheduleNextPersistedForecastForItem(item);
    private void storeInOtherServices(PersistenceService persistenceService, Item item, State oldState) {
        boolean changed = !item.getState().equals(oldState);
                .filter(container -> !container.persistenceService.equals(persistenceService)).forEach(container -> {
                        storeItem(container, item, PersistenceStrategy.Globals.CHANGE);
                    storeItem(container, item, PersistenceStrategy.Globals.UPDATE);
    private class PersistenceServiceContainer {
        private final PersistenceService persistenceService;
        private final Set<ScheduledCompletableFuture<?>> persistJobs = new HashSet<>();
        private final Map<String, ScheduledCompletableFuture<?>> forecastJobs = new ConcurrentHashMap<>();
        private final Map<PersistenceStrategy, Collection<PersistenceItemConfiguration>> strategyCache = new ConcurrentHashMap<>();
        private PersistenceServiceConfiguration configuration;
        public PersistenceServiceContainer(PersistenceService persistenceService,
                @Nullable PersistenceServiceConfiguration configuration) {
            this.persistenceService = persistenceService;
            this.configuration = Objects.requireNonNullElseGet(configuration, this::getEmptyConfig);
        public PersistenceService getPersistenceService() {
            return persistenceService;
         * Set a new configuration for this persistence service (also cancels all cron jobs)
         * @param configuration the new {@link PersistenceServiceConfiguration}, if {@code null} all configuration will
         *            be removed
        public void setConfiguration(@Nullable PersistenceServiceConfiguration configuration) {
            cancelPersistJobs();
            cancelForecastJobs();
            strategyCache.clear();
         * Get all item configurations from this service that match a certain strategy
         * @param strategy the {@link PersistenceStrategy} to look for
         * @return a {@link Stream<PersistenceItemConfiguration>} of the result
        public Stream<PersistenceItemConfiguration> getMatchingConfigurations(PersistenceStrategy strategy) {
            return Objects.requireNonNull(strategyCache.computeIfAbsent(strategy, s -> {
                return configuration.getConfigs().stream()
                        .filter(itemConfig -> itemConfig.strategies().contains(strategy)).toList();
            })).stream();
        public @Nullable String getAlias(Item item) {
            return configuration.getAliases().get(item.getName());
        private PersistenceServiceConfiguration getEmptyConfig() {
            return new PersistenceServiceConfiguration(persistenceService.getId(), List.of(), Map.of(), List.of(),
                    List.of());
         * Cancel all scheduled cron jobs / strategies for this service
        public void cancelPersistJobs() {
            synchronized (persistJobs) {
                persistJobs.forEach(job -> job.cancel(true));
                persistJobs.clear();
            logger.debug("Removed scheduled cron jobs for persistence service '{}'", configuration.getUID());
        public void cancelForecastJobs() {
            synchronized (forecastJobs) {
                forecastJobs.values().forEach(job -> job.cancel(true));
                forecastJobs.clear();
            logger.debug("Removed scheduled forecast jobs for persistence service '{}'", configuration.getUID());
         * Schedule all necessary cron jobs / strategies for this service
        public void schedulePersistJobs() {
            configuration.getStrategies().stream().filter(PersistenceCronStrategy.class::isInstance)
                    .forEach(strategy -> {
                        PersistenceCronStrategy cronStrategy = (PersistenceCronStrategy) strategy;
                        String cronExpression = cronStrategy.getCronExpression();
                        List<PersistenceItemConfiguration> itemConfigs = getMatchingConfigurations(strategy).toList();
                        persistJobs.add(cronScheduler.schedule(() -> persistJob(itemConfigs), cronExpression));
                        logger.debug("Scheduled strategy {} with cron expression {} for service {}",
                                cronStrategy.getName(), cronExpression, configuration.getUID());
        public void restoreStatesAndScheduleForecastJobs() {
            itemRegistry.getItems().forEach(this::addItem);
        public void addItem(Item item) {
            if (persistenceService instanceof QueryablePersistenceService) {
                if (UnDefType.NULL.equals(item.getState())
                        && (getMatchingConfigurations(RESTORE)
                                .anyMatch(configuration -> appliesToItem(configuration, item)))
                        || getMatchingConfigurations(FORECAST)
                                .anyMatch(configuration -> appliesToItem(configuration, item))) {
                    restoreItemStateOnStartup(item);
                if (getMatchingConfigurations(FORECAST).anyMatch(configuration -> appliesToItem(configuration, item))) {
                    scheduleNextPersistedForecastForItem(item);
        public void removeItem(String itemName) {
            ScheduledCompletableFuture<?> job = forecastJobs.remove(itemName);
            if (job != null) {
        private @Nullable PersistedItem getPersistedItem(Item item) {
            QueryablePersistenceService queryService = (QueryablePersistenceService) persistenceService;
            String alias = getAlias(item);
            PersistedItem persistedItem = safeCaller.create(queryService, QueryablePersistenceService.class)
                    .onTimeout(
                            () -> logger.warn("Querying persistence service '{}' to restore '{}' takes more than {}ms.",
                                    queryService.getId(), item.getName(), SafeCaller.DEFAULT_TIMEOUT))
                    .onException(e -> logger.error(
                            "Exception occurred while querying persistence service '{}' to restore '{}': {}",
                            queryService.getId(), item.getName(), e.getMessage(), e))
                    .build().persistedItem(item.getName(), alias);
            return persistedItem;
        public void scheduleNextForecastForItem(Item item, Instant time, State state) {
            ScheduledFuture<?> oldJob = forecastJobs.remove(itemName);
            if (oldJob != null) {
                oldJob.cancel(true);
            forecastJobs.put(itemName, scheduler.at(() -> {
                restoreItemStateFromTimeSeriesEntry(item, time.atZone(ZoneId.systemDefault()), state);
            }, time));
            logger.trace("Scheduled forecasted value for {} at {}", item.getName(), time);
        public void scheduleNextPersistedForecastForItem(Item item) {
            if (item instanceof GenericItem) {
                FilterCriteria filter = new FilterCriteria().setItemName(item.getName())
                        .setBeginDate(ZonedDateTime.now()).setOrdering(ASCENDING);
                Iterator<HistoricItem> result = safeCaller.create(queryService, QueryablePersistenceService.class)
                        .onTimeout(() -> logger.warn("Querying persistence service '{}' takes more than {}ms.",
                                queryService.getId(), SafeCaller.DEFAULT_TIMEOUT))
                        .onException(e -> logger.error("Exception occurred while querying persistence service '{}': {}",
                                queryService.getId(), e.getMessage(), e))
                        .build().query(filter, alias).iterator();
                while (result.hasNext()) {
                    HistoricItem next = result.next();
                    Instant timestamp = next.getInstant();
                    if (timestamp.isAfter(Instant.now())) {
                        scheduleNextForecastForItem(item, timestamp, next.getState());
        private void restoreItemStateOnStartup(Item item) {
            PersistedItem persistedItem = getPersistedItem(item);
            if (persistedItem == null) {
                // in case of an exception or timeout, the safe caller returns null
            PersistedItem newItemState = itemState(item, persistedItem);
            if (newItemState == null) {
            GenericItem genericItem = (GenericItem) item;
            genericItem.removeStateChangeListener(PersistenceManagerImpl.this);
                genericItem.setState(newItemState.getState(), newItemState.getLastState(), newItemState.getTimestamp(),
                        newItemState.getLastStateChange(), PERSISTENCE_SOURCE);
                genericItem.addStateChangeListener(PersistenceManagerImpl.this);
                logger.debug("Restored item state from '{}' for item '{}' -> '{}'",
                        DateTimeFormatter.ISO_ZONED_DATE_TIME.format(persistedItem.getTimestamp()), item.getName(),
                        persistedItem.getState());
        private void restoreItemStateFromTimeSeriesEntry(Item item, ZonedDateTime timestamp, State state) {
            PersistedItem persistedItem = new PersistedItem() {
            restoreItemStateFromPersistenceUpdate(item, persistedItem);
        public void restoreItemStateFromPersistenceUpdate(Item item) {
            if (persistedItem != null) {
        private void restoreItemStateFromPersistenceUpdate(Item item, PersistedItem persistedItem) {
            State oldState = item.getState();
                // other services with update or change strategy should persist new state
                storeInOtherServices(persistenceService, item, oldState);
                logger.debug("Reset item state from '{}' for item '{}' -> '{}'",
        private @Nullable PersistedItem itemState(Item item, PersistedItem persistedItem) {
            ZonedDateTime itemLastStateUpdate = item.getLastStateUpdate();
            ZonedDateTime persistedItemTimestamp = persistedItem.getTimestamp();
            if (itemState != UnDefType.NULL && itemLastStateUpdate != null
                    && persistedItemTimestamp.isBefore(itemLastStateUpdate)) {
            State persistedItemState = persistedItem.getState();
            State persistedItemLastState = persistedItem.getLastState();
            ZonedDateTime persistedItemLastStateChange = persistedItem.getLastStateChange();
            State itemLastState = item.getLastState();
            ZonedDateTime itemLastStateChange = item.getLastStateChange();
            State state;
            ZonedDateTime lastStateUpdate = persistedItemTimestamp;
            State lastState;
            ZonedDateTime lastStateChange;
            if (itemState.equals(persistedItemState)) {
                state = itemState;
                lastState = itemLastState;
                lastStateChange = (persistedItemLastStateChange != null && itemLastStateChange != null
                        && persistedItemLastStateChange.isAfter(itemLastStateChange)) ? persistedItemLastStateChange
                                : itemLastStateChange;
                state = persistedItemState;
                if (itemState == UnDefType.NULL || (persistedItemLastStateChange != null
                        && persistedItemLastState != null && itemLastStateUpdate != null
                        && persistedItemLastStateChange.isAfter(itemLastStateUpdate))) {
                    lastState = persistedItemLastState;
                    lastStateChange = persistedItemLastStateChange;
                    lastState = itemState;
                    lastStateChange = persistedItemTimestamp;
            // Check again if item has not been updated in the mean time before commit
            itemLastStateUpdate = item.getLastStateUpdate();
                    return lastStateChange;
                    return lastState;
        private void persistJob(List<PersistenceItemConfiguration> itemConfigs) {
            itemConfigs.forEach(itemConfig -> {
                for (Item item : getAllItems(itemConfig)) {
                    if (itemConfig.filters().stream().allMatch(filter -> filter.apply(item))) {
                        persistenceService.store(item, getAlias(item));
                        logger.trace("Storing item '{}' with persistence service '{}' took {}ms", item.getName(),
                                configuration.getUID(), TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - startTime));
