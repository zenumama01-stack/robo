import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_PLAY_PAUSE;
 * The {@link RawRockerPlayPauseProfile} transforms rocker switch channel events into PLAY and PAUSE commands. Can be
 * used on a {@link PlayerItem}.
 * @author Daniel Weber - Initial contribution
public class RawRockerPlayPauseProfile implements TriggerProfile {
    RawRockerPlayPauseProfile(ProfileCallback callback) {
        return RAWROCKER_PLAY_PAUSE;
            callback.sendCommand(PlayPauseType.PLAY);
            callback.sendCommand(PlayPauseType.PAUSE);
