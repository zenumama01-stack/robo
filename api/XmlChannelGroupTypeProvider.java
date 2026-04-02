 * {@link XmlChannelGroupTypeProvider} provides channel group types from XML files.
 * @author Christoph Weitkamp - factored out common aspects into ThingTypeI18nLocalizationService
@Component(property = { "openhab.scope=core.xml.channelGroups" })
public class XmlChannelGroupTypeProvider extends AbstractXmlBasedProvider<UID, ChannelGroupType>
        implements ChannelGroupTypeProvider {
    public XmlChannelGroupTypeProvider(
            final @Reference ChannelGroupTypeI18nLocalizationService channelGroupTypeI18nLocalizationService) {
        return get(channelGroupTypeUID, locale);
    protected @Nullable ChannelGroupType localize(Bundle bundle, ChannelGroupType channelGroupType,
        return channelGroupTypeI18nLocalizationService.createLocalizedChannelGroupType(bundle, channelGroupType,
