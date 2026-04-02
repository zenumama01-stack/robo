import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_REWIND_FASTFORWARD;
 * The {@link RawRockerRewindFastforwardProfile} transforms rocker switch channel events into REWIND and FASTFORWARD
 * commands. Can be used on a {@link PlayerItem}.
public class RawRockerRewindFastforwardProfile implements TriggerProfile {
    RawRockerRewindFastforwardProfile(ProfileCallback callback) {
        return RAWROCKER_REWIND_FASTFORWARD;
            callback.sendCommand(RewindFastforwardType.FASTFORWARD);
            callback.sendCommand(RewindFastforwardType.REWIND);
