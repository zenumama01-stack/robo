import org.openhab.core.thing.profiles.SystemProfiles;
 * This is the default profile for stateful channels.
 * It forwards commands to the {@link ThingHandler}. In the other direction it posts events to the event bus
 * for state updates.
public class SystemDefaultProfile implements TimeSeriesProfile {
    public SystemDefaultProfile(ProfileCallback callback) {
        return SystemProfiles.DEFAULT;
        callback.handleCommand(command);
        callback.sendUpdate(state);
        callback.sendCommand(command);
        callback.sendTimeSeries(timeSeries);
