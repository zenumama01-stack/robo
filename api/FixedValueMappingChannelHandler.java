 * The {@link FixedValueMappingChannelHandler} implements mapping conversions for different item-types
public class FixedValueMappingChannelHandler extends AbstractTransformingChannelHandler {
    public FixedValueMappingChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
        String value = channelConfig.commandToFixedValue(command);
                "Command type '" + command.toString() + "' not supported or mapping not defined.");
        State state = channelConfig.fixedValueToState(string);
        return Optional.of(Objects.requireNonNullElse(state, UnDefType.UNDEF));
