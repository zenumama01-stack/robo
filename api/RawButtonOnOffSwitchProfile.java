import static org.openhab.core.thing.profiles.SystemProfiles.RAWBUTTON_ON_OFF_SWITCH;
 * The {@link RawButtonOnOffSwitchProfile} transforms raw button switch
 * channel events into ON and OFF commands.
 * @author Mark Hilbush - Initial contribution
public class RawButtonOnOffSwitchProfile implements TriggerProfile {
    RawButtonOnOffSwitchProfile(ProfileCallback callback) {
        return RAWBUTTON_ON_OFF_SWITCH;
    public void onTriggerFromHandler(String event) {
        if (CommonTriggerEvents.PRESSED.equals(event)) {
            callback.sendCommand(OnOffType.ON);
        } else if (CommonTriggerEvents.RELEASED.equals(event)) {
            callback.sendCommand(OnOffType.OFF);
