package org.openhab.core.thing.internal.i18n;
import org.openhab.core.thing.i18n.ChannelGroupTypeI18nLocalizationService;
 * A utility service which localizes {@link ChannelGroupDefinition}.
 * Falls back to a localized {@link ChannelGroupType} for label and description when not given otherwise.
 * @see org.openhab.core.thing.i18n.ThingTypeI18nLocalizationService ThingTypeI18nLocalizationService
public class ChannelGroupI18nUtil {
    private final ChannelGroupTypeI18nLocalizationService channelGroupTypeI18nLocalizationService;
     * Create a new util instance and pass the appropriate dependencies.
     * @param channelGroupTypeI18nLocalizationService an instance of {@link ChannelGroupTypeI18nLocalizationService}.
     * @param channelGroupTypeRegistry the {@link ChannelGroupTypeRegistry}.
    public ChannelGroupI18nUtil(ChannelGroupTypeI18nLocalizationService channelGroupTypeI18nLocalizationService,
            ChannelGroupTypeRegistry channelGroupTypeRegistry) {
        this.channelGroupTypeI18nLocalizationService = channelGroupTypeI18nLocalizationService;
    public List<ChannelGroupDefinition> createLocalizedChannelGroupDefinitions(final Bundle bundle,
            final List<ChannelGroupDefinition> channelGroupDefinitions,
            final Function<ChannelGroupDefinition, @Nullable String> channelGroupLabelResolver,
            final Function<ChannelGroupDefinition, @Nullable String> channelGroupDescriptionResolver,
        List<ChannelGroupDefinition> localizedChannelGroupDefinitions = new ArrayList<>(channelGroupDefinitions.size());
        for (final ChannelGroupDefinition channelGroupDefinition : channelGroupDefinitions) {
            String channelGroupLabel = channelGroupLabelResolver.apply(channelGroupDefinition);
            String channelGroupDescription = channelGroupDescriptionResolver.apply(channelGroupDefinition);
            if (channelGroupLabel == null || channelGroupDescription == null) {
                ChannelGroupTypeUID channelGroupTypeUID = channelGroupDefinition.getTypeUID();
                ChannelGroupType channelGroupType = channelGroupTypeRegistry.getChannelGroupType(channelGroupTypeUID,
                    ChannelGroupType localizedChannelGroupType = channelGroupTypeI18nLocalizationService
                            .createLocalizedChannelGroupType(bundle, channelGroupType, locale);
                    if (channelGroupLabel == null) {
                        channelGroupLabel = localizedChannelGroupType.getLabel();
                    if (channelGroupDescription == null) {
                        channelGroupDescription = localizedChannelGroupType.getDescription();
            localizedChannelGroupDefinitions.add(new ChannelGroupDefinition(channelGroupDefinition.getId(),
                    channelGroupDefinition.getTypeUID(), channelGroupLabel, channelGroupDescription));
        return localizedChannelGroupDefinitions;
