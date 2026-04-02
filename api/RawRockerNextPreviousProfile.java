import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_NEXT_PREVIOUS;
import org.openhab.core.library.items.PlayerItem;
 * The {@link RawRockerNextPreviousProfile} transforms rocker switch channel events into NEXT and PREVIOUS commands. Can
 * be used on a {@link PlayerItem}.
public class RawRockerNextPreviousProfile implements TriggerProfile {
    RawRockerNextPreviousProfile(ProfileCallback callback) {
        return RAWROCKER_NEXT_PREVIOUS;
            callback.sendCommand(NextPreviousType.NEXT);
            callback.sendCommand(NextPreviousType.PREVIOUS);
