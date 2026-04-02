import org.openhab.core.thing.i18n.ThingTypeI18nLocalizationService;
 * The {@link XmlThingTypeProvider} is a concrete implementation of the {@link ThingTypeProvider} service interface.
 * This implementation manages any {@link ThingType} objects associated to specific modules. If a specific module
 * disappears, any registered {@link ThingType} objects associated with that module are released.
 * @author Dennis Nobel - Added locale support, Added cache for localized thing types
@Component(property = { "openhab.scope=core.xml.thing" })
public class XmlThingTypeProvider extends AbstractXmlBasedProvider<UID, ThingType>
        implements ThingTypeProvider, XmlDocumentProviderFactory<List<?>> {
    private static final String XML_DIRECTORY = "/OH-INF/thing/";
    public static final String READY_MARKER = "openhab.xmlThingTypes";
    private final ThingTypeI18nLocalizationService thingTypeI18nLocalizationService;
    private @Nullable XmlDocumentBundleTracker<List<?>> thingTypeTracker;
    public XmlThingTypeProvider(
            final @Reference(target = "(openhab.scope=core.xml.channelGroups)") ChannelGroupTypeProvider channelGroupTypeProvider,
            final @Reference(target = "(openhab.scope=core.xml.channels)") ChannelTypeProvider channelTypeProvider,
            final @Reference(target = "(openhab.scope=core.xml.thing)") ConfigDescriptionProvider configDescriptionProvider,
            final @Reference ThingTypeI18nLocalizationService thingTypeI18nLocalizationService) {
        this.channelGroupTypeProvider = (XmlChannelGroupTypeProvider) channelGroupTypeProvider;
        this.channelTypeProvider = (XmlChannelTypeProvider) channelTypeProvider;
        this.thingTypeI18nLocalizationService = thingTypeI18nLocalizationService;
        XmlDocumentReader<List<?>> thingTypeReader = new ThingDescriptionReader();
        thingTypeTracker = new XmlDocumentBundleTracker<>(bundleContext, XML_DIRECTORY, thingTypeReader, this,
                READY_MARKER, readyService);
            thingTypeTracker.open();
        XmlDocumentBundleTracker<List<?>> localThingTypeTracker = thingTypeTracker;
        if (localThingTypeTracker != null) {
            localThingTypeTracker.close();
            thingTypeTracker = null;
        return get(thingTypeUID, locale);
    public synchronized Collection<ThingType> getThingTypes(@Nullable Locale locale) {
    protected @Nullable ThingType localize(Bundle bundle, ThingType thingType, @Nullable Locale locale) {
        return thingTypeI18nLocalizationService.createLocalizedThingType(bundle, thingType, locale);
    public XmlDocumentProvider<List<?>> createDocumentProvider(Bundle bundle) {
        return new ThingTypeXmlProvider(bundle, configDescriptionProvider, this, channelTypeProvider,
                channelGroupTypeProvider);
