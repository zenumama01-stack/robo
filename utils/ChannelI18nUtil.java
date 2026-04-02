 * A utility service which localises {@link ChannelDefinition}.
 * Falls back to a localised {@link ChannelType} for label and description when not given otherwise.
 * @see org.openhab.core.thing.i18n.ChannelGroupTypeI18nLocalizationService ChannelGroupTypeI18nLocalizationService
public class ChannelI18nUtil {
     * @param channelTypeI18nLocalizationService an instance of {@link ChannelTypeI18nLocalizationService}.
     * @param channelTypeRegistry the {@link ChannelTypeRegistry}.
    public ChannelI18nUtil(ChannelTypeI18nLocalizationService channelTypeI18nLocalizationService,
            ChannelTypeRegistry channelTypeRegistry) {
    public List<ChannelDefinition> createLocalizedChannelDefinitions(final Bundle bundle,
            final List<ChannelDefinition> channelDefinitions,
            final Function<ChannelDefinition, @Nullable String> channelLabelResolver,
            final Function<ChannelDefinition, @Nullable String> channelDescriptionResolver,
        List<ChannelDefinition> localizedChannelDefinitions = new ArrayList<>(channelDefinitions.size());
        for (final ChannelDefinition channelDefinition : channelDefinitions) {
            final ChannelDefinitionBuilder builder = new ChannelDefinitionBuilder(channelDefinition);
            String channelLabel = channelLabelResolver.apply(channelDefinition);
            String channelDescription = channelDescriptionResolver.apply(channelDefinition);
            if (channelLabel == null || channelDescription == null) {
                ChannelTypeUID channelTypeUID = channelDefinition.getChannelTypeUID();
                ChannelType channelType = channelTypeRegistry.getChannelType(channelTypeUID, locale);
                    ChannelType localizedChannelType = channelTypeI18nLocalizationService
                            .createLocalizedChannelType(bundle, channelType, locale);
                    if (channelLabel == null) {
                        channelLabel = localizedChannelType.getLabel();
                    if (channelDescription == null) {
                        channelDescription = localizedChannelType.getDescription();
            builder.withLabel(channelLabel);
            builder.withDescription(channelDescription);
            localizedChannelDefinitions.add(builder.build());
        return localizedChannelDefinitions;
