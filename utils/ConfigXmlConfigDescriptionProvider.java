import org.openhab.core.config.core.xml.internal.ConfigDescriptionReader;
import org.openhab.core.config.core.xml.internal.ConfigDescriptionXmlProvider;
 * Provides {@link ConfigDescription}s for configurations which are read from XML files.
@Component(service = ConfigDescriptionProvider.class, immediate = true, property = { "openhab.scope=core.xml.config" })
public class ConfigXmlConfigDescriptionProvider extends AbstractXmlConfigDescriptionProvider
        implements XmlDocumentProviderFactory<List<ConfigDescription>> {
    private static final String XML_DIRECTORY = "/OH-INF/config/";
    public static final String READY_MARKER = "openhab.xmlConfig";
    private @Nullable XmlDocumentBundleTracker<List<ConfigDescription>> configDescriptionTracker;
    private @Nullable Future<?> trackerJob;
    public ConfigXmlConfigDescriptionProvider(final @Reference ConfigI18nLocalizationService configI18nService,
            final @Reference ReadyService readyService) {
    public void activate(ComponentContext componentContext) {
        XmlDocumentReader<List<ConfigDescription>> configDescriptionReader = new ConfigDescriptionReader();
        configDescriptionTracker = new XmlDocumentBundleTracker<>(componentContext.getBundleContext(), XML_DIRECTORY,
                configDescriptionReader, this, READY_MARKER, readyService);
        trackerJob = scheduler.submit(() -> {
            configDescriptionTracker.open();
        Future<?> localTrackerJob = trackerJob;
        if (localTrackerJob != null && !localTrackerJob.isDone()) {
            localTrackerJob.cancel(true);
            trackerJob = null;
        XmlDocumentBundleTracker<List<ConfigDescription>> localConfigDescriptionTracker = configDescriptionTracker;
        if (localConfigDescriptionTracker != null) {
            localConfigDescriptionTracker.close();
            configDescriptionTracker = null;
    public XmlDocumentProvider<List<ConfigDescription>> createDocumentProvider(Bundle bundle) {
        return new ConfigDescriptionXmlProvider(bundle, this);
