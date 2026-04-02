 * Provides {@link ConfigDescription}s for things which are read from XML files.
@Component(service = ConfigDescriptionProvider.class, immediate = true, property = { "openhab.scope=core.xml.thing" })
public class ThingXmlConfigDescriptionProvider extends AbstractXmlConfigDescriptionProvider {
    public ThingXmlConfigDescriptionProvider(final @Reference ConfigI18nLocalizationService configI18nService) {
