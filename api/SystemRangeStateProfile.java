 * This is the default implementation for a {@link SystemRangeStateProfile}.
public class SystemRangeStateProfile implements StateProfile {
    static final String UPPER_PARAM = "upper";
    static final String INVERTED_PARAM = "inverted";
    private final Logger logger = LoggerFactory.getLogger(SystemRangeStateProfile.class);
    private final OnOffType inRange;
    private final OnOffType notInRange;
    public SystemRangeStateProfile(ProfileCallback callback, ProfileContext context) {
        if (upperParam == null) {
            throw new IllegalArgumentException(String.format("Parameter '%s' is not a Number value.", UPPER_PARAM));
        final QuantityType<?> convertedUpperParam = upperParam.toInvertibleUnit(lower.getUnit());
        if (convertedUpperParam.doubleValue() <= lower.doubleValue()) {
                    String.format("Parameter '%s' (%s) is less than or equal to '%s' (%s) parameter.", UPPER_PARAM,
                            convertedUpperParam, LOWER_PARAM, lower));
        this.inRange = OnOffType.from(!inverted);
        this.notInRange = OnOffType.from(inverted);
        return SystemProfiles.RANGE;
        return lower <= value && value <= upper ? inRange : notInRange;
