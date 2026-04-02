import java.util.concurrent.Future;
import org.openhab.core.addon.AddonI18nLocalizationService;
import org.openhab.core.config.core.xml.AbstractXmlBasedProvider;
import org.openhab.core.config.core.xml.osgi.XmlDocumentBundleTracker;
import org.openhab.core.config.core.xml.osgi.XmlDocumentProviderFactory;
import org.openhab.core.service.ReadyService;
import org.osgi.service.component.ComponentContext;
 * The {@link XmlAddonInfoProvider} is a concrete implementation of the {@link AddonInfoProvider} service interface.
 * This implementation manages any {@link AddonInfo} objects associated to specific modules. If a specific module
 * disappears, any registered {@link AddonInfo} objects associated with that module are released.
 * @author Michael Grammling - Refactoring: Provider/Registry pattern is used, added locale support
 * @author Simon Kaufmann - factored out common aspects into {@link AbstractXmlBasedProvider}
@Component
public class XmlAddonInfoProvider extends AbstractXmlBasedProvider<String, AddonInfo>
        implements AddonInfoProvider, XmlDocumentProviderFactory<AddonInfoXmlResult> {
    private static final String XML_DIRECTORY = "/OH-INF/addon/";
    public static final String READY_MARKER = "openhab.xmlAddonInfo";
    private final AddonI18nLocalizationService addonI18nService;
    private final XmlDocumentBundleTracker<AddonInfoXmlResult> addonInfoTracker;
    private final Future<?> trackerJob;
    public XmlAddonInfoProvider(final @Reference AddonI18nLocalizationService addonI18nService,
            final @Reference(target = "(openhab.scope=core.xml.addon)") ConfigDescriptionProvider configDescriptionProvider,
            final @Reference ReadyService readyService, ComponentContext componentContext) {
        this.addonI18nService = addonI18nService;
        this.configDescriptionProvider = (AbstractXmlConfigDescriptionProvider) configDescriptionProvider;
        XmlDocumentReader<AddonInfoXmlResult> addonInfoReader = new AddonInfoReader();
        addonInfoTracker = new XmlDocumentBundleTracker<>(componentContext.getBundleContext(), XML_DIRECTORY,
                addonInfoReader, this, READY_MARKER, readyService);
        ScheduledExecutorService scheduler = ThreadPoolManager
                .getScheduledPool(XmlDocumentBundleTracker.THREAD_POOL_NAME);
        trackerJob = scheduler.submit(addonInfoTracker::open);
        trackerJob.cancel(true);
        addonInfoTracker.close();
    public synchronized @Nullable AddonInfo getAddonInfo(@Nullable String id, @Nullable Locale locale) {
        return id == null ? null : get(id, locale);
    public synchronized Set<AddonInfo> getAddonInfos(@Nullable Locale locale) {
        return new HashSet<>(getAll(locale));
    protected @Nullable AddonInfo localize(Bundle bundle, AddonInfo bindingInfo, @Nullable Locale locale) {
        return addonI18nService.createLocalizedAddonInfo(bundle, bindingInfo, locale);
    public XmlDocumentProvider<AddonInfoXmlResult> createDocumentProvider(Bundle bundle) {
        return new AddonInfoXmlProvider(bundle, this, configDescriptionProvider);
