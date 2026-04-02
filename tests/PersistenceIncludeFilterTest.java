 * The {@link PersistenceIncludeFilterTest} contains tests for {@link PersistenceIncludeFilter}
public class PersistenceIncludeFilterTest {
    public void includeFilterTest(State state, BigDecimal lower, BigDecimal upper, String unit, boolean expected) {
        PersistenceIncludeFilter filter = new PersistenceIncludeFilter("filter", lower, upper, unit, false);
    @MethodSource("notArgumentProvider")
    public void notIncludeFilterTest(State state, BigDecimal lower, BigDecimal upper, String unit, boolean expected) {
        PersistenceIncludeFilter filter = new PersistenceIncludeFilter("filter", lower, upper, unit, true);
                // item state, lower, upper, unit, result
                // QuantityType
                Arguments.of(new QuantityType<>("17 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "°C", true),
                Arguments.of(new QuantityType<>("17 °C"), BigDecimal.valueOf(17), BigDecimal.valueOf(19), "°C", true),
                Arguments.of(new QuantityType<>("17 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "°C", true),
                Arguments.of(new QuantityType<>("10 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "°C", false),
                Arguments.of(new QuantityType<>("20 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "°C", false),
                Arguments.of(new QuantityType<>("0 °C"), BigDecimal.valueOf(270), BigDecimal.valueOf(275), "K", true),
                // invalid or missing units
                Arguments.of(new QuantityType<>("5 kg"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "°C", true),
                Arguments.of(new QuantityType<>("17 kg"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "°C", true),
                Arguments.of(new QuantityType<>("5 kg"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "", true),
                Arguments.of(new QuantityType<>("17 kg"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "", true),
                // DecimalType
                Arguments.of(new DecimalType("17"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "", true),
                Arguments.of(new DecimalType("17"), BigDecimal.valueOf(17), BigDecimal.valueOf(19), "", true),
                Arguments.of(new DecimalType("17"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "", true),
                Arguments.of(new DecimalType("10"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "", false),
                Arguments.of(new DecimalType("20"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "", false));
    private static Stream<Arguments> notArgumentProvider() {
                Arguments.of(new QuantityType<>("17 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "°C", false),
                Arguments.of(new QuantityType<>("10 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "°C", true),
                Arguments.of(new QuantityType<>("20 °C"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "°C", true),
                Arguments.of(new QuantityType<>("0 °C"), BigDecimal.valueOf(270), BigDecimal.valueOf(275), "K", false),
                Arguments.of(new DecimalType("17"), BigDecimal.valueOf(14), BigDecimal.valueOf(19), "", false),
                Arguments.of(new DecimalType("10"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "", true),
                Arguments.of(new DecimalType("20"), BigDecimal.valueOf(14), BigDecimal.valueOf(17), "", true));
