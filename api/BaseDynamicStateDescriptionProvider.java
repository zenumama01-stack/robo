 * The {@link BaseDynamicStateDescriptionProvider} provides a base implementation for the
 * {@link DynamicStateDescriptionProvider}.
 * It provides localized patterns and dynamic {@link StateOption}s while leaving other {@link StateDescription} fields
 * as original. Therefore the inheriting class has to request a reference for the
 * {@link ChannelTypeI18nLocalizationService} on its own.
public abstract class BaseDynamicStateDescriptionProvider extends AbstractDynamicDescriptionProvider
        implements DynamicStateDescriptionProvider {
    protected final Map<ChannelUID, String> channelPatternMap = new ConcurrentHashMap<>();
    protected final Map<ChannelUID, List<StateOption>> channelOptionsMap = new ConcurrentHashMap<>();
     * For a given {@link ChannelUID}, set a pattern that should be used for the channel, instead of the one defined
     * statically in the {@link ChannelType}.
     * @param pattern a pattern
    public void setStatePattern(ChannelUID channelUID, String pattern) {
        String oldPattern = channelPatternMap.get(channelUID);
        if (!pattern.equals(oldPattern)) {
            channelPatternMap.put(channelUID, pattern);
            postEvent(ThingEventFactory.createChannelDescriptionPatternChangedEvent(channelUID,
                    pattern, oldPattern));
     * For a given {@link ChannelUID}, set a {@link List} of {@link StateOption}s that should be used for the channel,
     * @param options a {@link List} of {@link StateOption}s
    public void setStateOptions(ChannelUID channelUID, List<StateOption> options) {
        List<StateOption> oldOptions = channelOptionsMap.get(channelUID);
            postEvent(ThingEventFactory.createChannelDescriptionStateOptionsChangedEvent(channelUID,
    public @Nullable StateDescription getStateDescription(Channel channel, @Nullable StateDescription original,
        ChannelUID channelUID = channel.getUID();
        String pattern = channelPatternMap.get(channelUID);
        List<StateOption> options = channelOptionsMap.get(channelUID);
        if (pattern == null && options == null) {
        StateDescriptionFragmentBuilder builder = (original == null) ? StateDescriptionFragmentBuilder.create()
                : StateDescriptionFragmentBuilder.create(original);
            String localizedPattern = localizeStatePattern(pattern, channel, locale);
            if (localizedPattern != null) {
                builder.withPattern(localizedPattern);
            builder.withOptions(localizedStateOptions(options, channel, locale));
        return builder.build().toStateDescription();
     * Localizes a pattern that should be used for the channel.
     * @return the localized pattern
    protected @Nullable String localizeStatePattern(String pattern, Channel channel, @Nullable Locale locale) {
            return channelTypeI18nLocalizationService.createLocalizedStatePattern(bundleContext.getBundle(), pattern,
     * Localizes a {@link List} of {@link StateOption}s that should be used for the channel.
     * @return the localized {@link List} of {@link StateOption}s
    protected List<StateOption> localizedStateOptions(List<StateOption> options, Channel channel,
            return channelTypeI18nLocalizationService.createLocalizedStateOptions(bundleContext.getBundle(), options,
        channelPatternMap.clear();
