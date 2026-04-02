import org.junit.jupiter.api.Assertions;
 * The {@link ConfigParserTest} contains tests for the {@link ConfigParser}
public class ConfigParserTest {
    private static final List<TestParameter<?>> TEST_PARAMETERS = List.of( //
            // float/Float
            new TestParameter<>("7.5", float.class, 7.5f), //
            new TestParameter<>("-7.5", Float.class, -7.5f), //
            new TestParameter<>(-7.5, float.class, -7.5f), //
            new TestParameter<>(7.5, Float.class, 7.5f), //
            // double/Double
            new TestParameter<>("7.5", double.class, 7.5), //
            new TestParameter<>("-7.5", Double.class, -7.5), //
            new TestParameter<>(-7.5, double.class, -7.5), //
            new TestParameter<>(7.5, Double.class, 7.5), //
            // long/Long
            new TestParameter<>("1", long.class, 1L), //
            new TestParameter<>("-1", Long.class, -1L), //
            new TestParameter<>(-1, long.class, -1L), //
            new TestParameter<>(1, Long.class, 1L), //
            // int/Integer
            new TestParameter<>("1", int.class, 1), //
            new TestParameter<>("-1", Integer.class, -1), //
            new TestParameter<>(-1, int.class, -1), //
            new TestParameter<>(1, Integer.class, 1), //
            // short/Short
            new TestParameter<>("1", short.class, (short) 1), //
            new TestParameter<>("-1", Short.class, (short) -1), //
            new TestParameter<>(-1, short.class, (short) -1), //
            new TestParameter<>(1, Short.class, (short) 1), //
            // byte/Byte
            new TestParameter<>("1", byte.class, (byte) 1), //
            new TestParameter<>("-1", Byte.class, (byte) -1), //
            new TestParameter<>(-1, byte.class, (byte) -1), //
            new TestParameter<>(1, Byte.class, (byte) 1), //
            // boolean/Boolean
            new TestParameter<>("true", boolean.class, true), //
            new TestParameter<>("true", Boolean.class, true), //
            new TestParameter<>(false, boolean.class, false), //
            new TestParameter<>(false, Boolean.class, false), //
            // BigDecimal
            new TestParameter<>("7.5", BigDecimal.class, BigDecimal.valueOf(7.5)), //
            new TestParameter<>(BigDecimal.valueOf(-7.5), BigDecimal.class, BigDecimal.valueOf(-7.5)), //
            new TestParameter<>(1, BigDecimal.class, BigDecimal.ONE), //
            // String
            new TestParameter<>("foo", String.class, "foo"), //
            // Enum
            new TestParameter<>("ENUM1", TestEnum.class, TestEnum.ENUM1), //
            // List
            new TestParameter<>("1", List.class, List.of("1")), //
            new TestParameter<>(List.of(1, 2, 3), List.class, List.of(1, 2, 3)),
            // Set
            new TestParameter<>("1", Set.class, Set.of("1")), //
            new TestParameter<>(Set.of(1, 2, 3), Set.class, Set.of(1, 2, 3)), //
            // illegal conversion
            new TestParameter<>(1, Boolean.class, null), //
            // null input
            new TestParameter<>(null, Object.class, null) //
    private static Stream<TestParameter<?>> valueAsTest() {
        return TEST_PARAMETERS.stream();
    @MethodSource
    public void valueAsTest(TestParameter<?> parameter) {
        Object result = ConfigParser.valueAs(parameter.input, parameter.type);
        Assertions.assertEquals(parameter.result, result, "Failed equals: " + parameter);
    public void valueAsDefaultTest() {
        Object result = ConfigParser.valueAsOrElse(null, String.class, "foo");
        Assertions.assertEquals("foo", result);
    private enum TestEnum {
        ENUM1
    private static class TestParameter<T> {
        public final @Nullable Object input;
        public final Class<T> type;
        public final @Nullable T result;
        public TestParameter(@Nullable Object input, Class<T> type, @Nullable T result) {
            return "TestParameter{input=" + input + ", type=" + type + ", result=" + result + "}";
