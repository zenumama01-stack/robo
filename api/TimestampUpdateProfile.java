 * This is the default implementation for a state update timestamp profile.
 * The timestamp updates to now each time the Channel state is updated.
public class TimestampUpdateProfile implements StateProfile {
    private final Logger logger = LoggerFactory.getLogger(TimestampUpdateProfile.class);
    public TimestampUpdateProfile(ProfileCallback callback) {
        return SystemProfiles.TIMESTAMP_UPDATE;
        logger.debug("Received state update from Handler, sending timestamp to callback");
