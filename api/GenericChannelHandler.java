 * The {@link GenericChannelHandler} implements simple conversions for different item types
public class GenericChannelHandler extends AbstractTransformingChannelHandler {
    private final Function<String, State> toState;
    public GenericChannelHandler(Function<String, State> toState, Consumer<State> updateState,
            Consumer<Command> postCommand, @Nullable Consumer<String> sendValue,
            ChannelTransformation stateTransformations, ChannelTransformation commandTransformations,
            ChannelValueConverterConfig channelConfig) {
        this.toState = toState;
    protected Optional<State> toState(String value) {
            return Optional.of(toState.apply(value));
            return Optional.of(UnDefType.UNDEF);
    protected String toString(Command command) {
        return command.toString();
