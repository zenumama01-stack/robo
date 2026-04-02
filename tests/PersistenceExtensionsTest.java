import static org.hamcrest.Matchers.closeTo;
import static org.openhab.core.persistence.extensions.TestPersistenceService.*;
import java.util.stream.DoubleStream;
import org.openhab.core.library.types.ArithmeticGroupFunction;
import org.openhab.core.persistence.extensions.PersistenceExtensions.RiemannType;
import org.openhab.core.types.TimeSeries.Policy;
 * @author Jan N. Klug - Fix averageSince calculation
 * @author Jan N. Klug - Interval method tests and refactoring
 * @author Mark Herwege - Handle persisted GroupItem with QuantityType
 * @author Mark Herwege - Add median methods
 * @author Mark Herwege - Add Riemann sum methods
 * @author Mark Herwege - Make tests less impacted by the current time for slow builds, improves test reliability
public class PersistenceExtensionsTest {
    public static final String TEST_NUMBER = "testNumber";
    public static final String TEST_QUANTITY_NUMBER = "testQuantityNumber";
    public static final String TEST_GROUP_QUANTITY_NUMBER = "testGroupQuantityNumber";
    public static final String TEST_SWITCH = "testSwitch";
    public static final double KELVIN_OFFSET = 273.15;
    private @Mock @NonNullByDefault({}) PersistenceManager persistenceManagerMock;
    private @NonNullByDefault({}) GenericItem numberItem, quantityItem, groupQuantityItem, switchItem;
        numberItem = itemFactory.createItem(CoreItemFactory.NUMBER, TEST_NUMBER);
        quantityItem = itemFactory.createItem(CoreItemFactory.NUMBER + ItemUtil.EXTENSION_SEPARATOR + "Temperature",
                TEST_QUANTITY_NUMBER);
        switchItem = itemFactory.createItem(CoreItemFactory.SWITCH, TEST_SWITCH);
        GenericItem baseItem = itemFactory
                .createItem(CoreItemFactory.NUMBER + ItemUtil.EXTENSION_SEPARATOR + "Temperature", "testGroupBaseItem");
        GroupItem groupItem = new GroupItem(TEST_GROUP_QUANTITY_NUMBER, baseItem, new ArithmeticGroupFunction.Sum());
        groupItem.addMember(quantityItem);
        groupQuantityItem = groupItem;
        numberItem.setState(STATE);
        quantityItem.setState(new QuantityType<Temperature>(STATE, SIUnits.CELSIUS));
        groupQuantityItem.setState(new QuantityType<Temperature>(STATE, SIUnits.CELSIUS));
        switchItem.setState(SWITCH_STATE);
        when(itemRegistryMock.get(TEST_NUMBER)).thenReturn(numberItem);
        when(itemRegistryMock.get(TEST_QUANTITY_NUMBER)).thenReturn(quantityItem);
        when(itemRegistryMock.get(TEST_SWITCH)).thenReturn(switchItem);
        when(itemRegistryMock.get(TEST_GROUP_QUANTITY_NUMBER)).thenReturn(groupQuantityItem);
        when(persistenceServiceConfigurationRegistryMock.get(anyString())).thenReturn(null);
        new PersistenceExtensions(persistenceManagerMock, new PersistenceServiceRegistry() {
            private final PersistenceService testPersistenceService = new TestPersistenceService(itemRegistryMock);
                // not available
                return Set.of(testPersistenceService);
                return TestPersistenceService.SERVICE_ID.equals(serviceId) ? testPersistenceService : null;
        }, persistenceServiceConfigurationRegistryMock, timeZoneProviderMock);
    public void testPersistedStateDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.persistedState(numberItem,
                ZonedDateTime.of(HISTORIC_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertNotNull(historicItem);
        assertThat(historicItem.getState(), is(instanceOf(DecimalType.class)));
        assertEquals(value(HISTORIC_END), historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(numberItem,
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 12, 31, 0, 0, 0, 0, ZoneId.systemDefault()),
                SERVICE_ID);
        assertEquals(value(HISTORIC_INTERMEDIATE_VALUE_1), historicItem.getState());
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        historicItem = PersistenceExtensions.persistedState(numberItem, ZonedDateTime.now(), SERVICE_ID);
        assertEquals(value(TestPersistenceService.HISTORIC_END), historicItem.getState());
                ZonedDateTime.of(FUTURE_INTERMEDIATE_NOVALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
                ZonedDateTime.of(FUTURE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertEquals(value(FUTURE_START), historicItem.getState());
                ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertEquals(value(FUTURE_INTERMEDIATE_VALUE_3), historicItem.getState());
                ZonedDateTime.of(FUTURE_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertEquals(value(FUTURE_END), historicItem.getState());
                ZonedDateTime.of(AFTER_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        // default persistence service
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()));
        assertNull(historicItem);
    public void testPersistedStateQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.persistedState(quantityItem,
        assertThat(historicItem.getState(), is(instanceOf(QuantityType.class)));
        assertEquals(new QuantityType<>(value(HISTORIC_END), SIUnits.CELSIUS), historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(quantityItem,
        assertEquals(new QuantityType<>(value(HISTORIC_INTERMEDIATE_VALUE_1), SIUnits.CELSIUS),
                historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(quantityItem, ZonedDateTime.now(), SERVICE_ID);
        assertEquals(new QuantityType<>(value(FUTURE_START), SIUnits.CELSIUS), historicItem.getState());
        assertEquals(new QuantityType<>(value(FUTURE_INTERMEDIATE_VALUE_3), SIUnits.CELSIUS), historicItem.getState());
        assertEquals(new QuantityType<>(value(FUTURE_END), SIUnits.CELSIUS), historicItem.getState());
    public void testPersistedStateGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.persistedState(groupQuantityItem,
        historicItem = PersistenceExtensions.persistedState(groupQuantityItem,
        historicItem = PersistenceExtensions.persistedState(groupQuantityItem, ZonedDateTime.now(), SERVICE_ID);
    public void testPersistedStateOnOffType() {
        ZonedDateTime now = ZonedDateTime.now().truncatedTo(ChronoUnit.HOURS).minusMinutes(1);
        HistoricItem historicItem = PersistenceExtensions.persistedState(switchItem, now.plusHours(SWITCH_START),
        historicItem = PersistenceExtensions.persistedState(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_1),
        assertEquals(switchValue(SWITCH_ON_INTERMEDIATE_1), historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(switchItem, now.plusHours(SWITCH_OFF_INTERMEDIATE_1),
        assertEquals(switchValue(SWITCH_OFF_INTERMEDIATE_1), historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(switchItem, now.plusHours(SWITCH_OFF_INTERMEDIATE_2),
        assertEquals(switchValue(SWITCH_OFF_INTERMEDIATE_2), historicItem.getState());
        historicItem = PersistenceExtensions.persistedState(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_3),
        assertEquals(switchValue(SWITCH_ON_INTERMEDIATE_3), historicItem.getState());
    public void testMaximumSinceDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.maximumSince(numberItem,
        historicItem = PersistenceExtensions.maximumSince(numberItem,
        assertEquals(ZonedDateTime.of(HISTORIC_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                historicItem.getTimestamp());
    public void testMaximumUntilDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.maximumUntil(numberItem,
        historicItem = PersistenceExtensions.maximumUntil(numberItem,
        assertEquals(ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()));
    public void testMaximumBetweenDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.maximumBetween(numberItem,
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_2, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertThat(historicItem.getState(), is(value(HISTORIC_INTERMEDIATE_VALUE_2)));
        historicItem = PersistenceExtensions.maximumBetween(numberItem,
                ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_4, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertThat(historicItem.getState(), is(value(FUTURE_INTERMEDIATE_VALUE_4)));
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_2, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()));
    public void testMaximumSinceQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumSince(quantityItem,
        assertThat(historicItem.getState(), is(new QuantityType<>(value(HISTORIC_END), SIUnits.CELSIUS)));
        historicItem = PersistenceExtensions.maximumSince(quantityItem,
        assertThat(historicItem.getTimestamp(),
                is(ZonedDateTime.of(HISTORIC_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault())));
        // test with alternative unit
        quantityItem.setState(QuantityType.valueOf(5000, Units.KELVIN));
        assertThat(historicItem.getState(), is(new QuantityType<>(4726.85, SIUnits.CELSIUS)));
    public void testMaximumUntilQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumUntil(quantityItem,
        historicItem = PersistenceExtensions.maximumUntil(quantityItem,
    public void testMaximumBetweenQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumBetween(quantityItem,
        assertThat(historicItem.getState(),
                is(new QuantityType<>(value(HISTORIC_INTERMEDIATE_VALUE_2), SIUnits.CELSIUS)));
        historicItem = PersistenceExtensions.maximumBetween(quantityItem,
                is(new QuantityType<>(value(FUTURE_INTERMEDIATE_VALUE_4), SIUnits.CELSIUS)));
    public void testMaximumSinceGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumSince(groupQuantityItem,
        historicItem = PersistenceExtensions.maximumSince(groupQuantityItem,
        groupQuantityItem.setState(QuantityType.valueOf(5000, Units.KELVIN));
    public void testMaximumUntilGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumUntil(groupQuantityItem,
        historicItem = PersistenceExtensions.maximumUntil(groupQuantityItem,
    public void testMaximumBetweenGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.maximumBetween(groupQuantityItem,
        historicItem = PersistenceExtensions.maximumBetween(groupQuantityItem,
    public void testMaximumSinceOnOffType() {
        HistoricItem historicItem = PersistenceExtensions.maximumSince(switchItem, now.plusHours(SWITCH_START),
        assertEquals(switchValue(SWITCH_ON_1), historicItem.getState());
        historicItem = PersistenceExtensions.maximumSince(switchItem, now.plusHours(SWITCH_OFF_INTERMEDIATE_1),
        assertEquals(switchValue(SWITCH_ON_2), historicItem.getState());
        historicItem = PersistenceExtensions.maximumSince(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_21),
        historicItem = PersistenceExtensions.maximumSince(switchItem, now, SERVICE_ID);
        historicItem = PersistenceExtensions.maximumSince(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_22),
    public void testMaximumUntilOnOffType() {
        HistoricItem historicItem = PersistenceExtensions.maximumUntil(switchItem,
                now.plusHours(SWITCH_OFF_INTERMEDIATE_2), SERVICE_ID);
        historicItem = PersistenceExtensions.maximumUntil(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_3),
        assertEquals(switchValue(SWITCH_ON_3), historicItem.getState());
        historicItem = PersistenceExtensions.maximumUntil(switchItem, now.plusHours(SWITCH_END), SERVICE_ID);
        historicItem = PersistenceExtensions.maximumUntil(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_21),
    public void testMinimumSinceDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.minimumSince(numberItem,
                ZonedDateTime.of(BEFORE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertEquals(value(HISTORIC_START), historicItem.getState());
        historicItem = PersistenceExtensions.minimumSince(numberItem,
        assertEquals(ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
    public void testMinimumUntilDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.minimumUntil(numberItem,
        historicItem = PersistenceExtensions.minimumUntil(numberItem,
    public void testMinimumBetweenDecimalType() {
        HistoricItem historicItem = PersistenceExtensions.minimumBetween(numberItem,
        assertThat(historicItem.getState(), is(value(HISTORIC_INTERMEDIATE_VALUE_1)));
        historicItem = PersistenceExtensions.minimumBetween(numberItem,
        assertThat(historicItem.getState(), is(value(FUTURE_INTERMEDIATE_VALUE_3)));
    public void testMinimumSinceQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumSince(quantityItem,
        assertEquals(new QuantityType<>(value(HISTORIC_START), SIUnits.CELSIUS), historicItem.getState());
        historicItem = PersistenceExtensions.minimumSince(quantityItem,
        quantityItem.setState(QuantityType.valueOf(273.15, Units.KELVIN));
        assertThat(historicItem.getState(), is(new QuantityType<>(0, SIUnits.CELSIUS)));
    public void testMinimumUntilQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumUntil(quantityItem,
        historicItem = PersistenceExtensions.minimumUntil(quantityItem,
    public void testMinimumBetweenQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumBetween(quantityItem,
                is(new QuantityType<>(value(HISTORIC_INTERMEDIATE_VALUE_1), SIUnits.CELSIUS)));
        historicItem = PersistenceExtensions.minimumBetween(quantityItem,
                is(new QuantityType<>(value(FUTURE_INTERMEDIATE_VALUE_3), SIUnits.CELSIUS)));
    public void testMinimumSinceGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumSince(groupQuantityItem,
        historicItem = PersistenceExtensions.minimumSince(groupQuantityItem,
        groupQuantityItem.setState(QuantityType.valueOf(273.15, Units.KELVIN));
    public void testMinimumUntilGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumUntil(groupQuantityItem,
        historicItem = PersistenceExtensions.minimumUntil(groupQuantityItem,
    public void testMinimumBetweenGroupQuantityType() {
        HistoricItem historicItem = PersistenceExtensions.minimumBetween(groupQuantityItem,
        historicItem = PersistenceExtensions.minimumBetween(groupQuantityItem,
    public void testMinimumSinceOnOffType() {
        HistoricItem historicItem = PersistenceExtensions.minimumSince(switchItem, now.plusHours(SWITCH_START),
        assertEquals(switchValue(SWITCH_OFF_1), historicItem.getState());
        historicItem = PersistenceExtensions.minimumSince(switchItem, now.plusHours(SWITCH_OFF_INTERMEDIATE_1),
        historicItem = PersistenceExtensions.minimumSince(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_21),
        assertEquals(switchValue(SWITCH_ON_INTERMEDIATE_21), historicItem.getState());
        historicItem = PersistenceExtensions.minimumSince(switchItem, now, SERVICE_ID);
        assertEquals(switchValue(SWITCH_ON_INTERMEDIATE_22), historicItem.getState());
        historicItem = PersistenceExtensions.minimumSince(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_22),
    public void testVarianceSinceDecimalType() {
        ZonedDateTime startStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0,
                ZoneId.systemDefault());
        double expectedAverage = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, null);
        double expected = DoubleStream
                .concat(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_END)
                        .mapToDouble(i -> Double.valueOf(i)), DoubleStream.of(STATE.doubleValue()))
                .map(d -> Math.pow(d - expectedAverage, 2)).sum()
                / (HISTORIC_END + 1 - HISTORIC_INTERMEDIATE_VALUE_1 + 1);
        State variance = PersistenceExtensions.varianceSince(numberItem, startStored, SERVICE_ID);
        assertNotNull(variance);
        assertNotNull(dt);
        assertEquals(expected, dt.doubleValue(), 0.01);
        variance = PersistenceExtensions.varianceSince(numberItem, startStored);
        assertNull(variance);
    public void testVarianceUntilDecimalType() {
        ZonedDateTime endStored = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0,
        double expectedAverage = testAverage(null, FUTURE_INTERMEDIATE_VALUE_3);
                .concat(DoubleStream.of(STATE.doubleValue()),
                        IntStream.rangeClosed(FUTURE_START, FUTURE_INTERMEDIATE_VALUE_3)
                                .mapToDouble(i -> Double.valueOf(i)))
                / (1 + FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1);
        State variance = PersistenceExtensions.varianceUntil(numberItem, endStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceUntil(numberItem, endStored);
    public void testVarianceBetweenDecimalType() {
        ZonedDateTime endStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_2, 1, 1, 0, 0, 0, 0,
        double expectedAverage1 = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2);
        double expected = IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2)
                .mapToDouble(i -> Double.valueOf(i)).map(d -> Math.pow(d - expectedAverage1, 2)).sum()
                / (HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1 + 1);
        State variance = PersistenceExtensions.varianceBetween(numberItem, startStored, endStored, SERVICE_ID);
        assertThat(dt.doubleValue(), is(closeTo(expected, 0.01)));
        startStored = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        endStored = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_4, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        double expectedAverage2 = testAverage(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4);
        expected = IntStream.rangeClosed(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4)
                .mapToDouble(i -> Double.valueOf(i)).map(d -> Math.pow(d - expectedAverage2, 2)).sum()
                / (FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3 + 1);
        variance = PersistenceExtensions.varianceBetween(numberItem, startStored, endStored, SERVICE_ID);
        dt = variance.as(DecimalType.class);
        startStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        endStored = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        double expectedAverage3 = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, FUTURE_INTERMEDIATE_VALUE_3);
        expected = IntStream
                .concat(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_END),
                        IntStream.rangeClosed(FUTURE_START, FUTURE_INTERMEDIATE_VALUE_3))
                .mapToDouble(i -> Double.valueOf(i)).map(d -> Math.pow(d - expectedAverage3, 2)).sum()
                / (FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1 + HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + 1);
        variance = PersistenceExtensions.varianceBetween(numberItem, startStored, endStored);
    public void testVarianceSinceQuantityType() {
        State variance = PersistenceExtensions.varianceSince(quantityItem, startStored, SERVICE_ID);
        QuantityType<?> qt = variance.as(QuantityType.class);
        assertNotNull(qt);
        assertEquals(expected, qt.doubleValue(), 0.01);
        assertEquals(Units.KELVIN.multiply(Units.KELVIN), qt.getUnit());
        variance = PersistenceExtensions.varianceSince(quantityItem, startStored);
    public void testVarianceUntilQuantityType() {
        State variance = PersistenceExtensions.varianceUntil(quantityItem, endStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceUntil(quantityItem, endStored);
    public void testVarianceBetweenQuantityType() {
        State variance = PersistenceExtensions.varianceBetween(quantityItem, startStored, endStored, SERVICE_ID);
        assertThat(qt.doubleValue(), is(closeTo(expected, 0.01)));
        variance = PersistenceExtensions.varianceBetween(quantityItem, startStored, endStored, SERVICE_ID);
        qt = variance.as(QuantityType.class);
        variance = PersistenceExtensions.varianceBetween(quantityItem, startStored, endStored);
    public void testVarianceSinceGroupQuantityType() {
        State variance = PersistenceExtensions.varianceSince(groupQuantityItem, startStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceSince(groupQuantityItem, startStored);
    public void testVarianceUntilGroupQuantityType() {
        State variance = PersistenceExtensions.varianceUntil(groupQuantityItem, endStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceUntil(groupQuantityItem, endStored);
    public void testVarianceBetweenGroupQuantityType() {
        State variance = PersistenceExtensions.varianceBetween(groupQuantityItem, startStored, endStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceBetween(groupQuantityItem, startStored, endStored, SERVICE_ID);
        variance = PersistenceExtensions.varianceBetween(groupQuantityItem, startStored, endStored);
    public void testDeviationSinceDecimalType() {
        double expected = Math.sqrt(DoubleStream
                / (HISTORIC_END + 1 - HISTORIC_INTERMEDIATE_VALUE_1 + 1));
        State deviation = PersistenceExtensions.deviationSince(numberItem, startStored, SERVICE_ID);
        assertNotNull(deviation);
        DecimalType dt = deviation.as(DecimalType.class);
        deviation = PersistenceExtensions.deviationSince(numberItem, startStored);
        assertNull(deviation);
    public void testDeviationUntilDecimalType() {
                / (1 + FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1));
        State deviation = PersistenceExtensions.deviationUntil(numberItem, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationUntil(numberItem, endStored);
    public void testDeviationBetweenDecimalType() {
        double expectedAverage = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2);
        double expected = Math.sqrt(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2)
                .mapToDouble(i -> Double.parseDouble(Integer.toString(i))).map(d -> Math.pow(d - expectedAverage, 2))
                .sum() / (HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1 + 1));
        State deviation = PersistenceExtensions.deviationBetween(numberItem, startStored, endStored, SERVICE_ID);
        expected = Math.sqrt(IntStream.rangeClosed(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4)
                / (FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3 + 1));
        deviation = PersistenceExtensions.deviationBetween(numberItem, startStored, endStored, SERVICE_ID);
        dt = deviation.as(DecimalType.class);
        expected = Math.sqrt(IntStream
                / (FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1 + HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + 1));
        deviation = PersistenceExtensions.deviationBetween(numberItem, startStored, endStored);
    public void testDeviationSinceQuantityType() {
        State deviation = PersistenceExtensions.deviationSince(quantityItem, startStored, SERVICE_ID);
        QuantityType<?> qt = deviation.as(QuantityType.class);
        assertEquals(Units.KELVIN, qt.getUnit());
        deviation = PersistenceExtensions.deviationSince(quantityItem, startStored);
    public void testDeviationUntilQuantityType() {
        State deviation = PersistenceExtensions.deviationUntil(quantityItem, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationUntil(quantityItem, endStored);
    public void testDeviationBetweenQuantityType() {
        State deviation = PersistenceExtensions.deviationBetween(quantityItem, startStored, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationBetween(quantityItem, startStored, endStored, SERVICE_ID);
        qt = deviation.as(QuantityType.class);
        deviation = PersistenceExtensions.deviationBetween(quantityItem, startStored, endStored);
    public void testDeviationSinceGroupQuantityType() {
        State deviation = PersistenceExtensions.deviationSince(groupQuantityItem, startStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationSince(groupQuantityItem, startStored);
    public void testDeviationUntilGroupQuantityType() {
        State deviation = PersistenceExtensions.deviationUntil(groupQuantityItem, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationUntil(groupQuantityItem, endStored);
    public void testDeviationBetweenGroupQuantityType() {
        State deviation = PersistenceExtensions.deviationBetween(groupQuantityItem, startStored, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationBetween(groupQuantityItem, startStored, endStored, SERVICE_ID);
        deviation = PersistenceExtensions.deviationBetween(groupQuantityItem, startStored, endStored);
    public void testRiemannSumBetweenDecimalType() {
        RiemannType type = RiemannType.LEFT;
        ZonedDateTime beginStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0,
        double expected = testRiemannSum(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2, type);
        State sum = PersistenceExtensions.riemannSumBetween(numberItem, beginStored, endStored, type, SERVICE_ID);
        assertNotNull(sum);
        DecimalType dt = sum.as(DecimalType.class);
        beginStored = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        expected = testRiemannSum(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4, type);
        sum = PersistenceExtensions.riemannSumBetween(numberItem, beginStored, endStored, type, SERVICE_ID);
        dt = sum.as(DecimalType.class);
        beginStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        expected = testRiemannSum(HISTORIC_INTERMEDIATE_VALUE_1, FUTURE_INTERMEDIATE_VALUE_3, type);
        sum = PersistenceExtensions.riemannSumBetween(numberItem, beginStored, endStored, type);
        assertNull(sum);
        type = RiemannType.RIGHT;
        endStored = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_2, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        expected = testRiemannSum(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2, type);
        type = RiemannType.TRAPEZOIDAL;
        type = RiemannType.MIDPOINT;
    public void testRiemannSumBetweenQuantityType() {
        for (RiemannType type : RiemannType.values()) {
            double expected = testRiemannSumCelsius(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2, type);
            State sum = PersistenceExtensions.riemannSumBetween(quantityItem, beginStored, endStored, type, SERVICE_ID);
            QuantityType<?> qt = sum.as(QuantityType.class);
            assertEquals(Units.KELVIN.multiply(Units.SECOND), qt.getUnit());
            expected = testRiemannSumCelsius(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4, type);
            sum = PersistenceExtensions.riemannSumBetween(quantityItem, beginStored, endStored, type, SERVICE_ID);
            qt = sum.as(QuantityType.class);
            expected = testRiemannSumCelsius(HISTORIC_INTERMEDIATE_VALUE_1, FUTURE_INTERMEDIATE_VALUE_3, type);
            sum = PersistenceExtensions.riemannSumBetween(quantityItem, beginStored, endStored, type);
    public void testRiemannSumBetweenDecimalTypeIrregularTimespans() {
        int historicHours = 27;
        int futureHours = 27;
        // Persistence will contain following entries:
        // 0 - 27 hours back in time
        // 100 - 26 hours back in time
        // 0 - 25 hours back in time
        // 50 - 2 hours back in time
        // 0 - 1 hour back in time
        // 0 - 1 hour forward in time
        // 50 - 2 hours forward in time
        // 0 - 3 hours forward in time
        // 100 - 25 hours forward in time
        // 0 - 26 hour forward in time
        createTestCachedValuesPersistenceService(now, historicHours, futureHours);
        // Testing that riemannSum calculates the correct Riemann sum for the last half hour without persisted value in
        // that horizon
        State sum = PersistenceExtensions.riemannSumSince(numberItem, now.minusMinutes(30), type,
                TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo(0.0, 0.01)));
        // Testing that riemannSum calculates the correct Riemann sum from the last 27 hours to the next 27 hours
        sum = PersistenceExtensions.riemannSumBetween(numberItem, now.minusHours(historicHours),
                now.plusHours(futureHours), type, TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo(100.0 * 3600 + 50.0 * 3600 + 100.0 * 3600 + 50.0 * 3600, 0.01)));
        // Testing that riemannSum calculates the correct Riemann sum from the last 24 hours to the next 24 hours
        sum = PersistenceExtensions.riemannSumBetween(numberItem, now.minusHours(historicHours).plusHours(3),
                now.plusHours(futureHours).minusHours(3), type, TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo(50.0 * 3600 + 50.0 * 3600, 0.01)));
        // Testing that riemannSum calculates the correct Riemann sum from the last 30 minutes to the next 30 minutes
        sum = PersistenceExtensions.riemannSumBetween(numberItem, now.minusMinutes(30), now.plusMinutes(30), type,
        assertThat(dt.doubleValue(), is(closeTo(0, 0.01)));
    public void testRiemannSumBetweenNoPersistedValues() {
        int futureHours = 0;
        // Testing that average calculates the correct average for the half hour without persisted values in
        State sum = PersistenceExtensions.riemannSumBetween(numberItem, now.minusMinutes(105), now.minusMinutes(75),
                type, TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo(50.0 * 1800, 0.01)));
    public void testRiemannSumBetweenZeroDuration() {
        State sum = PersistenceExtensions.riemannSumBetween(numberItem, now, now, SERVICE_ID);
    public void testAverageSinceDecimalType() {
        ZonedDateTime start = ZonedDateTime.of(BEFORE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        double expected = testAverage(BEFORE_START, null);
        State average = PersistenceExtensions.averageSince(numberItem, start, SERVICE_ID);
        assertNotNull(average);
        DecimalType dt = average.as(DecimalType.class);
        start = ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        expected = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, null);
        average = PersistenceExtensions.averageSince(numberItem, start, SERVICE_ID);
        dt = average.as(DecimalType.class);
        average = PersistenceExtensions.averageSince(numberItem, start);
        assertNull(average);
    public void testAverageUntilDecimalType() {
        ZonedDateTime end = ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault());
        double expected = testAverage(null, FUTURE_INTERMEDIATE_VALUE_3);
        State average = PersistenceExtensions.averageUntil(numberItem, end, SERVICE_ID);
        average = PersistenceExtensions.averageUntil(numberItem, end);
    public void testAverageBetweenDecimalType() {
        double expected = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2);
        State average = PersistenceExtensions.averageBetween(numberItem, beginStored, endStored, SERVICE_ID);
        expected = testAverage(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4);
        average = PersistenceExtensions.averageBetween(numberItem, beginStored, endStored, SERVICE_ID);
        expected = testAverage(HISTORIC_INTERMEDIATE_VALUE_1, FUTURE_INTERMEDIATE_VALUE_3);
        average = PersistenceExtensions.averageBetween(numberItem, beginStored, endStored);
    public void testAverageSinceQuantityType() {
        State average = PersistenceExtensions.averageSince(quantityItem, start, SERVICE_ID);
        QuantityType<?> qt = average.as(QuantityType.class);
        assertEquals(SIUnits.CELSIUS, qt.getUnit());
        average = PersistenceExtensions.averageSince(quantityItem, start, SERVICE_ID);
        qt = average.as(QuantityType.class);
        average = PersistenceExtensions.averageSince(quantityItem, start);
    public void testAverageUntilQuantityType() {
        State average = PersistenceExtensions.averageUntil(quantityItem, end, SERVICE_ID);
        average = PersistenceExtensions.averageUntil(quantityItem, end);
    public void testAverageBetweenQuantityType() {
        State average = PersistenceExtensions.averageBetween(quantityItem, beginStored, endStored, SERVICE_ID);
        average = PersistenceExtensions.averageBetween(quantityItem, beginStored, endStored, SERVICE_ID);
        average = PersistenceExtensions.averageBetween(quantityItem, beginStored, endStored);
    public void testAverageSinceGroupQuantityType() {
        State average = PersistenceExtensions.averageSince(groupQuantityItem, start, SERVICE_ID);
        average = PersistenceExtensions.averageSince(groupQuantityItem, start, SERVICE_ID);
        average = PersistenceExtensions.averageSince(groupQuantityItem, start);
    public void testAverageUntilGroupQuantityType() {
        State average = PersistenceExtensions.averageUntil(groupQuantityItem, end, SERVICE_ID);
        average = PersistenceExtensions.averageUntil(groupQuantityItem, end);
    public void testAverageBetweenGroupQuantityType() {
        State average = PersistenceExtensions.averageBetween(groupQuantityItem, beginStored, endStored, SERVICE_ID);
        average = PersistenceExtensions.averageBetween(groupQuantityItem, beginStored, endStored, SERVICE_ID);
        average = PersistenceExtensions.averageBetween(groupQuantityItem, beginStored, endStored);
    public void testAverageOnOffType() {
        // switch is 5h ON, 5h OFF, and 5h ON (until now)
        // switch is 5h ON, 5h OFF, and 5h ON (from now)
        ZonedDateTime now = ZonedDateTime.now().truncatedTo(ChronoUnit.MINUTES);
        State average = PersistenceExtensions.averageBetween(switchItem, now.plusHours(SWITCH_START),
                now.plusHours(SWITCH_END), SERVICE_ID);
        assertThat(dt.doubleValue(),
                is(closeTo((SWITCH_OFF_1 - SWITCH_ON_1 - SWITCH_ON_2 + SWITCH_OFF_3 - SWITCH_ON_3 + SWITCH_OFF_2)
                        / (1.0 * (-SWITCH_START + SWITCH_END)), 0.01)));
        average = PersistenceExtensions.averageBetween(switchItem, now.plusHours(SWITCH_OFF_INTERMEDIATE_1),
        assertThat(dt.doubleValue(), is(closeTo(
                (-SWITCH_ON_2 + SWITCH_OFF_2) / (1.0 * (-SWITCH_OFF_INTERMEDIATE_1 + SWITCH_OFF_INTERMEDIATE_2)),
                0.01)));
        average = PersistenceExtensions.averageBetween(switchItem, now.plusHours(SWITCH_ON_2),
                now.plusHours(SWITCH_ON_3), SERVICE_ID);
                is(closeTo((-SWITCH_ON_2 + SWITCH_OFF_2) / (1.0 * (-SWITCH_ON_2 + SWITCH_ON_3)), 0.01)));
        average = PersistenceExtensions.averageBetween(switchItem, now.plusHours(SWITCH_ON_INTERMEDIATE_21),
                now.plusHours(SWITCH_ON_INTERMEDIATE_22), SERVICE_ID);
        assertThat(dt.doubleValue(), is(closeTo((-SWITCH_ON_INTERMEDIATE_21 + SWITCH_ON_INTERMEDIATE_22)
                / (1.0 * (-SWITCH_ON_INTERMEDIATE_21 + SWITCH_ON_INTERMEDIATE_22)), 0.01)));
        average = PersistenceExtensions.averageSince(switchItem, now.plusHours(1), SERVICE_ID);
        average = PersistenceExtensions.averageUntil(switchItem, now.minusHours(1), SERVICE_ID);
    public void testAverageBetweenDecimalTypeIrregularTimespans() {
        // Testing that average calculates the correct average for the last half hour without persisted value in
        State average = PersistenceExtensions.averageSince(numberItem, now.minusMinutes(30),
        // Testing that average calculates the correct average from the last 27 hours to the next 27 hours
        average = PersistenceExtensions.averageBetween(numberItem, now.minusHours(historicHours),
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo((100.0 + 50.0 + 100.0 + 50.0) / (historicHours + futureHours), 0.01)));
        // Testing that average calculates the correct average from the last 24 hours to the next 24 hours
        average = PersistenceExtensions.averageBetween(numberItem, now.minusHours(historicHours).plusHours(3),
                now.plusHours(futureHours).minusHours(3), TestCachedValuesPersistenceService.ID);
        assertThat(dt.doubleValue(), is(closeTo((50.0 + 50.0) / (historicHours - 3.0 + futureHours - 3.0), 0.01)));
        // Testing that average calculates the correct average from the last 30 minutes to the next 30 minutes
        average = PersistenceExtensions.averageBetween(numberItem, now.minusMinutes(30), now.plusMinutes(30),
    public void testAverageBetweenNoPersistedValues() {
        State average = PersistenceExtensions.averageBetween(numberItem, now.minusMinutes(105), now.minusMinutes(75),
        assertThat(dt.doubleValue(), is(closeTo(50.0, 0.01)));
    public void testAverageBetweenZeroDuration() {
        State state = PersistenceExtensions.averageBetween(quantityItem, now, now, SERVICE_ID);
        assertNotNull(state);
        QuantityType<?> qt = state.as(QuantityType.class);
        assertEquals(HISTORIC_END, qt.doubleValue(), 0.01);
    public void testMedianSinceDecimalType() {
        double expected = testMedian(BEFORE_START, null);
        State median = PersistenceExtensions.medianSince(numberItem, start, SERVICE_ID);
        assertNotNull(median);
        DecimalType dt = median.as(DecimalType.class);
        expected = testMedian(HISTORIC_INTERMEDIATE_VALUE_1, null);
        median = PersistenceExtensions.medianSince(numberItem, start, SERVICE_ID);
        dt = median.as(DecimalType.class);
        median = PersistenceExtensions.medianSince(numberItem, start);
        assertNull(median);
    public void testMedianUntilDecimalType() {
        double expected = testMedian(null, FUTURE_INTERMEDIATE_VALUE_3);
        State median = PersistenceExtensions.medianUntil(numberItem, end, SERVICE_ID);
        median = PersistenceExtensions.medianUntil(numberItem, end);
    public void testMedianBetweenDecimalType() {
        double expected = testMedian(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2);
        State median = PersistenceExtensions.medianBetween(numberItem, beginStored, endStored, SERVICE_ID);
        expected = testMedian(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4);
        median = PersistenceExtensions.medianBetween(numberItem, beginStored, endStored, SERVICE_ID);
        expected = testMedian(HISTORIC_INTERMEDIATE_VALUE_1, FUTURE_INTERMEDIATE_VALUE_3);
        median = PersistenceExtensions.medianBetween(quantityItem, beginStored, endStored);
    public void testMedianSinceQuantityType() {
        State median = PersistenceExtensions.medianSince(quantityItem, start, SERVICE_ID);
        QuantityType<?> qt = median.as(QuantityType.class);
        median = PersistenceExtensions.medianSince(quantityItem, start, SERVICE_ID);
        qt = median.as(QuantityType.class);
        median = PersistenceExtensions.medianSince(quantityItem, start);
    public void testMedianUntilQuantityType() {
        State median = PersistenceExtensions.medianUntil(quantityItem, end, SERVICE_ID);
        median = PersistenceExtensions.medianUntil(quantityItem, end);
    public void testMedianBetweenQuantityType() {
        State median = PersistenceExtensions.medianBetween(quantityItem, beginStored, endStored, SERVICE_ID);
        median = PersistenceExtensions.medianBetween(quantityItem, beginStored, endStored, SERVICE_ID);
    public void testMedianSinceGroupQuantityType() {
        State median = PersistenceExtensions.medianSince(groupQuantityItem, start, SERVICE_ID);
        median = PersistenceExtensions.medianSince(groupQuantityItem, start, SERVICE_ID);
        median = PersistenceExtensions.medianSince(groupQuantityItem, start);
    public void testMedianUntilGroupQuantityType() {
        State median = PersistenceExtensions.medianUntil(groupQuantityItem, end, SERVICE_ID);
        median = PersistenceExtensions.medianUntil(groupQuantityItem, end);
    public void testMedianBetweenGroupQuantityType() {
        State median = PersistenceExtensions.medianBetween(groupQuantityItem, beginStored, endStored, SERVICE_ID);
        median = PersistenceExtensions.medianBetween(groupQuantityItem, beginStored, endStored, SERVICE_ID);
        median = PersistenceExtensions.medianBetween(groupQuantityItem, beginStored, endStored);
    public void testMedianBetweenZeroDuration() {
        State state = PersistenceExtensions.medianBetween(quantityItem, now, now, SERVICE_ID);
    public void testSumSinceDecimalType() {
        State sum = PersistenceExtensions.sumSince(numberItem,
        assertEquals(IntStream.rangeClosed(HISTORIC_START, HISTORIC_END).sum(), dt.doubleValue(), 0.001);
        sum = PersistenceExtensions.sumSince(numberItem,
        assertEquals(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_END).sum(), dt.doubleValue(), 0.001);
    public void testSumUntilDecimalType() {
        State sum = PersistenceExtensions.sumUntil(numberItem,
        assertEquals(IntStream.rangeClosed(FUTURE_START, FUTURE_INTERMEDIATE_VALUE_3).sum(), dt.doubleValue(), 0.001);
        sum = PersistenceExtensions.sumUntil(numberItem,
    public void testSumBetweenDecimalType() {
        State sum = PersistenceExtensions.sumBetween(numberItem,
        assertEquals(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2).sum(),
                dt.doubleValue(), 0.001);
        sum = PersistenceExtensions.sumBetween(numberItem,
        assertEquals(IntStream.rangeClosed(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4).sum(),
                IntStream.concat(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_END),
                        IntStream.rangeClosed(FUTURE_START, FUTURE_INTERMEDIATE_VALUE_3)).sum(),
    public void testSumSinceQuantityType() {
        State sum = PersistenceExtensions.sumSince(quantityItem,
        assertEquals(IntStream.rangeClosed(HISTORIC_START, HISTORIC_END).sum()
                + (HISTORIC_END - HISTORIC_START + 1) * KELVIN_OFFSET, qt.doubleValue(), 0.001);
        sum = PersistenceExtensions.sumSince(quantityItem,
        assertEquals(IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_END).sum()
                + (HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + 1) * KELVIN_OFFSET, qt.doubleValue(), 0.001);
    public void testSumUntilQuantityType() {
        State sum = PersistenceExtensions.sumUntil(quantityItem,
        assertEquals(IntStream.rangeClosed(FUTURE_START, FUTURE_INTERMEDIATE_VALUE_3).sum()
                + (FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1) * KELVIN_OFFSET, qt.doubleValue(), 0.001);
    public void testSumBetweenQuantityType() {
        State sum = PersistenceExtensions.sumBetween(groupQuantityItem,
                IntStream.rangeClosed(HISTORIC_INTERMEDIATE_VALUE_1, HISTORIC_INTERMEDIATE_VALUE_2).sum()
                        + (HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1 + 1) * KELVIN_OFFSET,
                qt.doubleValue(), 0.001);
        sum = PersistenceExtensions.sumBetween(groupQuantityItem,
                IntStream.rangeClosed(FUTURE_INTERMEDIATE_VALUE_3, FUTURE_INTERMEDIATE_VALUE_4).sum()
                        + (FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3 + 1) * KELVIN_OFFSET,
        assertEquals(IntStream
                .sum()
                + (HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 2)
                        * KELVIN_OFFSET,
    public void testSumSinceGroupQuantityType() {
        State sum = PersistenceExtensions.sumSince(groupQuantityItem,
        sum = PersistenceExtensions.sumSince(groupQuantityItem,
    public void testSumUntilGroupQuantityType() {
        State sum = PersistenceExtensions.sumUntil(groupQuantityItem,
    public void testSumBetweenGroupQuantityType() {
    public void testLastUpdate() {
        ZonedDateTime lastUpdate = PersistenceExtensions.lastUpdate(numberItem, SERVICE_ID);
        assertNotNull(lastUpdate);
        assertEquals(ZonedDateTime.of(HISTORIC_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), lastUpdate);
        lastUpdate = PersistenceExtensions.lastUpdate(numberItem);
        assertNull(lastUpdate);
        // last update on empty persistence
        int historicHours = 0;
        lastUpdate = PersistenceExtensions.lastUpdate(numberItem, TestCachedValuesPersistenceService.ID);
    public void testNextUpdate() {
        ZonedDateTime nextUpdate = PersistenceExtensions.nextUpdate(numberItem, SERVICE_ID);
        assertNotNull(nextUpdate);
        assertEquals(ZonedDateTime.of(FUTURE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), nextUpdate);
        nextUpdate = PersistenceExtensions.lastUpdate(numberItem);
        assertNull(nextUpdate);
    public void testLastChange() {
        ZonedDateTime lastChange = PersistenceExtensions.lastChange(numberItem, SERVICE_ID);
        assertNotNull(lastChange);
        assertEquals(ZonedDateTime.of(HISTORIC_END, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), lastChange);
        lastChange = PersistenceExtensions.lastChange(numberItem);
        assertNull(lastChange);
        // last change on empty persistence
        lastChange = PersistenceExtensions.lastChange(numberItem, TestCachedValuesPersistenceService.ID);
    public void testNextChange() {
        ZonedDateTime nextChange = PersistenceExtensions.nextChange(numberItem, SERVICE_ID);
        assertNotNull(nextChange);
        assertEquals(ZonedDateTime.of(FUTURE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), nextChange);
        nextChange = PersistenceExtensions.nextChange(numberItem);
        assertNull(nextChange);
    public void testDeltaSince() {
        State delta = PersistenceExtensions.deltaSince(numberItem,
        assertNull(delta);
        delta = PersistenceExtensions.deltaSince(numberItem,
        assertNotNull(delta);
        DecimalType dt = delta.as(DecimalType.class);
        DecimalType dtState = numberItem.getState().as(DecimalType.class);
        assertNotNull(dtState);
        assertEquals(dtState.doubleValue() - HISTORIC_INTERMEDIATE_VALUE_1, dt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaSince(quantityItem,
        QuantityType<?> qt = delta.as(QuantityType.class);
        QuantityType<?> qtState = quantityItem.getState().as(QuantityType.class);
        assertNotNull(qtState);
        assertEquals(qtState.doubleValue() - HISTORIC_INTERMEDIATE_VALUE_1, qt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaSince(groupQuantityItem,
        qt = delta.as(QuantityType.class);
        qtState = groupQuantityItem.getState().as(QuantityType.class);
    public void testDeltaUntil() {
        State delta = PersistenceExtensions.deltaUntil(numberItem,
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - dtState.doubleValue(), dt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaUntil(quantityItem,
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - dtState.doubleValue(), qt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaUntil(groupQuantityItem,
        delta = PersistenceExtensions.deltaUntil(numberItem,
    public void testDeltaBetween() {
        State delta = PersistenceExtensions.deltaBetween(numberItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1, dt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaBetween(quantityItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1, qt.doubleValue(), 0.001);
        delta = PersistenceExtensions.deltaBetween(groupQuantityItem,
        delta = PersistenceExtensions.deltaBetween(numberItem,
        dt = delta.as(DecimalType.class);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3, dt.doubleValue(), 0.001);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3, qt.doubleValue(), 0.001);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - HISTORIC_INTERMEDIATE_VALUE_1, dt.doubleValue(), 0.001);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - HISTORIC_INTERMEDIATE_VALUE_1, qt.doubleValue(), 0.001);
    public void testEvolutionRateSince() {
        DecimalType rate = PersistenceExtensions.evolutionRateSince(numberItem,
        assertThat(rate, is(nullValue()));
        rate = PersistenceExtensions.evolutionRateSince(numberItem,
        assertNotNull(rate);
        // ((now - then) / then) * 100
        assertThat(rate.doubleValue(),
                is(closeTo(
                        100.0 * (STATE.doubleValue() - HISTORIC_INTERMEDIATE_VALUE_1) / HISTORIC_INTERMEDIATE_VALUE_1,
                        0.001)));
        rate = PersistenceExtensions.evolutionRateSince(quantityItem,
        assertNull(rate);
    public void testEvolutionRateUntil() {
        DecimalType rate = PersistenceExtensions.evolutionRateUntil(numberItem,
        // ((then - now) / now) * 100
                is(closeTo(100.0 * (FUTURE_INTERMEDIATE_VALUE_3 - STATE.doubleValue()) / STATE.doubleValue(), 0.001)));
        rate = PersistenceExtensions.evolutionRateUntil(quantityItem,
        rate = PersistenceExtensions.evolutionRateUntil(numberItem,
    public void testEvolutionRateBetween() {
        DecimalType rate = PersistenceExtensions.evolutionRateBetween(numberItem,
        assertThat(rate.doubleValue(), is(closeTo(
                100.0 * (HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1) / HISTORIC_INTERMEDIATE_VALUE_1,
        rate = PersistenceExtensions.evolutionRateBetween(quantityItem,
        rate = PersistenceExtensions.evolutionRateBetween(numberItem,
                100.0 * (FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3) / FUTURE_INTERMEDIATE_VALUE_3,
                100.0 * (FUTURE_INTERMEDIATE_VALUE_3 - HISTORIC_INTERMEDIATE_VALUE_1) / HISTORIC_INTERMEDIATE_VALUE_1,
    public void testPreviousStateDecimalTypeNoSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(numberItem, false, SERVICE_ID);
        assertNotNull(prevStateItem);
        assertThat(prevStateItem.getState(), is(instanceOf(DecimalType.class)));
        assertEquals(value(HISTORIC_END), prevStateItem.getState());
        numberItem.setState(new DecimalType(4321));
        prevStateItem = PersistenceExtensions.previousState(numberItem, false, SERVICE_ID);
        numberItem.setState(new DecimalType(HISTORIC_END));
        numberItem.setState(new DecimalType(3025));
        prevStateItem = PersistenceExtensions.previousState(numberItem, false);
        assertNull(prevStateItem);
    public void testPreviousStateQuantityTypeNoSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(quantityItem, false, SERVICE_ID);
        assertThat(prevStateItem.getState(), is(instanceOf(QuantityType.class)));
        assertEquals(new QuantityType<>(value(HISTORIC_END), SIUnits.CELSIUS), prevStateItem.getState());
        quantityItem.setState(QuantityType.valueOf(4321, SIUnits.CELSIUS));
        prevStateItem = PersistenceExtensions.previousState(quantityItem, false, SERVICE_ID);
        quantityItem.setState(QuantityType.valueOf(HISTORIC_END, SIUnits.CELSIUS));
        quantityItem.setState(QuantityType.valueOf(3025, SIUnits.CELSIUS));
        prevStateItem = PersistenceExtensions.previousState(quantityItem, false);
    public void testPreviousStateGroupQuantityTypeNoSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(groupQuantityItem, false, SERVICE_ID);
        groupQuantityItem.setState(QuantityType.valueOf(4321, SIUnits.CELSIUS));
        prevStateItem = PersistenceExtensions.previousState(groupQuantityItem, false, SERVICE_ID);
        groupQuantityItem.setState(QuantityType.valueOf(HISTORIC_END, SIUnits.CELSIUS));
        groupQuantityItem.setState(QuantityType.valueOf(3025, SIUnits.CELSIUS));
        prevStateItem = PersistenceExtensions.previousState(groupQuantityItem, false);
    public void testPreviousStateDecimalTypeSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(numberItem, true, SERVICE_ID);
        assertEquals(value(HISTORIC_END - 1), prevStateItem.getState());
        prevStateItem = PersistenceExtensions.previousState(numberItem, true);
    public void testPreviousStateQuantityTypeSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(quantityItem, true, SERVICE_ID);
        assertEquals(new QuantityType<>(value(HISTORIC_END - 1), SIUnits.CELSIUS), prevStateItem.getState());
        prevStateItem = PersistenceExtensions.previousState(quantityItem, true);
    public void testPreviousStateGroupQuantityTypeSkip() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(groupQuantityItem, true, SERVICE_ID);
        prevStateItem = PersistenceExtensions.previousState(groupQuantityItem, true);
    public void testPreviousStateEmptyPersistence() {
        HistoricItem prevStateItem = PersistenceExtensions.previousState(numberItem, false,
        prevStateItem = PersistenceExtensions.previousState(numberItem, true, TestCachedValuesPersistenceService.ID);
    public void testNextStateDecimalTypeNoSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(numberItem, false, SERVICE_ID);
        assertNotNull(nextStateItem);
        assertThat(nextStateItem.getState(), is(instanceOf(DecimalType.class)));
        assertEquals(value(FUTURE_START), nextStateItem.getState());
        nextStateItem = PersistenceExtensions.nextState(numberItem, false, SERVICE_ID);
        numberItem.setState(new DecimalType(FUTURE_START));
        nextStateItem = PersistenceExtensions.nextState(numberItem, false);
        assertNull(nextStateItem);
    public void testNextStateQuantityTypeNoSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(quantityItem, false, SERVICE_ID);
        assertThat(nextStateItem.getState(), is(instanceOf(QuantityType.class)));
        assertEquals(new QuantityType<>(value(FUTURE_START), SIUnits.CELSIUS), nextStateItem.getState());
        nextStateItem = PersistenceExtensions.nextState(quantityItem, false, SERVICE_ID);
        quantityItem.setState(QuantityType.valueOf(FUTURE_START, SIUnits.CELSIUS));
        nextStateItem = PersistenceExtensions.nextState(quantityItem, false);
    public void testNextStateGroupQuantityTypeNoSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(groupQuantityItem, false, SERVICE_ID);
        nextStateItem = PersistenceExtensions.nextState(groupQuantityItem, false, SERVICE_ID);
        groupQuantityItem.setState(QuantityType.valueOf(FUTURE_START, SIUnits.CELSIUS));
        nextStateItem = PersistenceExtensions.nextState(groupQuantityItem, false);
    public void testNextStateDecimalTypeSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(numberItem, true, SERVICE_ID);
        assertEquals(value(FUTURE_START + 1), nextStateItem.getState());
        nextStateItem = PersistenceExtensions.nextState(numberItem, true);
    public void testNextStateQuantityTypeSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(quantityItem, true, SERVICE_ID);
        assertEquals(new QuantityType<>(value(FUTURE_START + 1), SIUnits.CELSIUS), nextStateItem.getState());
        nextStateItem = PersistenceExtensions.nextState(quantityItem, true);
    public void testNextStateGroupQuantityTypeSkip() {
        HistoricItem nextStateItem = PersistenceExtensions.nextState(groupQuantityItem, true, SERVICE_ID);
        nextStateItem = PersistenceExtensions.nextState(groupQuantityItem, true);
    public void testChangedSince() {
        Boolean changed = PersistenceExtensions.changedSince(numberItem,
        assertEquals(changed, true);
        changed = PersistenceExtensions.changedSince(numberItem,
        assertNull(changed);
    public void testChangedUntil() {
        Boolean changed = PersistenceExtensions.changedUntil(numberItem,
        changed = PersistenceExtensions.changedUntil(numberItem,
    public void testChangedBetween() {
        Boolean changed = PersistenceExtensions.changedBetween(numberItem,
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_VALUE_1, 5, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
        assertEquals(changed, false);
        changed = PersistenceExtensions.changedBetween(numberItem,
                ZonedDateTime.of(FUTURE_INTERMEDIATE_VALUE_3, 5, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
    public void testUpdatedSince() {
        Boolean updated = PersistenceExtensions.updatedSince(numberItem,
        assertEquals(updated, true);
        updated = PersistenceExtensions.updatedSince(numberItem,
        assertNull(updated);
    public void testUpdatedUntil() {
        Boolean updated = PersistenceExtensions.updatedUntil(numberItem,
        updated = PersistenceExtensions.updatedUntil(numberItem,
    public void testUpdatedBetween() {
        Boolean updated = PersistenceExtensions.updatedBetween(numberItem,
                ZonedDateTime.of(BEFORE_START, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
        updated = PersistenceExtensions.updatedBetween(numberItem,
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_NOVALUE_3, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                ZonedDateTime.of(HISTORIC_INTERMEDIATE_NOVALUE_4, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
        assertEquals(updated, false);
                ZonedDateTime.of(FUTURE_INTERMEDIATE_NOVALUE_1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                ZonedDateTime.of(FUTURE_INTERMEDIATE_NOVALUE_2, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), SERVICE_ID);
    public void testCountSince() {
        Long counts = PersistenceExtensions.countSince(numberItem,
        assertEquals(HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + 1, counts);
        counts = PersistenceExtensions.countSince(numberItem,
        assertEquals(HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_2 + 1, counts);
        assertEquals(0, counts);
        assertNull(counts);
    public void testCountUntil() {
        Long counts = PersistenceExtensions.countUntil(numberItem,
        counts = PersistenceExtensions.countUntil(numberItem,
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1, counts);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_START + 1, counts);
    public void testCountBetween() {
        Long counts = PersistenceExtensions.countBetween(numberItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_1 - HISTORIC_START + 1, counts);
        counts = PersistenceExtensions.countBetween(numberItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1 + 1, counts);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_INTERMEDIATE_VALUE_3 + 1, counts);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START + 1 + HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1 + 1,
                counts);
    public void testCountStateChangesSince() {
        Long counts = PersistenceExtensions.countStateChangesSince(numberItem,
        assertEquals(HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_1, counts);
        counts = PersistenceExtensions.countStateChangesSince(numberItem,
        assertEquals(HISTORIC_END - HISTORIC_INTERMEDIATE_VALUE_2, counts);
    public void testCountStateChangesUntil() {
        Long counts = PersistenceExtensions.countStateChangesUntil(numberItem,
        counts = PersistenceExtensions.countStateChangesUntil(numberItem,
        assertEquals(FUTURE_INTERMEDIATE_VALUE_3 - FUTURE_START, counts);
        assertEquals(FUTURE_INTERMEDIATE_VALUE_4 - FUTURE_START, counts);
    public void testCountStateChangesBetween() {
        Long counts = PersistenceExtensions.countStateChangesBetween(numberItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_1 - HISTORIC_START, counts);
        counts = PersistenceExtensions.countStateChangesBetween(numberItem,
        assertEquals(HISTORIC_INTERMEDIATE_VALUE_2 - HISTORIC_INTERMEDIATE_VALUE_1, counts);
    public void testPersistState() {
        assertNotNull(PersistenceExtensions.getAllStatesSince(numberItem, now.minusHours(historicHours),
                TestCachedValuesPersistenceService.ID));
        assertThat(PersistenceExtensions.countSince(numberItem, now.minusHours(historicHours),
                TestCachedValuesPersistenceService.ID), is(5L));
        HistoricItem historicItem = PersistenceExtensions.previousState(numberItem,
        assertThat(historicItem.getState(), is(new DecimalType(0)));
        PersistenceExtensions.persist(numberItem, TestCachedValuesPersistenceService.ID);
        verify(persistenceManagerMock).handleExternalPersistenceDataChange(any(), eq(numberItem));
        historicItem = PersistenceExtensions.previousState(numberItem, TestCachedValuesPersistenceService.ID);
        assertThat(historicItem.getState(), is(STATE));
    public void testPersistStateAtTime() {
        PersistenceExtensions.persist(numberItem, now.minusMinutes(105), STATE, TestCachedValuesPersistenceService.ID);
        historicItem = PersistenceExtensions.persistedState(numberItem, now.minusMinutes(90),
    public void testPersistTimeSeries() {
        assertNotNull(PersistenceExtensions.getAllStatesBetween(numberItem, now.minusHours(historicHours),
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID));
        assertThat(PersistenceExtensions.countBetween(numberItem, now.minusHours(historicHours),
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID), is(10L));
        historicItem = PersistenceExtensions.nextState(numberItem, TestCachedValuesPersistenceService.ID);
        TimeSeries timeSeries = new TimeSeries(Policy.REPLACE);
        timeSeries.add(now.minusHours(5).toInstant(), STATE);
        timeSeries.add(now.plusHours(5).toInstant(), STATE);
        PersistenceExtensions.persist(numberItem, timeSeries, TestCachedValuesPersistenceService.ID);
        verify(persistenceManagerMock, times(1)).handleExternalPersistenceDataChange(any(), eq(numberItem));
        historicItem = PersistenceExtensions.persistedState(numberItem, now.minusHours(6),
        historicItem = PersistenceExtensions.persistedState(numberItem, now.plusHours(6),
    public void testRemoveAllStatesSince() {
        PersistenceExtensions.removeAllStatesSince(numberItem, now.minusHours(1),
                TestCachedValuesPersistenceService.ID), is(4L));
        assertThat(historicItem.getState(), is(new DecimalType(50)));
        PersistenceExtensions.removeAllStatesSince(numberItem, now.minusHours(3),
        verify(persistenceManagerMock, times(2)).handleExternalPersistenceDataChange(any(), eq(numberItem));
                TestCachedValuesPersistenceService.ID), is(3L));
        PersistenceExtensions.removeAllStatesSince(numberItem, now.minusHours(historicHours + 1),
        verify(persistenceManagerMock, times(3)).handleExternalPersistenceDataChange(any(), eq(numberItem));
                TestCachedValuesPersistenceService.ID), is(0L));
    public void testRemoveAllStatesUntil() {
        assertNotNull(PersistenceExtensions.getAllStatesUntil(numberItem, now.plusHours(futureHours),
        assertThat(PersistenceExtensions.countUntil(numberItem, now.plusHours(futureHours),
        HistoricItem historicItem = PersistenceExtensions.nextState(numberItem, TestCachedValuesPersistenceService.ID);
        PersistenceExtensions.removeAllStatesUntil(numberItem, now.plusHours(1), TestCachedValuesPersistenceService.ID);
        PersistenceExtensions.removeAllStatesUntil(numberItem, now.plusHours(2), TestCachedValuesPersistenceService.ID);
        PersistenceExtensions.removeAllStatesUntil(numberItem, now.plusHours(futureHours + 1),
    public void testRemoveAllStatesBetween() {
        PersistenceExtensions.removeAllStatesBetween(numberItem, now.minusHours(2), now.minusHours(1),
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID), is(8L));
        PersistenceExtensions.removeAllStatesBetween(numberItem, now.plusHours(1), now.plusHours(2),
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID), is(6L));
        PersistenceExtensions.removeAllStatesBetween(numberItem, now.minusHours(historicHours - 2),
                now.plusHours(futureHours - 2), TestCachedValuesPersistenceService.ID);
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID), is(3L));
        assertThat(historicItem.getState(), is(new DecimalType(100)));
        PersistenceExtensions.removeAllStatesBetween(numberItem, now.minusHours(historicHours + 1),
                now.plusHours(futureHours + 1), TestCachedValuesPersistenceService.ID);
        verify(persistenceManagerMock, times(4)).handleExternalPersistenceDataChange(any(), eq(numberItem));
                now.plusHours(futureHours), TestCachedValuesPersistenceService.ID), is(0L));
    private void createTestCachedValuesPersistenceService(ZonedDateTime now, int historicHours, int futureHours) {
        // Check that test is relevant and fail if badly configured
        assertTrue(historicHours == 0 || historicHours > 5);
        assertTrue(futureHours == 0 || futureHours > 5);
        TestCachedValuesPersistenceService persistenceService = new TestCachedValuesPersistenceService();
                return Set.of(persistenceService);
                return TestCachedValuesPersistenceService.ID.equals(serviceId) ? persistenceService : null;
        if (historicHours > 0) {
            ZonedDateTime beginHistory = now.minusHours(historicHours);
            persistenceService.store(numberItem, beginHistory, new DecimalType(0));
            persistenceService.store(numberItem, beginHistory.plusHours(1), new DecimalType(100));
            persistenceService.store(numberItem, beginHistory.plusHours(2), new DecimalType(0));
            persistenceService.store(numberItem, now.minusHours(2), new DecimalType(50));
            persistenceService.store(numberItem, now.minusHours(1), new DecimalType(0));
        numberItem.setState(new DecimalType(0));
        if (futureHours > 0) {
            ZonedDateTime endFuture = now.plusHours(futureHours);
            persistenceService.store(numberItem, now.plusHours(1), new DecimalType(0));
            persistenceService.store(numberItem, now.plusHours(2), new DecimalType(50));
            persistenceService.store(numberItem, now.plusHours(3), new DecimalType(0));
            persistenceService.store(numberItem, endFuture.minusHours(2), new DecimalType(100));
            persistenceService.store(numberItem, endFuture.minusHours(1), new DecimalType(0));
