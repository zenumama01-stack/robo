import org.openhab.core.common.SafeCallerBuilder;
import org.openhab.core.types.TimeSeries.Entry;
 * The {@link PersistenceManagerTest} contains tests for the {@link PersistenceManagerImpl}
public class PersistenceManagerTest {
    private static final String TEST_ITEM2_NAME = "testItem2";
    private static final String TEST_ITEM3_NAME = "testItem3";
    private static final String TEST_GROUP_ITEM_NAME = "groupItem";
    private static final String TEST_GROUP_ITEM2_BASE_NAME = "groupItem2Base";
    private static final String TEST_GROUP_ITEM2_NAME = "groupItem2";
    private static final StringItem TEST_ITEM = new StringItem(TEST_ITEM_NAME);
    private static final StringItem TEST_ITEM2 = new StringItem(TEST_ITEM2_NAME);
    private static final NumberItem TEST_ITEM3 = new NumberItem(TEST_ITEM3_NAME);
    private static final GroupItem TEST_GROUP_ITEM = new GroupItem(TEST_GROUP_ITEM_NAME);
    private static final NumberItem TEST_GROUP_ITEM2_BASE = new NumberItem(TEST_GROUP_ITEM2_BASE_NAME);
    private static final GroupItem TEST_GROUP_ITEM2 = new GroupItem(TEST_GROUP_ITEM2_NAME, TEST_GROUP_ITEM2_BASE,
            new GroupFunction.Equality());
    private static final State TEST_STATE = new StringType("testState1");
    private static final HistoricItem TEST_HISTORIC_ITEM = new HistoricItem() {
            return ZonedDateTime.now().minusDays(1);
            return TEST_STATE;
            return TEST_ITEM_NAME;
    private static final PersistedItem TEST_PERSISTED_ITEM = new PersistedItem() {
    private static final String TEST_PERSISTENCE_SERVICE_ID = "testPersistenceService";
    private static final String TEST_QUERYABLE_PERSISTENCE_SERVICE_ID = "testQueryablePersistenceService";
    private static final String TEST_MODIFIABLE_PERSISTENCE_SERVICE_ID = "testModifiablePersistenceService";
    private @Mock @NonNullByDefault({}) CronScheduler cronSchedulerMock;
    private @Mock @NonNullByDefault({}) Scheduler schedulerMock;
    private @Mock @NonNullByDefault({}) ScheduledCompletableFuture<Void> scheduledFutureMock;
    private @Mock @NonNullByDefault({}) SafeCaller safeCallerMock;
    private @Mock @NonNullByDefault({}) SafeCallerBuilder<@NonNull QueryablePersistenceService> safeCallerBuilderMock;
    private @Mock @NonNullByDefault({}) PersistenceService persistenceServiceMock;
    private @Mock @NonNullByDefault({}) QueryablePersistenceService queryablePersistenceServiceMock;
    private @Mock @NonNullByDefault({}) ModifiablePersistenceService modifiablePersistenceServiceMock;
    private @NonNullByDefault({}) PersistenceManagerImpl manager;
        TEST_GROUP_ITEM.addMember(TEST_ITEM);
        TEST_GROUP_ITEM.addMember(TEST_GROUP_ITEM2);
        TEST_GROUP_ITEM2.addMember(TEST_ITEM3);
        // set initial states
        TEST_ITEM.setState(UnDefType.NULL);
        TEST_ITEM2.setState(UnDefType.NULL);
        TEST_ITEM3.setState(DecimalType.ZERO);
        TEST_GROUP_ITEM.setState(UnDefType.NULL);
        TEST_GROUP_ITEM2.setState(DecimalType.ZERO);
        when(itemRegistryMock.getItem(TEST_GROUP_ITEM_NAME)).thenReturn(TEST_GROUP_ITEM);
        when(itemRegistryMock.getItem(TEST_GROUP_ITEM2_NAME)).thenReturn(TEST_GROUP_ITEM2);
        when(itemRegistryMock.getItem(TEST_ITEM_NAME)).thenReturn(TEST_ITEM);
        when(itemRegistryMock.getItem(TEST_ITEM2_NAME)).thenReturn(TEST_ITEM2);
        when(itemRegistryMock.getItem(TEST_ITEM3_NAME)).thenReturn(TEST_ITEM3);
        when(itemRegistryMock.getItems()).thenReturn(List.of(TEST_ITEM, TEST_ITEM2, TEST_ITEM3, TEST_GROUP_ITEM));
        when(persistenceServiceMock.getId()).thenReturn(TEST_PERSISTENCE_SERVICE_ID);
        when(queryablePersistenceServiceMock.getId()).thenReturn(TEST_QUERYABLE_PERSISTENCE_SERVICE_ID);
        when(queryablePersistenceServiceMock.query(any(), any())).thenReturn(List.of(TEST_HISTORIC_ITEM));
        when(queryablePersistenceServiceMock.persistedItem(any(), any())).thenReturn(TEST_PERSISTED_ITEM);
        when(modifiablePersistenceServiceMock.getId()).thenReturn(TEST_MODIFIABLE_PERSISTENCE_SERVICE_ID);
        manager = new PersistenceManagerImpl(cronSchedulerMock, schedulerMock, itemRegistryMock, safeCallerMock,
                readyServiceMock, persistenceServiceConfigurationRegistryMock);
        manager.addPersistenceService(persistenceServiceMock);
        manager.addPersistenceService(queryablePersistenceServiceMock);
        manager.addPersistenceService(modifiablePersistenceServiceMock);
        clearInvocations(persistenceServiceMock, queryablePersistenceServiceMock, modifiablePersistenceServiceMock);
    public void appliesToItemWithItemConfig() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceItemConfig(TEST_ITEM_NAME)),
                PersistenceStrategy.Globals.UPDATE, null);
        manager.stateUpdated(TEST_ITEM, TEST_STATE);
        verify(persistenceServiceMock).store(TEST_ITEM, null);
        verifyNoMoreInteractions(persistenceServiceMock);
    public void doesNotApplyToItemWithItemConfig() {
        manager.stateUpdated(TEST_ITEM2, TEST_STATE);
    public void appliesToGroupItemWithItemConfig() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceItemConfig(TEST_GROUP_ITEM_NAME)),
        manager.stateUpdated(TEST_GROUP_ITEM, TEST_STATE);
        verify(persistenceServiceMock).store(TEST_GROUP_ITEM, null);
    public void appliesToItemWithGroupConfig() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceGroupConfig(TEST_GROUP_ITEM_NAME)),
    public void doesNotApplyToItemWithGroupConfig() {
    public void appliesToItemWithAllConfig() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceAllConfig()),
        verify(persistenceServiceMock).store(TEST_ITEM2, null);
    public void doesNotApplyToItemWithGroupConfigAndItemExclusion() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceGroupConfig(TEST_GROUP_ITEM_NAME),
                new PersistenceItemExcludeConfig(TEST_ITEM_NAME)), PersistenceStrategy.Globals.UPDATE, null);
    public void doesNotApplyToItemWithAllConfigAndItemExclusion() {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID,
                List.of(new PersistenceAllConfig(), new PersistenceItemExcludeConfig(TEST_ITEM_NAME)),
    public void doesNotApplyToItemWithAllConfigAndGroupExclusion() {
                List.of(new PersistenceAllConfig(), new PersistenceGroupExcludeConfig(TEST_GROUP_ITEM_NAME)),
    public void doesNotApplyToNestedGroupItemWithAllConfigAndGroupExclusion() {
        manager.stateUpdated(TEST_ITEM3, DecimalType.ZERO);
        manager.stateUpdated(TEST_GROUP_ITEM2, DecimalType.ZERO);
    public void updatedStatePersistsEveryUpdate() {
        verify(persistenceServiceMock, times(2)).store(TEST_ITEM, null);
    public void updatedStateDoesNotPersistWithChangeStrategy() {
                PersistenceStrategy.Globals.CHANGE, null);
    public void changedStatePersistsWithChangeStrategy() {
        manager.stateChanged(TEST_ITEM, UnDefType.UNDEF, TEST_STATE);
    public void changedStateDoesNotPersistWithUpdateStrategy() {
    public void restoreOnStartupWhenItemNull() {
        setupPersistence(new PersistenceAllConfig());
        manager.onReadyMarkerAdded(new ReadyMarker("", ""));
        verify(readyServiceMock, timeout(1000)).markReady(any());
        assertThat(TEST_ITEM.getState(), is(TEST_STATE));
        assertThat(TEST_ITEM2.getState(), is(TEST_STATE));
        assertThat(TEST_GROUP_ITEM.getState(), is(TEST_STATE));
        verify(queryablePersistenceServiceMock, times(3)).persistedItem(any(), any());
        ZonedDateTime lastStateUpdate = TEST_ITEM.getLastStateUpdate();
        assertNotNull(lastStateUpdate);
        assertTrue(lastStateUpdate.isAfter(ZonedDateTime.now().minusDays(2)));
        assertTrue(lastStateUpdate.isBefore(ZonedDateTime.now().minusDays(1)));
        verifyNoMoreInteractions(queryablePersistenceServiceMock);
    public void noRestoreOnStartupWhenItemNotNull() {
        // set TEST_ITEM state to a value
        StringType initialValue = new StringType("value");
        TEST_ITEM.setState(initialValue);
        assertThat(TEST_ITEM.getState(), is(initialValue));
        verify(queryablePersistenceServiceMock, times(2)).persistedItem(any(), any());
        assertTrue(lastStateUpdate.isAfter(ZonedDateTime.now().minusHours(1)));
    public void storeTimeSeriesAndForecastsScheduled() {
        List<ScheduledCompletableFuture<?>> futures = new ArrayList<>();
        TestModifiablePersistenceService service = spy(new TestModifiablePersistenceService());
        manager.addPersistenceService(service);
        when(schedulerMock.at(any(SchedulerRunnable.class), any(Instant.class))).thenAnswer(i -> {
            ScheduledCompletableFuture<?> future = mock(ScheduledCompletableFuture.class);
            when(future.getScheduledTime()).thenReturn(((Instant) i.getArgument(1)).atZone(ZoneId.systemDefault()));
            futures.add(future);
        addConfiguration(TestModifiablePersistenceService.ID, List.of(new PersistenceAllConfig()),
                PersistenceStrategy.Globals.FORECAST, null);
        ZonedDateTime time0 = now.atZone(ZoneId.systemDefault()).minusSeconds(5000);
        Instant time1 = now.minusSeconds(1000);
        Instant time2 = now.plusSeconds(1000);
        Instant time3 = now.plusSeconds(2000);
        Instant time4 = now.plusSeconds(3000);
        // add elements
        timeSeries.add(time1, new StringType("one"));
        timeSeries.add(time2, new StringType("two"));
        timeSeries.add(time3, new StringType("three"));
        timeSeries.add(time4, new StringType("four"));
        TEST_ITEM.setState(new StringType("zero"), null, time0, null, null);
        manager.timeSeriesUpdated(TEST_ITEM, timeSeries);
        InOrder inOrder = inOrder(service, schedulerMock);
        // verify elements are stored
        timeSeries.getStates().forEach(entry -> inOrder.verify(service).store(any(Item.class),
                eq(entry.timestamp().atZone(ZoneId.systemDefault())), eq(entry.state())));
        // first element not scheduled, because it is in the past, check if second is scheduled
        inOrder.verify(schedulerMock).at(any(SchedulerRunnable.class), eq(time2));
        // allow any number of getId() calls
        inOrder.verify(service, atLeast(0)).getId();
        inOrder.verifyNoMoreInteractions();
        // check if timeseries element in the past updated item state
        Entry firstEntry = timeSeries.getStates().findFirst().get();
        assertThat(TEST_ITEM.getState(), is(firstEntry.state()));
        assertThat(TEST_ITEM.getLastState(), is(new StringType("zero")));
        assertThat(lastStateUpdate.toInstant(), is(firstEntry.timestamp()));
        ZonedDateTime lastStateChange = TEST_ITEM.getLastStateChange();
        assertNotNull(lastStateChange);
        assertThat(lastStateChange.toInstant(), is(firstEntry.timestamp()));
        // Check if other persistence services got updated
        verify(persistenceServiceMock, atLeast(0)).getId();
        // replace elements
        TimeSeries timeSeries2 = new TimeSeries(TimeSeries.Policy.REPLACE);
        timeSeries2.add(time3, new StringType("three2"));
        timeSeries2.add(time4, new StringType("four2"));
        manager.timeSeriesUpdated(TEST_ITEM, timeSeries2);
        // verify removal of old elements from service
        ArgumentCaptor<FilterCriteria> filterCaptor = ArgumentCaptor.forClass(FilterCriteria.class);
        inOrder.verify(service).remove(filterCaptor.capture());
        FilterCriteria filterCriteria = filterCaptor.getValue();
        assertThat(filterCriteria.getItemName(), is(TEST_ITEM_NAME));
        assertThat(filterCriteria.getBeginDate(), is(time3.atZone(ZoneId.systemDefault())));
        assertThat(filterCriteria.getEndDate(), is(time4.atZone(ZoneId.systemDefault())));
        // verify restore future is not cancelled
        verify(futures.getFirst(), never()).cancel(anyBoolean());
        // verify new values are stored
        inOrder.verify(service, times(2)).store(any(Item.class), any(ZonedDateTime.class), any(State.class));
        // try adding a new element in front and check it is correctly scheduled
        Instant time5 = Instant.now().plusSeconds(500);
        TimeSeries timeSeries3 = new TimeSeries(TimeSeries.Policy.ADD);
        timeSeries3.add(time5, new StringType("five"));
        manager.timeSeriesUpdated(TEST_ITEM, timeSeries3);
        // verify old restore future is cancelled
        inOrder.verify(service, times(1)).store(any(Item.class), any(ZonedDateTime.class), any(State.class));
        verify(futures.getFirst()).cancel(true);
        // verify new restore future is properly created
        inOrder.verify(schedulerMock).at(any(SchedulerRunnable.class), eq(time5));
    public void externalPersistenceDataChangeIsHandled() {
        addConfiguration(TEST_QUERYABLE_PERSISTENCE_SERVICE_ID, List.of(new PersistenceAllConfig()),
        manager.handleExternalPersistenceDataChange(persistenceServiceMock, TEST_ITEM);
        assertNotEquals(TEST_STATE, TEST_ITEM.getState());
        manager.handleExternalPersistenceDataChange(queryablePersistenceServiceMock, TEST_ITEM);
        verify(queryablePersistenceServiceMock).persistedItem(eq(TEST_ITEM_NAME), any());
        assertEquals(TEST_STATE, TEST_ITEM.getState());
    public void cronStrategyIsScheduledAndCancelledAndPersistsValue() throws Exception {
        ArgumentCaptor<SchedulerRunnable> runnableCaptor = ArgumentCaptor.forClass(SchedulerRunnable.class);
        when(cronSchedulerMock.schedule(runnableCaptor.capture(), any())).thenReturn(scheduledFutureMock);
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(new PersistenceItemConfig(TEST_ITEM3_NAME)),
                new PersistenceCronStrategy("withoutFilter", "0 0 * * * ?"), null);
        addConfiguration(TEST_QUERYABLE_PERSISTENCE_SERVICE_ID, List.of(new PersistenceItemConfig(TEST_ITEM3_NAME)),
                new PersistenceCronStrategy("withFilter", "0 * * * * ?"),
                new PersistenceThresholdFilter("test", BigDecimal.TEN, "", false));
        List<SchedulerRunnable> runnables = runnableCaptor.getAllValues();
        assertThat(runnables.size(), is(2));
        runnables.getFirst().run();
        runnables.get(1).run();
        manager.deactivate();
        verify(cronSchedulerMock, times(2)).schedule(any(), any());
        verify(scheduledFutureMock, times(2)).cancel(true);
        // no filter - persist everything
        verify(persistenceServiceMock, times(2)).store(TEST_ITEM3, null);
        // filter - persist filtered value
        verify(queryablePersistenceServiceMock, times(1)).store(TEST_ITEM3, null);
    public void cronStrategyIsProperlyUpdated() {
        when(cronSchedulerMock.schedule(any(), any())).thenReturn(scheduledFutureMock);
        PersistenceServiceConfiguration configuration = addConfiguration(TEST_PERSISTENCE_SERVICE_ID,
                List.of(new PersistenceItemConfig(TEST_ITEM_NAME)),
                new PersistenceCronStrategy("everyHour", "0 0 * * * ?"), null);
        manager.updated(configuration, configuration);
    public void filterAppliesOnStateUpdate() {
                PersistenceStrategy.Globals.UPDATE, new PersistenceThresholdFilter("test", BigDecimal.TEN, "", false));
        verify(persistenceServiceMock, times(1)).store(TEST_ITEM3, null);
     * Add a configuration for restoring TEST_ITEM and mock the SafeCaller
    private void setupPersistence(PersistenceConfig itemConfig) {
        addConfiguration(TEST_PERSISTENCE_SERVICE_ID, List.of(itemConfig), PersistenceStrategy.Globals.RESTORE, null);
        addConfiguration(TEST_QUERYABLE_PERSISTENCE_SERVICE_ID, List.of(itemConfig),
                PersistenceStrategy.Globals.RESTORE, null);
        when(safeCallerMock.create(queryablePersistenceServiceMock, QueryablePersistenceService.class))
                .thenReturn(safeCallerBuilderMock);
        when(safeCallerBuilderMock.onTimeout(any())).thenReturn(safeCallerBuilderMock);
        when(safeCallerBuilderMock.onException(any())).thenReturn(safeCallerBuilderMock);
        when(safeCallerBuilderMock.build()).thenReturn(queryablePersistenceServiceMock);
     * Add a configuration to the manager
     * @param serviceId the persistence service id
     * @param itemConfigs list item configurations
     * @param strategy the strategy
     * @param filter a persistence filter
     * @return the added strategy
    private PersistenceServiceConfiguration addConfiguration(String serviceId, List<PersistenceConfig> itemConfigs,
            PersistenceStrategy strategy, @Nullable PersistenceFilter filter) {
        List<PersistenceFilter> filters = filter != null ? List.of(filter) : List.of();
        PersistenceItemConfiguration itemConfiguration = new PersistenceItemConfiguration(itemConfigs,
                List.of(strategy), filters);
        List<PersistenceStrategy> strategies = PersistenceStrategy.Globals.STRATEGIES.containsValue(strategy)
                ? List.of()
                : List.of(strategy);
        PersistenceServiceConfiguration serviceConfiguration = new PersistenceServiceConfiguration(serviceId,
                List.of(itemConfiguration), Map.of(), strategies, filters);
        manager.added(serviceConfiguration);
        return serviceConfiguration;
    private static class TestModifiablePersistenceService implements ModifiablePersistenceService {
        public static final String ID = "TMPS";
        private final Map<ZonedDateTime, State> states = new HashMap<>();
            states.put(date, state);
            store(item, date, state);
            ZonedDateTime begin = Objects.requireNonNull(filter.getBeginDate());
            ZonedDateTime end = Objects.requireNonNull(filter.getEndDate());
            List<ZonedDateTime> keys = states.keySet().stream().filter(t -> t.isAfter(begin) && t.isBefore(end))
            keys.forEach(states::remove);
            return !keys.isEmpty();
            ZonedDateTime begin = filter.getBeginDate();
            ZonedDateTime end = filter.getEndDate();
            List<ZonedDateTime> keys = states.keySet().stream()
                    .filter(t -> (begin == null || t.isAfter(begin)) && (end == null || t.isBefore(end))).toList();
            return states.entrySet().stream().filter(e -> keys.contains(e.getKey()))
                    .<HistoricItem> map(e -> new HistoricItem() {
                            return e.getKey();
                            return e.getValue();
                            return "item";
