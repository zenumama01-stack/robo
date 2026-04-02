import org.openhab.core.config.core.ConfigDescriptionProvider;
import org.openhab.core.config.core.i18n.ConfigI18nLocalizationService;
 * Provides {@link org.openhab.core.config.core.ConfigDescription}s for bindings which are read from XML files.
 * @author Simon Kaufmann - Initial contribution
@Component(service = ConfigDescriptionProvider.class, immediate = true, property = { "openhab.scope=core.xml.addon" })
public class AddonXmlConfigDescriptionProvider extends AbstractXmlConfigDescriptionProvider {
    private final ConfigI18nLocalizationService configI18nService;
    public AddonXmlConfigDescriptionProvider(final @Reference ConfigI18nLocalizationService configI18nService) {
        this.configI18nService = configI18nService;
    protected ConfigI18nLocalizationService getConfigI18nLocalizerService() {
        return configI18nService;
