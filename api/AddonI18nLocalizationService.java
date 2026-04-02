import org.openhab.core.addon.internal.AddonI18nUtil;
import org.openhab.core.i18n.TranslationProvider;
 * This OSGi service could be used to localize the add-on info using the I18N mechanism of the openHAB
 * framework.
 * @author Christoph Weitkamp - Initial contribution
@Component(immediate = true, service = { AddonI18nLocalizationService.class })
public class AddonI18nLocalizationService {
    private final AddonI18nUtil addonI18NUtil;
    public AddonI18nLocalizationService(final @Reference TranslationProvider i18nProvider) {
        this.addonI18NUtil = new AddonI18nUtil(i18nProvider);
     * Localizes an add-on info.
     * @param bundle the bundle the i18n resources are located
     * @param addonInfo the add-on info that should be localized
     * @param locale the locale it should be localized to
     * @return a localized add-on info on success, a non-localized one on error (e.g. no translation is found).
    public AddonInfo createLocalizedAddonInfo(Bundle bundle, AddonInfo addonInfo, @Nullable Locale locale) {
        String addonInfoUID = addonInfo.getId();
        String name = addonI18NUtil.getName(bundle, addonInfoUID, addonInfo.getName(), locale);
        String description = addonI18NUtil.getDescription(bundle, addonInfoUID, addonInfo.getDescription(), locale);
        return AddonInfo.builder(addonInfo).withName(name).withDescription(description)
                .withSourceBundle(addonInfo.getSourceBundle()).build();
