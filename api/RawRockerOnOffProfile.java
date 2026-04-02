import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_ON_OFF;
 * The {@link RawRockerOnOffProfile} transforms rocker switch channel events into ON and OFF commands.
public class RawRockerOnOffProfile implements TriggerProfile {
    RawRockerOnOffProfile(ProfileCallback callback) {
        return RAWROCKER_ON_OFF;
