 * The {@link ChannelGroupTypeProvider} is responsible for providing channel group types.
 * @see ChannelGroupTypeRegistry
public interface ChannelGroupTypeProvider {
     * @see ChannelGroupTypeRegistry#getChannelGroupType(ChannelGroupTypeUID, Locale)
    ChannelGroupType getChannelGroupType(ChannelGroupTypeUID channelGroupTypeUID, @Nullable Locale locale);
     * @see ChannelGroupTypeRegistry#getChannelGroupTypes(Locale)
    Collection<ChannelGroupType> getChannelGroupTypes(@Nullable Locale locale);
