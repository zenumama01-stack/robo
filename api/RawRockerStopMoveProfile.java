import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_STOP_MOVE;
 * The {@link RawRockerStopMoveProfile} transforms rocker switch channel events into STOP and MOVE commands. Can be used
 * on a {@link RollershutterItem}.
public class RawRockerStopMoveProfile implements TriggerProfile {
    RawRockerStopMoveProfile(ProfileCallback callback) {
        return RAWROCKER_STOP_MOVE;
            callback.sendCommand(StopMoveType.MOVE);
            callback.sendCommand(StopMoveType.STOP);
