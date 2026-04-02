 * The {@link ChannelGroupTypeRegistry} tracks all {@link ChannelGroupType}s provided by
 * registered {@link ChannelGroupTypeProvider}s.
@Component(service = ChannelGroupTypeRegistry.class)
public class ChannelGroupTypeRegistry {
    private final List<ChannelGroupTypeProvider> channelGroupTypeProviders = new CopyOnWriteArrayList<>();
     * Returns all channel group types with the default {@link Locale}.
     * @return all channel group types or empty list if no channel group type exists
    public List<ChannelGroupType> getChannelGroupTypes() {
        return getChannelGroupTypes(null);
     * Returns all channel group types for the given {@link Locale}.
    public List<ChannelGroupType> getChannelGroupTypes(@Nullable Locale locale) {
        List<ChannelGroupType> channelGroupTypes = new ArrayList<>();
        for (ChannelGroupTypeProvider channelTypeProvider : channelGroupTypeProviders) {
            channelGroupTypes.addAll(channelTypeProvider.getChannelGroupTypes(locale));
        return Collections.unmodifiableList(channelGroupTypes);
     * Returns the channel group type for the given UID with the default {@link Locale}.
     * @return channel group type or null if no channel group type for the given UID exists
    public @Nullable ChannelGroupType getChannelGroupType(@Nullable ChannelGroupTypeUID channelGroupTypeUID) {
        return getChannelGroupType(channelGroupTypeUID, null);
     * Returns the channel group type for the given UID and the given {@link Locale}.
    public @Nullable ChannelGroupType getChannelGroupType(@Nullable ChannelGroupTypeUID channelGroupTypeUID,
        if (channelGroupTypeUID == null) {
            ChannelGroupType channelGroupType = channelTypeProvider.getChannelGroupType(channelGroupTypeUID, locale);
                return channelGroupType;
    protected void addChannelGroupTypeProvider(ChannelGroupTypeProvider channelGroupTypeProvider) {
        this.channelGroupTypeProviders.add(channelGroupTypeProvider);
    protected void removeChannelGroupTypeProvider(ChannelGroupTypeProvider channelGroupTypeProvider) {
        this.channelGroupTypeProviders.remove(channelGroupTypeProvider);
