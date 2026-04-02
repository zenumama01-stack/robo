 * Applies the given parameter "offset" to a {@link QuantityType} or {@link DecimalType} state.
public class SystemOffsetProfile implements TimeSeriesProfile {
    static final String OFFSET_PARAM = "offset";
    private final Logger logger = LoggerFactory.getLogger(SystemOffsetProfile.class);
    private QuantityType<?> offset = QuantityType.ONE;
    public SystemOffsetProfile(ProfileCallback callback, ProfileContext context) {
        Object paramValue = context.getConfiguration().get(OFFSET_PARAM);
        logger.debug("Configuring profile with {} parameter '{}'", OFFSET_PARAM, paramValue);
                offset = new QuantityType<>(string);
                        "Cannot convert value '{}' of parameter '{}' into a valid offset of type QuantityType. Using offset 0 now.",
                        paramValue, OFFSET_PARAM);
        } else if (paramValue instanceof BigDecimal bd) {
            offset = new QuantityType<>(bd.toString());
                    "Parameter '{}' is not of type String or BigDecimal. Please make sure it is one of both, e.g. 3, \"-1.4\" or \"3.2°C\".",
                    OFFSET_PARAM);
        return SystemProfiles.OFFSET;
        callback.handleCommand((Command) applyOffset(command, false));
        callback.sendCommand((Command) applyOffset(command, true));
        callback.sendUpdate((State) applyOffset(state, true));
        timeSeries.getStates().forEach(
                entry -> transformedTimeSeries.add(entry.timestamp(), (State) applyOffset(entry.state(), true)));
    private Type applyOffset(Type state, boolean towardsItem) {
            // we cannot adjust UNDEF or NULL values, thus we simply return them without reporting an error or warning
        QuantityType finalOffset = towardsItem ? offset : offset.negate();
        Type result = UnDefType.UNDEF;
        if (state instanceof QuantityType qtState) {
                if (Units.ONE.equals(finalOffset.getUnit()) && !Units.ONE.equals(qtState.getUnit())) {
                    // allow offsets without unit -> implicitly assume its the same as the one from the state, but warn
                            "Received a QuantityType state '{}' with unit, but the offset is defined as a plain number without unit ({}), please consider adding a unit to the profile offset.",
                            state, offset);
                    finalOffset = new QuantityType<>(finalOffset.toBigDecimal(), qtState.getUnit());
                result = qtState.add(finalOffset);
                logger.warn("Cannot apply offset '{}' to state '{}' because types do not match.", finalOffset, qtState);
        } else if (state instanceof DecimalType decState && Units.ONE.equals(finalOffset.getUnit())) {
            result = new DecimalType(decState.toBigDecimal().add(finalOffset.toBigDecimal()));
                    "Offset '{}' cannot be applied to the incompatible state '{}' sent from the binding. Returning original state.",
                    offset, state);
            result = state;
