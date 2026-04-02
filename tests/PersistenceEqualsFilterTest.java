 * The {@link PersistenceEqualsFilterTest} contains tests for {@link PersistenceEqualsFilter}
public class PersistenceEqualsFilterTest {
    private @Mock @NonNullByDefault({}) GenericItem item;
    @MethodSource("argumentProvider")
    public void equalsFilterTest(State state, Collection<String> values, boolean expected) {
        when(item.getState()).thenReturn(state);
        PersistenceEqualsFilter filter = new PersistenceEqualsFilter("filter", values, false);
        assertThat(filter.apply(item), is(expected));
    public void notEqualsFilterTest(State state, Collection<String> values, boolean expected) {
        PersistenceEqualsFilter filter = new PersistenceEqualsFilter("filter", values, true);
        assertThat(filter.apply(item), is(not(expected)));
    private static Stream<Arguments> argumentProvider() {
        return Stream.of(//
                // item state, values, result
                Arguments.of(new StringType("value1"), List.of("value1", "value2"), true),
                Arguments.of(new StringType("value3"), List.of("value1", "value2"), false),
                Arguments.of(new DecimalType(5), List.of("3", "5", "9"), true),
                Arguments.of(new DecimalType(7), List.of("3", "5", "9"), false),
                Arguments.of(new QuantityType<>(10, SIUnits.CELSIUS), List.of("5 °C", "10 °C", "15 °C"), true),
                Arguments.of(new QuantityType<>(20, SIUnits.CELSIUS), List.of("5 °C", "10 °C", "15 °C"), false),
                Arguments.of(OnOffType.ON, List.of("ON", "UNDEF", "NULL"), true),
                Arguments.of(OnOffType.OFF, List.of("ON", "UNDEF", "NULL"), false));
