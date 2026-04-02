import org.openhab.core.types.CommandDescriptionBuilder;
 * The {@link BaseDynamicCommandDescriptionProvider} provides a base implementation for the
 * {@link DynamicCommandDescriptionProvider}.
 * It provides localized dynamic {@link CommandOption}s. Therefore the inheriting class has to request a reference for
 * the {@link ChannelTypeI18nLocalizationService} on its own.
 * @author Christoph Weitkamp - Added ChannelStateDescriptionChangedEvent
public abstract class BaseDynamicCommandDescriptionProvider extends AbstractDynamicDescriptionProvider
        implements DynamicCommandDescriptionProvider {
    protected final Map<ChannelUID, List<CommandOption>> channelOptionsMap = new ConcurrentHashMap<>();
     * For a given {@link ChannelUID}, set a {@link List} of {@link CommandOption}s that should be used for the channel,
     * instead of the one defined statically in the {@link ChannelType}.
     * @param channelUID the {@link ChannelUID} of the channel
     * @param options a {@link List} of {@link CommandOption}s
    public void setCommandOptions(ChannelUID channelUID, List<CommandOption> options) {
        List<CommandOption> oldOptions = channelOptionsMap.get(channelUID);
        if (!options.equals(oldOptions)) {
            channelOptionsMap.put(channelUID, options);
            postEvent(ThingEventFactory.createChannelDescriptionCommandOptionsChangedEvent(channelUID,
                    itemChannelLinkRegistry != null ? itemChannelLinkRegistry.getLinkedItemNames(channelUID) : Set.of(),
                    options, oldOptions));
    public @Nullable CommandDescription getCommandDescription(Channel channel,
            @Nullable CommandDescription originalCommandDescription, @Nullable Locale locale) {
        List<CommandOption> options = channelOptionsMap.get(channel.getUID());
        return CommandDescriptionBuilder.create().withCommandOptions(localizedCommandOptions(options, channel, locale))
     * Localizes a {@link List} of {@link CommandOption}s that should be used for the channel.
     * @param channel the channel
     * @param locale a locale
     * @return the localized {@link List} of {@link CommandOption}s
    protected List<CommandOption> localizedCommandOptions(List<CommandOption> options, Channel channel,
        // can be overridden by subclasses
        if (channelTypeI18nLocalizationService != null && channelTypeUID != null) {
            return channelTypeI18nLocalizationService.createLocalizedCommandOptions(bundleContext.getBundle(), options,
                    channelTypeUID, locale);
        channelOptionsMap.clear();
