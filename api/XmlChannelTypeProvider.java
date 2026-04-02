 * {@link XmlChannelTypeProvider} provides channel types from XML files.
 * @author Kai Kreuzer - fixed concurrency issues
 * @author Henning Treu - QuantityType implementation
@Component(property = { "openhab.scope=core.xml.channels" })
public class XmlChannelTypeProvider extends AbstractXmlBasedProvider<UID, ChannelType> implements ChannelTypeProvider {
    public XmlChannelTypeProvider(
            final @Reference ChannelTypeI18nLocalizationService channelTypeI18nLocalizationService) {
        return get(channelTypeUID, locale);
    public synchronized Collection<ChannelType> getChannelTypes(@Nullable Locale locale) {
    protected @Nullable ChannelType localize(Bundle bundle, ChannelType channelType, @Nullable Locale locale) {
        return channelTypeI18nLocalizationService.createLocalizedChannelType(bundle, channelType, locale);
