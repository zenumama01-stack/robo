 * A {@link StateProfile} defined the communication for channels of STATE kind.
public interface StateProfile extends Profile {
     * Will be called if a command should be forwarded to the binding.
    void onCommandFromItem(Command command);
    default void onCommandFromItem(Command command, @Nullable String source) {
        onCommandFromItem(command);
     * If a binding issued a command to a channel, this method will be called for each linked item.
    void onCommandFromHandler(Command command);
     * If the binding indicated a state update on a channel, then this method will be called for each linked item.
    void onStateUpdateFromHandler(State state);
