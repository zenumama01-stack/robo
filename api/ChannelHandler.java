package org.openhab.core.thing.binding.generic;
 * The {@link ChannelHandler} defines the interface for converting received {@link ChannelHandlerContent}
 * to {@link org.openhab.core.types.State}s for posting updates to {@link org.openhab.core.thing.Channel}s and
 * {@link Command}s to values for sending
public interface ChannelHandler {
     * called to process a given content for this channel
     * @param content raw content to process (<code>null</code> results in
     *            {@link org.openhab.core.types.UnDefType#UNDEF})
    void process(@Nullable ChannelHandlerContent content);
     * called to send a command to this channel
    void send(Command command);
    interface Factory {
        ChannelHandler create(Consumer<State> updateState, Consumer<Command> postCommand,
                @Nullable Consumer<String> sendValue, ChannelTransformation stateTransformations,
                ChannelTransformation commandTransformations, ChannelValueConverterConfig channelConfig);
