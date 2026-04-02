 * The {@link RollershutterChannelHandler} implements {@link org.openhab.core.library.items.RollershutterItem}
public class RollershutterChannelHandler extends AbstractTransformingChannelHandler {
    public RollershutterChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
            final String downValue = channelConfig.downValue;
            final String upValue = channelConfig.upValue;
            if (command.equals(PercentType.HUNDRED) && downValue != null) {
                return downValue;
            } else if (command.equals(PercentType.ZERO) && upValue != null) {
                return upValue;
        if (string.equals(channelConfig.upValue)) {
            return UpDownType.UP;
        } else if (string.equals(channelConfig.downValue)) {
            return UpDownType.DOWN;
        } else if (string.equals(channelConfig.moveValue)) {
            return StopMoveType.MOVE;
        } else if (string.equals(channelConfig.stopValue)) {
            return StopMoveType.STOP;
