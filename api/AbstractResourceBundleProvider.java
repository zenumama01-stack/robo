package org.openhab.core.automation.internal.provider;
import java.util.Enumeration;
import org.osgi.util.tracker.BundleTrackerCustomizer;
 * This class is base for {@link ModuleTypeProvider}, {@link TemplateProvider} and {@code RuleImporter} which are
 * responsible for importing and persisting the {@link ModuleType}s, {@link RuleTemplate}s and {@link Rule}s from
 * bundles which provides resource files.
 * It tracks {@link Parser} services by implementing {@link #addParser(Parser, Map)} and
 * {@link #removeParser(Parser, Map)} methods.
 * The functionality, responsible for tracking the bundles with resources, comes from
 * {@link AutomationResourceBundlesTracker} by implementing a {@link BundleTrackerCustomizer} but the functionality for
 * processing them, comes from this class.
public abstract class AbstractResourceBundleProvider<@NonNull E> {
    protected AbstractResourceBundleProvider(String path) {
        this.path = path;
     * This static field provides a root directory for automation object resources in the bundle resources.
     * It is common for all resources - {@link ModuleType}s, {@link RuleTemplate}s and {@link Rule}s.
    protected static final String ROOT_DIRECTORY = "OH-INF/automation";
    protected @Nullable ConfigI18nLocalizationService configI18nService;
     * This field keeps instance of {@link Logger} that is used for logging.
    protected final Logger logger = LoggerFactory.getLogger(AbstractResourceBundleProvider.class);
    protected @NonNullByDefault({}) BundleContext bundleContext;
     * This field is initialized in constructors of any particular provider with specific path for the particular
     * resources from specific type as {@link ModuleType}s, {@link RuleTemplate}s and {@link Rule}s:
     * <li>for
     * {@link ModuleType}s it is a "OH-INF/automation/moduletypes/"
     * <li>for {@link RuleTemplate}s it is a
     * "OH-INF/automation/templates/"
     * <li>for {@link Rule}s it is a "OH-INF/automation/rules/"
    protected final String path;
     * This Map collects all binded {@link Parser}s.
    protected final Map<String, Parser<E>> parsers = new ConcurrentHashMap<>();
     * The Map has for keys UIDs of the objects and for values one of {@link ModuleType}s, {@link RuleTemplate}s and
     * {@link Rule}s.
    protected final Map<String, E> providedObjectsHolder = new ConcurrentHashMap<>();
     * The Map has for keys - {@link Vendor}s and for values - Lists with UIDs of the objects.
    protected final Map<Vendor, List<String>> providerPortfolio = new ConcurrentHashMap<>();
     * This Map holds bundles whose {@link Parser} for resources is missing in the moment of processing the bundle.
     * Later, if the {@link Parser} appears, they will be added again in the {@link #queue} for processing.
    protected final Map<Bundle, List<URL>> waitingProviders = new ConcurrentHashMap<>();
     * This field provides an access to the queue for processing bundles.
    protected final AutomationResourceBundlesEventQueue queue = new AutomationResourceBundlesEventQueue<>(this);
    protected void activate(@Nullable BundleContext bundleContext) {
        bundleContext = null;
        queue.stop();
        synchronized (parsers) {
        synchronized (waitingProviders) {
            waitingProviders.clear();
    protected AutomationResourceBundlesEventQueue getQueue() {
     * This method is called before the {@link Parser} services to be added to the {@code ServiceTracker} and storing
     * them in the {@link #parsers} into the memory, for fast access on demand. The returned service object is stored in
     * the {@code ServiceTracker} and is available from the {@code getService} and {@code getServices} methods.
     * Also if there are bundles that were stored in {@link #waitingProviders}, to be processed later, because of
     * missing {@link Parser} for particular format,
     * and then the {@link Parser} service appears, they will be processed.
     * @param parser {@link Parser} service
     * @param properties of the service that has been added.
    protected void addParser(Parser<E> parser, Map<String, String> properties) {
        for (Bundle bundle : waitingProviders.keySet()) {
            if (bundle.getState() != Bundle.UNINSTALLED) {
                processAutomationProvider(bundle);
     * This method is called after a service is no longer being tracked by the {@code ServiceTracker} and removes the
     * {@link Parser} service objects from the structure Map "{@link #parsers}".
     * @param parser The {@link Parser} service object for the specified referenced service.
     * @param properties of the service that has been removed.
    protected void removeParser(Parser<E> parser, Map<String, String> properties) {
     * This method provides common functionality for {@link ModuleTypeProvider} and {@link TemplateProvider} to process
     * the bundles. For {@link RuleResourceBundleImporter} this method is overridden.
     * Checks for availability of the needed {@link Parser}. If it is not available - the bundle is added into
     * {@link #waitingProviders} and the execution of the method ends.
     * If it is available, the execution of the method continues with checking if the version of the bundle is changed.
     * If the version is changed - removes persistence of old variants of the objects, provided by this bundle.
     * Continues with loading the new version of these objects. If this bundle is added for the very first time, only
     * loads the provided objects.
     * The loading can fail because of {@link IOException}.
     * @param bundle it is a {@link Bundle} which has to be processed, because it provides resources for automation
     *            objects.
    protected void processAutomationProvider(Bundle bundle) {
        Enumeration<URL> urlEnum = null;
                urlEnum = bundle.findEntries(path, null, true);
            logger.debug("Can't read from resource of bundle with ID {}. The bundle is uninstalled.",
                    bundle.getBundleId(), e);
            processAutomationProviderUninstalled(bundle);
        String bsn = bundle.getSymbolicName();
        if (bsn == null) {
            bsn = String.format("@bundleId@0x%x", bundle.getBundleId());
        Vendor vendor = new Vendor(bsn, bundle.getVersion().toString());
        List<String> previousPortfolio = getPreviousPortfolio(vendor);
        List<String> newPortfolio = new LinkedList<>();
        if (urlEnum != null) {
            while (urlEnum.hasMoreElements()) {
                URL url = urlEnum.nextElement();
                if (url.getPath().endsWith(File.separator)) {
                String parserType = getParserType(url);
                updateWaitingProviders(parser, bundle, url);
                    Set<E> parsedObjects = parseData(parser, url, bundle);
                    if (!parsedObjects.isEmpty()) {
                        addNewProvidedObjects(newPortfolio, previousPortfolio, parsedObjects);
            putNewPortfolio(vendor, newPortfolio);
        removeUninstalledObjects(previousPortfolio, newPortfolio);
    protected void removeUninstalledObjects(List<String> previousPortfolio, List<String> newPortfolio) {
        for (String uid : previousPortfolio) {
            if (!newPortfolio.contains(uid)) {
                final @Nullable E removedObject = providedObjectsHolder.remove(uid);
                    List<ProviderChangeListener<E>> snapshot;
                        snapshot = new LinkedList<>(listeners);
                    for (ProviderChangeListener<E> listener : snapshot) {
                        listener.removed((Provider<E>) this, removedObject);
    protected List<String> getPreviousPortfolio(Vendor vendor) {
        List<String> portfolio = providerPortfolio.remove(vendor);
        if (portfolio == null) {
            for (Vendor v : providerPortfolio.keySet()) {
                if (v.getVendorSymbolicName().equals(vendor.getVendorSymbolicName())) {
                    List<String> vendorPortfolio = providerPortfolio.remove(v);
                    return vendorPortfolio == null ? List.of() : vendorPortfolio;
        return portfolio == null ? List.of() : portfolio;
    protected void putNewPortfolio(Vendor vendor, List<String> portfolio) {
        providerPortfolio.put(vendor, portfolio);
     * This method is used to determine which parser to be used.
     * @param url the URL of the source of data for parsing.
     * @return the type of the parser.
    protected String getParserType(URL url) {
        String fileName = url.getPath();
        String extension = index != -1 ? fileName.substring(index + 1) : "";
        return extension.isEmpty() || "txt".equals(extension) ? Parser.FORMAT_JSON : extension;
     * uninstalling the bundles. For {@link RuleResourceBundleImporter} this method is overridden.
     * When some of the bundles that provides automation objects is uninstalled, this method will remove it from
     * {@link #waitingProviders}, if it is still there or from {@link #providerPortfolio} in the other case.
     * Will remove the provided objects from {@link #providedObjectsHolder} and will remove their persistence, injected
     * in the system from this bundle.
     * @param bundle the uninstalled {@link Bundle}, provider of automation objects.
    protected void processAutomationProviderUninstalled(Bundle bundle) {
        waitingProviders.remove(bundle);
     * This method is used to get the bundle providing localization resources for {@link ModuleType}s or
     * {@link Template}s.
     * @param uid is the unique identifier of {@link ModuleType} or {@link Template} that must be localized.
     * @return the bundle providing localization resources.
    protected @Nullable Bundle getBundle(String uid) {
        String symbolicName = null;
        for (Entry<Vendor, List<String>> entry : providerPortfolio.entrySet()) {
            if (entry.getValue().contains(uid)) {
                symbolicName = entry.getKey().getVendorSymbolicName();
        if (symbolicName != null) {
            Bundle[] bundles = bundleContext.getBundles();
            for (Bundle bundle : bundles) {
                if (symbolicName.equals(bundle.getSymbolicName())) {
    protected List<ConfigDescriptionParameter> getLocalizedConfigurationDescription(
            @Nullable List<ConfigDescriptionParameter> config, Bundle bundle, String uid, String prefix,
        ConfigI18nLocalizationService localConfigI18nService = configI18nService;
        if (config != null && localConfigI18nService != null) {
                URI uri = new URI(prefix + ":" + uid + ".name");
                return config.stream()
                        .map(p -> localConfigI18nService.getLocalizedConfigDescriptionParameter(bundle, uri, p, locale))
            } catch (URISyntaxException e) {
                logger.error("Constructed invalid uri '{}:{}.name'", prefix, uid, e);
     * This method is called from {@link #processAutomationProvider(Bundle)} to process the loading of the provided
     * @param parser the {@link Parser} which is responsible for parsing of a particular format in which the provided
     *            objects are presented
     * @param url the resource which is used for loading the objects.
     * @param bundle is the resource holder
     * @return a set of automation objects - the result of loading.
    protected Set<E> parseData(Parser<E> parser, URL url, Bundle bundle) {
        InputStreamReader reader = null;
        InputStream is = null;
            is = url.openStream();
            reader = new InputStreamReader(is, StandardCharsets.UTF_8);
            return parser.parse(reader);
        } catch (ParsingException e) {
            logger.error("{}", e.getLocalizedMessage(), e);
            logger.error("Can't read from resource of bundle with ID {}", bundle.getBundleId(), e);
            if (reader != null) {
                } catch (IOException ignore) {
            if (is != null) {
                    is.close();
    protected void addNewProvidedObjects(List<String> newPortfolio, List<String> previousPortfolio,
            Set<E> parsedObjects) {
        for (E parsedObject : parsedObjects) {
            String uid = getUID(parsedObject);
            final @Nullable E oldElement = providedObjectsHolder.get(uid);
            if (oldElement != null && !previousPortfolio.contains(uid)) {
                logger.warn("{} with UID '{}' already exists! Failed to add a second with the same UID!",
                        parsedObject.getClass().getName(), uid);
                newPortfolio.add(uid);
                providedObjectsHolder.put(uid, parsedObject);
                    if (oldElement == null) {
                        listener.added((Provider<E>) this, parsedObject);
                        listener.updated((Provider<E>) this, oldElement, parsedObject);
    protected void updateWaitingProviders(@Nullable Parser<E> parser, Bundle bundle, URL url) {
        List<URL> urlList = waitingProviders.get(bundle);
            if (urlList == null) {
                urlList = new ArrayList<>();
            urlList.add(url);
            waitingProviders.put(bundle, urlList);
        if (urlList != null && urlList.remove(url) && urlList.isEmpty()) {
     * @param parsedObject
    protected abstract String getUID(E parsedObject);
