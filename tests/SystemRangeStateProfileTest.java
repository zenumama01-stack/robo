 * Basic unit tests for {@link SystemRangeStateProfileTest}.
public class SystemRangeStateProfileTest {
        public final boolean inverted;
                @Nullable Object upper, boolean inverted) {
            this.inverted = inverted;
                // lower bound = 10, upper bound = 40 (as BigDecimal), one state update / command (PercentType), not
                // inverted
                { new ParameterSet(List.of(PercentType.HUNDRED), List.of(OnOffType.OFF), BigDecimal.TEN,
                        BIGDECIMAL_FOURTY, false) }, //
                { new ParameterSet(List.of(PERCENT_TYPE_TWENTY_FIVE), List.of(OnOffType.ON), BigDecimal.TEN,
                { new ParameterSet(List.of(PERCENT_TYPE_TEN), List.of(OnOffType.ON), BigDecimal.TEN, BIGDECIMAL_FOURTY,
                        false) }, //
                { new ParameterSet(List.of(PercentType.ZERO), List.of(OnOffType.OFF), BigDecimal.TEN, BIGDECIMAL_FOURTY,
                // lower bound = 10, upper bound = 40 (as BigDecimal), one state update / command (QuantityType), not
                { new ParameterSet(List.of(QuantityType.valueOf("100 %")), List.of(OnOffType.OFF), BigDecimal.TEN,
                { new ParameterSet(List.of(QuantityType.valueOf(QUANTITY_STRING_TEN)), List.of(OnOffType.ON),
                        BigDecimal.TEN, BIGDECIMAL_FOURTY, false) }, //
                { new ParameterSet(List.of(new QuantityType<>()), List.of(OnOffType.OFF), BigDecimal.TEN,
                // lower bound = 10, upper bound = 40 (as QuantityType), one state update / command (QuantityType), not
                { new ParameterSet(List.of(QuantityType.valueOf("100 %")), List.of(OnOffType.OFF), QUANTITY_STRING_TEN,
                        QUANTITY_STRING_FOURTY, false) }, //
                        QUANTITY_STRING_TEN, QUANTITY_STRING_FOURTY, false) }, //
                // lower bound = 10, upper bound = 40 (as QuantityType), one state update / command (QuantityType) ->
                // values
                // are converted to the same unit, not inverted
                { new ParameterSet(List.of(QuantityType.valueOf("10 m")), List.of(OnOffType.OFF), "25 cm", "100cm",
                { new ParameterSet(List.of(QuantityType.valueOf("10 m")), List.of(UnDefType.UNDEF), "25 °C", "30 °C",
                // lower bound = 10, upper bound = 40 (as BigDecimal), one state update / command (PercentType),
                        BIGDECIMAL_FOURTY, true) }, //
                { new ParameterSet(List.of(PERCENT_TYPE_TWENTY_FIVE), List.of(OnOffType.OFF), BigDecimal.TEN,
                { new ParameterSet(List.of(PERCENT_TYPE_TEN), List.of(OnOffType.OFF), BigDecimal.TEN, BIGDECIMAL_FOURTY,
                { new ParameterSet(List.of(PercentType.ZERO), List.of(OnOffType.ON), BigDecimal.TEN, BIGDECIMAL_FOURTY,
                // lower bound = 10, upper bound = 40 (as BigDecimal), one state update / command (QuantityType),
                { new ParameterSet(List.of(QuantityType.valueOf("25 %")), List.of(OnOffType.OFF), BigDecimal.TEN,
                        BigDecimal.TEN, BIGDECIMAL_FOURTY, true) }, //
                { new ParameterSet(List.of(QuantityType.valueOf("0 %")), List.of(OnOffType.ON), BigDecimal.TEN,
                // lower bound = 10, upper bound = 40 (as QuantityType), one state update / command (QuantityType),
                        QUANTITY_STRING_FOURTY, true) }, //
                { new ParameterSet(List.of(QuantityType.valueOf("25 %")), List.of(OnOffType.OFF), QUANTITY_STRING_TEN,
                        QUANTITY_STRING_TEN, QUANTITY_STRING_FOURTY, true) }, //
                { new ParameterSet(List.of(QuantityType.valueOf("0 %")), List.of(OnOffType.ON), QUANTITY_STRING_TEN,
    private @Mock @NonNullByDefault({}) ProfileContext contextMock;
    public void testWrongParameterUpper() {
        assertThrows(IllegalArgumentException.class, () -> initProfile(QUANTITY_STRING_TEN, null));
    public void testWrongParameterUpperLessThanOrEqualsToLower() {
        assertThrows(IllegalArgumentException.class, () -> initProfile(QUANTITY_STRING_FOURTY, QUANTITY_STRING_TEN));
        assertThrows(IllegalArgumentException.class, () -> initProfile(QUANTITY_STRING_FOURTY, QUANTITY_STRING_FOURTY));
        final StateProfile profile = initProfile(parameterSet.lower, parameterSet.upper, parameterSet.inverted);
        properties.put(SystemRangeStateProfile.LOWER_PARAM, lower);
        properties.put(SystemRangeStateProfile.UPPER_PARAM, upper);
        properties.put(SystemRangeStateProfile.INVERTED_PARAM, inverted);
        when(contextMock.getConfiguration()).thenReturn(new Configuration(properties));
        return new SystemRangeStateProfile(callbackMock, contextMock);
            verify(callbackMock, times(1)).sendCommand(eq(eC));
        verify(callbackMock, times(1)).sendUpdate(eq(expectedState));
