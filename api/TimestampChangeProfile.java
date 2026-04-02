 * This is the default implementation for a change timestamp profile.
 * The timestamp updates to now each time the Channel state changes.
public class TimestampChangeProfile implements StateProfile {
    private final Logger logger = LoggerFactory.getLogger(TimestampChangeProfile.class);
    private @Nullable State previousState;
    public TimestampChangeProfile(ProfileCallback callback) {
        return SystemProfiles.TIMESTAMP_CHANGE;
        logger.debug("Received state update from Handler");
        if (previousState == null || !state.equals(previousState.as(state.getClass()))) {
            logger.debug("Item state changed, sending timestamp to callback");
            callback.sendUpdate(new DateTimeType());
