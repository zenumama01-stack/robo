 * Gives access to the framework features for continuing the communication flow.
public interface ProfileCallback {
     * Get the link that this profile is associated with.
     * @return The ItemChannelLink
    ItemChannelLink getItemChannelLink();
     * Forward the given command to the respective thing handler.
    void handleCommand(Command command);
     * Send a command to the framework.
    default void sendCommand(Command command) {
        sendCommand(command, null);
     * @param source the source of the command event
    void sendCommand(Command command, @Nullable String source);
     * Send a state update to the framework.
     * @param state
    default void sendUpdate(State state) {
        sendUpdate(state, null);
     * @param source the source of the update
    void sendUpdate(State state, @Nullable String source);
     * Send a {@link TimeSeries} update to the framework.
     * @param timeSeries
    default void sendTimeSeries(TimeSeries timeSeries) {
        sendTimeSeries(timeSeries, null);
    void sendTimeSeries(TimeSeries timeSeries, @Nullable String source);
