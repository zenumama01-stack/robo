import org.openhab.core.thing.internal.i18n.ChannelGroupI18nUtil;
 * This OSGi service could be used to localize a {@link ThingType} using the I18N mechanism of the openHAB
@Component(service = ThingTypeI18nLocalizationService.class)
public class ThingTypeI18nLocalizationService {
    private final ChannelGroupI18nUtil channelGroupI18nUtil;
    public ThingTypeI18nLocalizationService(final @Reference TranslationProvider i18nProvider,
            final @Reference ChannelGroupTypeI18nLocalizationService channelGroupTypeI18nLocalizationService,
        this.channelGroupI18nUtil = new ChannelGroupI18nUtil(channelGroupTypeI18nLocalizationService,
                channelGroupTypeRegistry);
    public ThingType createLocalizedThingType(Bundle bundle, ThingType thingType, @Nullable Locale locale) {
        ThingTypeUID thingTypeUID = thingType.getUID();
        String label = thingTypeI18nUtil.getLabel(bundle, thingTypeUID, thingType.getLabel(), locale);
        String description = thingTypeI18nUtil.getDescription(bundle, thingTypeUID, thingType.getDescription(), locale);
                thingType.getChannelDefinitions(),
                channelDefinition -> thingTypeI18nUtil.getChannelLabel(bundle, thingTypeUID, channelDefinition,
                channelDefinition -> thingTypeI18nUtil.getChannelDescription(bundle, thingTypeUID, channelDefinition,
                        channelDefinition.getDescription(), locale),
        List<ChannelGroupDefinition> localizedChannelGroupDefinitions = channelGroupI18nUtil
                .createLocalizedChannelGroupDefinitions(bundle, thingType.getChannelGroupDefinitions(),
                        channelGroupDefinition -> thingTypeI18nUtil.getChannelGroupLabel(bundle, thingTypeUID,
                                channelGroupDefinition, channelGroupDefinition.getLabel(), locale),
                        channelGroupDefinition -> thingTypeI18nUtil.getChannelGroupDescription(bundle, thingTypeUID,
                                channelGroupDefinition, channelGroupDefinition.getDescription(), locale),
        ThingTypeBuilder builder = ThingTypeBuilder.instance(thingType);
        builder.withChannelDefinitions(localizedChannelDefinitions)
                .withChannelGroupDefinitions(localizedChannelGroupDefinitions);
            return builder.buildBridge();
