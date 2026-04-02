 * Implementations may provide {@link Channel} specific {@link CommandDescription}s. Therefore the provider must be
 * registered as OSGi service.
public interface DynamicCommandDescriptionProvider {
     * For a given {@link Channel}, return a {@link CommandDescription} that should be used for the channel, instead of
     * the one defined statically in the {@link ChannelType}.
     * For a particular channel, there should be only one provider of the dynamic command description. When more than
     * one description is provided for the same channel (by different providers), only one will be used, from the
     * provider that registered first.
     * If the given channel will not be managed by the provider null should be returned. You never must return the
     * original command description in such case.
     * @param channel channel
     * @param originalCommandDescription original command description retrieved from the channel type
     *            this is the description to be replaced by the provided one
     * @return command description or null if none provided
    CommandDescription getCommandDescription(Channel channel, @Nullable CommandDescription originalCommandDescription,
