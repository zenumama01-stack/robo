 * The {@link ChannelTypeRegistry} tracks all {@link ChannelType}s provided by registered {@link ChannelTypeProvider}s.
@Component(service = ChannelTypeRegistry.class)
public class ChannelTypeRegistry {
    private final List<ChannelTypeProvider> channelTypeProviders = new CopyOnWriteArrayList<>();
     * Returns all channel types with the default {@link Locale}.
     * @return all channel types or empty list if no channel type exists
    public List<ChannelType> getChannelTypes() {
        return getChannelTypes(null);
     * Returns all channel types for the given {@link Locale}.
    public List<ChannelType> getChannelTypes(@Nullable Locale locale) {
        List<ChannelType> channelTypes = new ArrayList<>();
        for (ChannelTypeProvider channelTypeProvider : channelTypeProviders) {
            channelTypes.addAll(channelTypeProvider.getChannelTypes(locale));
        return Collections.unmodifiableList(channelTypes);
     * Returns the channel type for the given UID with the default {@link Locale}.
     * @return channel type or null if no channel type for the given UID exists
    public @Nullable ChannelType getChannelType(@Nullable ChannelTypeUID channelTypeUID) {
        return getChannelType(channelTypeUID, null);
     * Returns the channel type for the given UID and the given {@link Locale}.
    public @Nullable ChannelType getChannelType(@Nullable ChannelTypeUID channelTypeUID, @Nullable Locale locale) {
            ChannelType channelType = channelTypeProvider.getChannelType(channelTypeUID, locale);
                return channelType;
    protected void addChannelTypeProvider(ChannelTypeProvider channelTypeProviders) {
        this.channelTypeProviders.add(channelTypeProviders);
    protected void removeChannelTypeProvider(ChannelTypeProvider channelTypeProviders) {
        this.channelTypeProviders.remove(channelTypeProviders);
