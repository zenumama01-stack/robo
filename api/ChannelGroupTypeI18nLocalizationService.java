package org.openhab.core.thing.i18n;
import org.openhab.core.thing.internal.i18n.ChannelI18nUtil;
import org.openhab.core.thing.internal.i18n.ThingTypeI18nUtil;
 * This OSGi service could be used to localize a {@link ChannelGroupType} type using the I18N mechanism of the openHAB
 * @author Markus Rathgeb - Move code from XML thing type provider to separate service
 * @author Laurent Garnier - fix localized label and description for channel group definition
 * @author Christoph Weitkamp - factored out from {@link org.openhab.core.thing.xml.internal.XmlChannelTypeProvider} and
 *         {@link org.openhab.core.thing.xml.internal.XmlChannelGroupTypeProvider}
 * @author Henning Treu - factored out from {@link ThingTypeI18nLocalizationService}
 * @author Christoph Weitkamp - Removed "advanced" attribute
@Component(service = ChannelGroupTypeI18nLocalizationService.class)
public class ChannelGroupTypeI18nLocalizationService {
    private final ThingTypeI18nUtil thingTypeI18nUtil;
    private final ChannelI18nUtil channelI18nUtil;
    public ChannelGroupTypeI18nLocalizationService(final @Reference TranslationProvider i18nProvider,
            final @Reference ChannelTypeRegistry channelTypeRegistry) {
        this.thingTypeI18nUtil = new ThingTypeI18nUtil(i18nProvider);
        this.channelI18nUtil = new ChannelI18nUtil(channelTypeI18nLocalizationService, channelTypeRegistry);
    public ChannelGroupType createLocalizedChannelGroupType(Bundle bundle, ChannelGroupType channelGroupType,
        ChannelGroupTypeUID channelGroupTypeUID = channelGroupType.getUID();
        String defaultLabel = channelGroupType.getLabel();
        String label = thingTypeI18nUtil.getChannelGroupLabel(bundle, channelGroupTypeUID, defaultLabel, locale);
        String description = thingTypeI18nUtil.getChannelGroupDescription(bundle, channelGroupTypeUID,
                channelGroupType.getDescription(), locale);
        List<ChannelDefinition> localizedChannelDefinitions = channelI18nUtil.createLocalizedChannelDefinitions(bundle,
                channelGroupType.getChannelDefinitions(),
                channelDefinition -> thingTypeI18nUtil.getChannelLabel(bundle, channelGroupTypeUID, channelDefinition,
                        channelDefinition.getLabel(), locale),
                channelDefinition -> thingTypeI18nUtil.getChannelDescription(bundle, channelGroupTypeUID,
                        channelDefinition, channelDefinition.getDescription(), locale),
        ChannelGroupTypeBuilder builder = ChannelGroupTypeBuilder
                .instance(channelGroupTypeUID, label == null ? defaultLabel : label)
                .withChannelDefinitions(localizedChannelDefinitions);
        String category = channelGroupType.getCategory();
        if (category != null) {
            builder.withCategory(category);
