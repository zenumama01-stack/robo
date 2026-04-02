 * Applies the given parameter "offset" to a {@link DateTimeType} state.
public class TimestampOffsetProfile implements StateProfile {
    private final Logger logger = LoggerFactory.getLogger(TimestampOffsetProfile.class);
    private final Duration offset;
    public TimestampOffsetProfile(ProfileCallback callback, ProfileContext context) {
        Object offsetParam = context.getConfiguration().get(OFFSET_PARAM);
        logger.debug("Configuring profile with {} parameter '{}'", OFFSET_PARAM, offsetParam);
        if (offsetParam instanceof Number bd) {
            offset = Duration.ofSeconds(bd.longValue());
        } else if (offsetParam instanceof String s) {
            offset = Duration.ofSeconds(Long.parseLong(s));
                    "Parameter '{}' is not of type String or Number. Please make sure it is one of both, e.g. 3 or \"-1\".",
            offset = Duration.ZERO;
        return SystemProfiles.TIMESTAMP_OFFSET;
    private Type applyOffset(Type type, boolean towardsItem) {
        if (type instanceof UnDefType) {
        Duration finalOffset = towardsItem ? offset : offset.negated();
        if (type instanceof DateTimeType timeType) {
            Instant instant = timeType.getInstant();
            // apply offset
            if (!Duration.ZERO.equals(offset)) {
                // we do not need apply an offset equals to 0
                instant = instant.plus(finalOffset);
            result = new DateTimeType(instant);
                    offset, type);
            result = type;
