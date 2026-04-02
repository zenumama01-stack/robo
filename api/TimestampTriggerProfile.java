import static org.openhab.core.thing.profiles.SystemProfiles.TIMESTAMP_TRIGGER;
 * This is the default implementation for a trigger timestamp profile.
 * The timestamp updates to now each time the channel is triggered.
public class TimestampTriggerProfile implements TriggerProfile {
    private final Logger logger = LoggerFactory.getLogger(TimestampTriggerProfile.class);
    TimestampTriggerProfile(ProfileCallback callback) {
        return TIMESTAMP_TRIGGER;
        logger.debug("Received trigger from Handler, sending timestamp to callback");
