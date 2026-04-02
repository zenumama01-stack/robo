import static org.openhab.core.thing.profiles.SystemProfiles.TRIGGER_EVENT_STRING;
 * The {@link TriggerEventStringProfile} transforms a trigger event to a String
public class TriggerEventStringProfile implements TriggerProfile {
    TriggerEventStringProfile(ProfileCallback callback) {
        callback.sendCommand(new StringType(event));
