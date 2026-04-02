 * The {@link DimmerChannelHandler} implements {@link org.openhab.core.library.items.DimmerItem} conversions
public class DimmerChannelHandler extends AbstractTransformingChannelHandler {
    public DimmerChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
        if (command instanceof PercentType percentCommand) {
            return percentCommand.toString();
            newState = PercentType.HUNDRED;
            newState = PercentType.ZERO;
        } else if (string.equals(channelConfig.increaseValue) && state instanceof PercentType percentState) {
            BigDecimal newBrightness = percentState.toBigDecimal().add(channelConfig.step);
            newState = new PercentType(newBrightness);
        } else if (string.equals(channelConfig.decreaseValue) && state instanceof PercentType percentState) {
            BigDecimal newBrightness = percentState.toBigDecimal().subtract(channelConfig.step);
                BigDecimal value = new BigDecimal(string);
                if (value.compareTo(PercentType.HUNDRED.toBigDecimal()) > 0) {
                    value = PercentType.HUNDRED.toBigDecimal();
                if (value.compareTo(PercentType.ZERO.toBigDecimal()) < 0) {
                    value = PercentType.ZERO.toBigDecimal();
                newState = new PercentType(value);
