import javax.measure.quantity.Temperature;
import org.openhab.core.library.items.DimmerItem;
import org.openhab.core.library.items.NumberItem;
import org.openhab.core.library.items.SwitchItem;
 * Basic unit tests for {@link ItemStateConditionHandler}.
public class ItemStateConditionHandlerTest extends JavaTest {
    public static class ParameterSet {
        public final Item item;
        public final String comparisonState;
        public final State itemState;
        public final boolean expectedResult;
        public ParameterSet(String itemType, String comparisonState, State itemState, boolean expectedResult) {
            switch (itemType) {
                case "Number":
                    item = new NumberItem(ITEM_NAME);
                    ((NumberItem) item).setState(itemState);
                case "Number:Temperature":
                    UnitProvider unitProviderMock = mock(UnitProvider.class);
                    when(unitProviderMock.getUnit(Temperature.class)).thenReturn(SIUnits.CELSIUS);
                    item = new NumberItem("Number:Temperature", ITEM_NAME, unitProviderMock);
                case "Dimmer":
                    item = new DimmerItem(ITEM_NAME);
                    ((DimmerItem) item).setState(itemState);
                case "DateTime":
                    item = new DateTimeItem(ITEM_NAME);
                    ((DateTimeItem) item).setState(itemState);
                    throw new IllegalArgumentException();
            this.comparisonState = comparisonState;
            this.itemState = itemState;
            this.expectedResult = expectedResult;
    public static Collection<Object[]> equalsParameters() {
        return List.of(new Object[][] { //
                { new ParameterSet("Number", "5", new DecimalType(23), false) }, //
                { new ParameterSet("Number", "5", new DecimalType(5), true) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new DecimalType(23), false) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new DecimalType(5), true) }, //
                { new ParameterSet("Number:Temperature", "0", new DecimalType(), false) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(23, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(5, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(23, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(5, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Number:Temperature", "0 °C", new QuantityType<>(32, ImperialUnits.FAHRENHEIT),
                        true) }, //
                { new ParameterSet("Number:Temperature", "32 °F", new QuantityType<>(0, SIUnits.CELSIUS), true) } });
    public static Collection<Object[]> greaterThanParameters() {
                { new ParameterSet("Number", "5", new DecimalType(23), true) }, //
                { new ParameterSet("Number", "5", new DecimalType(5), false) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(23), true) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(5), false) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(23, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(23, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(5, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Dimmer", "20", new PercentType(40), true) }, //
                { new ParameterSet("Dimmer", "20", new PercentType(20), false) }, //
                { new ParameterSet("DateTime", "-1H", new DateTimeType(), true) }, //
                { new ParameterSet("DateTime", "1D1M", new DateTimeType(), false) } });
    public static Collection<Object[]> greaterThanOrEqualsParameters() {
                { new ParameterSet("Number", "5", new DecimalType(4), false) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(5), true) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(4), false) }, //
                { new ParameterSet("Number:Temperature", "0", new DecimalType(), true) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(5, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(4, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(4, SIUnits.CELSIUS), false) }, //
                { new ParameterSet("Number:Temperature", "32 °F", new QuantityType<>(0, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Dimmer", "40", new PercentType(20), false) }, //
                { new ParameterSet("DateTime", "2000-01-01T12:05:00", new DateTimeType(), true) } });
    public static Collection<Object[]> lessThanParameters() {
                { new ParameterSet("Number", "5", new DecimalType(4), true) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(23), false) }, //
                { new ParameterSet("Number", "5 °C", new DecimalType(4), true) }, //
                { new ParameterSet("Number:Temperature", "5", new QuantityType<>(4, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Number:Temperature", "5 °C", new QuantityType<>(4, SIUnits.CELSIUS), true) }, //
                { new ParameterSet("Dimmer", "40", new PercentType(20), true) }, //
                { new ParameterSet("DateTime", "-1D", new DateTimeType(), false) }, //
                { new ParameterSet("DateTime", "1D5M", new DateTimeType(), true) }, //
                { new ParameterSet("DateTime", "2050-01-01T12:05:00+01:00", new DateTimeType(), true) } });
    public static Collection<Object[]> lessThanOrEqualsParameters() {
                { new ParameterSet("Dimmer", "20", new PercentType(40), false) }, //
                { new ParameterSet("DateTime", "", new DateTimeType(), true) } });
    private @NonNullByDefault({}) Item item;
    private @Mock @NonNullByDefault({}) TimeZoneProvider mockTimeZoneProvider;
        when(mockItemRegistry.getItem(ITEM_NAME)).thenAnswer(i -> item);
        when(mockItemRegistry.get(ITEM_NAME)).thenAnswer(i -> item);
        when(mockTimeZoneProvider.getTimeZone()).thenReturn(ZoneId.systemDefault());
    public Item getItem() {
    @MethodSource("equalsParameters")
    public void testEqualsCondition(ParameterSet parameterSet) {
        item = parameterSet.item;
        ItemStateConditionHandler handler = initItemStateConditionHandler("=", parameterSet.comparisonState);
        if (parameterSet.expectedResult) {
            assertTrue(handler.isSatisfied(Map.of()),
                    parameterSet.item + ", comparisonState=" + parameterSet.comparisonState);
            assertFalse(handler.isSatisfied(Map.of()),
    public void testNotEqualsCondition(ParameterSet parameterSet) {
        ItemStateConditionHandler handler = initItemStateConditionHandler("!=", parameterSet.comparisonState);
            assertFalse(handler.isSatisfied(Map.of()));
            assertTrue(handler.isSatisfied(Map.of()));
    @MethodSource("greaterThanParameters")
    public void testGreaterThanCondition(ParameterSet parameterSet) {
        ItemStateConditionHandler handler = initItemStateConditionHandler(">", parameterSet.comparisonState);
    @MethodSource("greaterThanOrEqualsParameters")
    public void testGreaterThanOrEqualsCondition(ParameterSet parameterSet) {
        ItemStateConditionHandler handler = initItemStateConditionHandler(">=", parameterSet.comparisonState);
    @MethodSource("lessThanParameters")
    public void testLessThanCondition(ParameterSet parameterSet) {
        ItemStateConditionHandler handler = initItemStateConditionHandler("<", parameterSet.comparisonState);
    @MethodSource("lessThanOrEqualsParameters")
    public void testLessThanOrEqualsCondition(ParameterSet parameterSet) {
        ItemStateConditionHandler handler = initItemStateConditionHandler("<=", parameterSet.comparisonState);
    private ItemStateConditionHandler initItemStateConditionHandler(String operator, String state) {
        configuration.put(ItemStateConditionHandler.ITEM_NAME, ITEM_NAME);
        configuration.put(ItemStateConditionHandler.OPERATOR, operator);
        configuration.put(ItemStateConditionHandler.STATE, state);
        ConditionBuilder builder = ConditionBuilder.create() //
                .withId("conditionId") //
                .withTypeUID(ItemStateConditionHandler.ITEM_STATE_CONDITION) //
                .withConfiguration(configuration);
        return new ItemStateConditionHandler(builder.build(), "", mockBundleContext, mockItemRegistry,
                mockTimeZoneProvider);
    public void itemMessagesAreLogged() {
        configuration.put(ItemStateConditionHandler.OPERATOR, "=");
        Condition condition = ConditionBuilder.create() //
                .withConfiguration(configuration) //
        setupInterceptedLogger(ItemStateConditionHandler.class, LogLevel.INFO);
        // missing on creation
        when(mockItemRegistry.get(ITEM_NAME)).thenReturn(null);
        ItemStateConditionHandler handler = new ItemStateConditionHandler(condition, "foo", mockBundleContext,
                mockItemRegistry, mockTimeZoneProvider);
        assertLogMessage(ItemStateConditionHandler.class, LogLevel.WARN,
                "Item 'myItem' needed for rule 'foo' is missing. Condition 'conditionId' will not work.");
        // added later
        ItemAddedEvent addedEvent = ItemEventFactory.createAddedEvent(new SwitchItem(ITEM_NAME));
        assertTrue(handler.getEventFilter().apply(addedEvent));
        handler.receive(addedEvent);
        assertLogMessage(ItemStateConditionHandler.class, LogLevel.INFO,
                "Item 'myItem' needed for rule 'foo' added. Condition 'conditionId' will now work.");
        // removed later
        ItemRemovedEvent removedEvent = ItemEventFactory.createRemovedEvent(new SwitchItem(ITEM_NAME));
        assertTrue(handler.getEventFilter().apply(removedEvent));
        handler.receive(removedEvent);
                "Item 'myItem' needed for rule 'foo' removed. Condition 'conditionId' will no longer work.");
