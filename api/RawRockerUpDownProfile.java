import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_UP_DOWN;
 * The {@link RawRockerUpDownProfile} transforms rocker switch channel events into UP and DOWM commands. Can be used on
 * a {@link RollershutterItem}.
public class RawRockerUpDownProfile implements TriggerProfile {
    RawRockerUpDownProfile(ProfileCallback callback) {
        return RAWROCKER_UP_DOWN;
            callback.sendCommand(UpDownType.UP);
            callback.sendCommand(UpDownType.DOWN);
