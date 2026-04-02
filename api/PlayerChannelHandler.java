 * The {@link PlayerChannelHandler} implements {@link org.openhab.core.library.items.RollershutterItem}
 * conversions
public class PlayerChannelHandler extends AbstractTransformingChannelHandler {
    private @Nullable String lastCommand; // store last command to prevent duplicate commands
    public PlayerChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
    protected @Nullable Command toCommand(String string) {
        if (string.equals(lastCommand)) {
            // only send commands once
        lastCommand = string;
        if (string.equals(channelConfig.playValue)) {
            return PlayPauseType.PLAY;
        } else if (string.equals(channelConfig.pauseValue)) {
            return PlayPauseType.PAUSE;
        } else if (string.equals(channelConfig.nextValue)) {
            return NextPreviousType.NEXT;
        } else if (string.equals(channelConfig.previousValue)) {
            return NextPreviousType.PREVIOUS;
        } else if (string.equals(channelConfig.rewindValue)) {
            return RewindFastforwardType.REWIND;
        } else if (string.equals(channelConfig.fastforwardValue)) {
            return RewindFastforwardType.FASTFORWARD;
        // no value - we ignore state updates
