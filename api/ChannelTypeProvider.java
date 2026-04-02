 * The {@link ChannelTypeProvider} is responsible for providing channel types.
 * @see ChannelTypeRegistry
public interface ChannelTypeProvider {
     * @see ChannelTypeRegistry#getChannelTypes(Locale)
    Collection<ChannelType> getChannelTypes(@Nullable Locale locale);
     * @see ChannelTypeRegistry#getChannelType(ChannelTypeUID, Locale)
    ChannelType getChannelType(ChannelTypeUID channelTypeUID, @Nullable Locale locale);
