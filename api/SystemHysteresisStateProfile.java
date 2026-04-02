import tech.units.indriya.AbstractUnit;
 * This is the default implementation for a {@link SystemHysteresisStateProfile}.
public class SystemHysteresisStateProfile implements StateProfile {
    static final String LOWER_PARAM = "lower";
    private static final String UPPER_PARAM = "upper";
    private static final String INVERTED_PARAM = "inverted";
    private final Logger logger = LoggerFactory.getLogger(SystemHysteresisStateProfile.class);
    private final QuantityType<?> lower;
    private final QuantityType<?> upper;
    private final OnOffType low;
    private final OnOffType high;
    private Type previousType = UnDefType.UNDEF;
    public SystemHysteresisStateProfile(ProfileCallback callback, ProfileContext context) {
        final QuantityType<?> lowerParam = getParam(context, LOWER_PARAM);
        if (lowerParam == null) {
            throw new IllegalArgumentException(String.format("Parameter '%s' is not a Number value.", LOWER_PARAM));
        this.lower = lowerParam;
        final QuantityType<?> upperParam = getParam(context, UPPER_PARAM);
        final QuantityType<?> convertedUpperParam = upperParam == null ? lower
                : upperParam.toInvertibleUnit(lower.getUnit());
        if (convertedUpperParam == null) {
                    String.format("Units of parameters '%s' and '%s' are not compatible: %s != %s", LOWER_PARAM,
                            UPPER_PARAM, lower, upperParam));
        this.upper = convertedUpperParam;
        final Object paramValue = context.getConfiguration().get(INVERTED_PARAM);
        final boolean inverted = paramValue != null && Boolean.parseBoolean(paramValue.toString());
        this.low = OnOffType.from(inverted);
        this.high = OnOffType.from(!inverted);
    private @Nullable QuantityType<?> getParam(ProfileContext context, String param) {
        final Object paramValue = context.getConfiguration().get(param);
        logger.debug("Configuring profile with {} parameter '{}'", param, paramValue);
        if (paramValue instanceof String string) {
                return new QuantityType<>(string);
                logger.error("Cannot convert value '{}' of parameter {} into a valid QuantityType.", paramValue, param);
        } else if (paramValue instanceof BigDecimal value) {
            return QuantityType.valueOf(value.doubleValue(), AbstractUnit.ONE);
        return SystemProfiles.HYSTERESIS;
        final Type mappedCommand = mapValue(command);
        logger.trace("Mapped command from '{}' to command '{}'.", command, mappedCommand);
        if (mappedCommand instanceof Command command1) {
            callback.sendCommand(command1);
        final Type mappedState = mapValue(state);
        logger.trace("Mapped state from '{}' to state '{}'.", state, mappedState);
        if (mappedState instanceof State state1) {
            callback.sendUpdate(state1);
    private Type mapValue(Type value) {
        if (value instanceof QuantityType qtState) {
            final QuantityType<?> finalLower;
            final QuantityType<?> finalUpper;
            if (Units.ONE.equals(lower.getUnit()) && Units.ONE.equals(upper.getUnit())) {
                // allow bounds without unit -> implicitly assume its the same as the one from the state, but warn
                // the user
                            "Received a QuantityType '{}' with unit, but the boundaries are defined as a plain number without units (lower={}, upper={}), please consider adding units to them.",
                            value, lower, upper);
                finalLower = new QuantityType<>(lower.toBigDecimal(), qtState.getUnit());
                finalUpper = new QuantityType<>(upper.toBigDecimal(), qtState.getUnit());
                finalLower = lower.toInvertibleUnit(qtState.getUnit());
                finalUpper = upper.toInvertibleUnit(qtState.getUnit());
                if (finalLower == null || finalUpper == null) {
                            "Cannot compare state '{}' to boundaries because units (lower={}, upper={}) do not match.",
                            qtState, lower, upper);
                    return previousType;
            return previousType = mapValue(finalLower.doubleValue(), finalUpper.doubleValue(), qtState.doubleValue());
        } else if (value instanceof DecimalType type) {
            return previousType = mapValue(lower.doubleValue(), upper.doubleValue(), type.doubleValue());
    private Type mapValue(double lower, double upper, double value) {
        if (value <= lower) {
            return low;
        } else if (value >= upper) {
            return high;
