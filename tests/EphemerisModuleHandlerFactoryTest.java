 * Basic test cases for {@link EphemerisModuleHandlerFactory}
public class EphemerisModuleHandlerFactoryTest {
    private @NonNullByDefault({}) EphemerisModuleHandlerFactory factory;
    private @NonNullByDefault({}) Module moduleMock;
        factory = new EphemerisModuleHandlerFactory(mock(EphemerisManager.class));
        moduleMock = mock(Condition.class);
        when(moduleMock.getId()).thenReturn("My id");
    public void testFactoryFailsCreatingModuleHandlerForDaysetCondition() {
        when(moduleMock.getTypeUID()).thenReturn(EphemerisConditionHandler.DAYSET_MODULE_TYPE_ID);
        when(moduleMock.getConfiguration()).thenReturn(new Configuration());
        assertThrows(IllegalArgumentException.class, () -> factory.internalCreate(moduleMock, "My first rule"));
    public void testFactoryCreatesModuleHandlerForDaysetCondition() {
        when(moduleMock.getConfiguration()).thenReturn(new Configuration(Map.of("dayset", "school")));
        ModuleHandler handler = factory.internalCreate(moduleMock, "My second rule");
        assertThat(handler, is(notNullValue()));
        assertThat(handler, instanceOf(EphemerisConditionHandler.class));
    public void testFactoryCreatesModuleHandlerForWeekdayCondition() {
        when(moduleMock.getTypeUID()).thenReturn(EphemerisConditionHandler.WEEKDAY_MODULE_TYPE_ID);
        ModuleHandler handler = factory.internalCreate(moduleMock, "My first rule");
        when(moduleMock.getConfiguration()).thenReturn(new Configuration(Map.of("offset", 5)));
        handler = factory.internalCreate(moduleMock, "My second rule");
    public void testFactoryCreatesModuleHandlerForHolidayCondition() {
        when(moduleMock.getTypeUID()).thenReturn(EphemerisConditionHandler.HOLIDAY_MODULE_TYPE_ID);
    public void testFactoryCreatesModuleHandlerForNotHolidayCondition() {
        when(moduleMock.getTypeUID()).thenReturn(EphemerisConditionHandler.NOT_HOLIDAY_MODULE_TYPE_ID);
