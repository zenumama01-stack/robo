package org.openhab.core.thing.internal.binding.generic.converter;
import org.openhab.core.thing.binding.generic.ChannelMode;
 * The {@link AbstractTransformingChannelHandler} is a base class for an item converter with transformations
public abstract class AbstractTransformingChannelHandler implements ChannelHandler {
    private final Consumer<Command> postCommand;
    private final @Nullable Consumer<String> sendValue;
    private final ChannelTransformation stateTransformations;
    private final ChannelTransformation commandTransformations;
    protected final ChannelValueConverterConfig channelConfig;
    protected AbstractTransformingChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
        this.postCommand = postCommand;
        this.sendValue = sendValue;
        this.stateTransformations = stateTransformations;
        this.commandTransformations = commandTransformations;
        this.channelConfig = channelConfig;
        if (channelConfig.mode != ChannelMode.WRITEONLY) {
            stateTransformations.apply(content.getAsString()).ifPresent(transformedValue -> {
                Command command = toCommand(transformedValue);
                    postCommand.accept(command);
                    toState(transformedValue).ifPresent(updateState);
            throw new IllegalStateException("Write-only channel");
        Consumer<String> sendValue = this.sendValue;
        if (sendValue != null && channelConfig.mode != ChannelMode.READONLY) {
            commandTransformations.apply(toString(command)).ifPresent(sendValue);
     * check if this converter received a value that needs to be sent as command
     * @param value the value
     * @return the command or null
    protected abstract @Nullable Command toCommand(String value);
     * convert the received value to a state
     * @return the state that represents the value of UNDEF if conversion failed
    protected abstract Optional<State> toState(String value);
     * convert a command to a string
     * @param command the command
     * @return the string representation of the command
    protected abstract String toString(Command command);
