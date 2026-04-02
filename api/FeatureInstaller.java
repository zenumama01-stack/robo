package org.openhab.core.karaf.internal;
import java.util.EnumSet;
import java.util.concurrent.LinkedBlockingQueue;
import org.apache.karaf.features.Feature;
import org.apache.karaf.features.FeaturesService;
 * This service reads addons.cfg and installs listed add-ons (= Karaf features) and the selected package.
 * It furthermore allows configuration of the base package through the UI as well as administrating Karaf to
 * access remote repos and certain feature repos like for experimental features.
@Component(name = "org.openhab.addons", service = { FeatureInstaller.class, ConfigurationListener.class })
@ConfigurableService(category = "system", label = "Add-on Management", description_uri = FeatureInstaller.CONFIG_URI)
public class FeatureInstaller implements ConfigurationListener {
    protected static final String CONFIG_URI = "system:addons";
    public static final String PREFIX = "openhab-";
    private static final String CFG_REMOTE = "remote";
    private static final String PAX_URL_PID = "org.ops4j.pax.url.mvn";
    private static final String ADDONS_PID = "org.openhab.addons";
    private static final String PROPERTY_MVN_REPOS = "org.ops4j.pax.url.mvn.repositories";
    public static final String FINDER_ADDON_TYPE = "core-config-discovery-addon";
    public static final List<String> ADDON_TYPES = Stream
            .concat(AddonType.DEFAULT_TYPES.stream().map(AddonType::getId), Stream.of(FINDER_ADDON_TYPE)).toList();
    private final Logger logger = LoggerFactory.getLogger(FeatureInstaller.class);
    private final FeaturesService featuresService;
    private final AtomicBoolean processingConfigQueue = new AtomicBoolean();
    private final LinkedBlockingQueue<Map<String, Object>> configQueue = new LinkedBlockingQueue<>();
    private @Nullable String onlineRepoUrl = null;
    private boolean paxCfgUpdated = true; // a flag used to check whether CM has already successfully updated the pax
                                          // configuration as this must be waited for before trying to add feature repos
    private @Nullable Map<String, Object> configMapCache;
    public FeatureInstaller(final @Reference ConfigurationAdmin configurationAdmin,
            final @Reference FeaturesService featuresService, final @Reference KarService karService,
            final @Reference EventPublisher eventPublisher, Map<String, Object> config) {
        this.featuresService = featuresService;
        scheduler = Executors.newSingleThreadScheduledExecutor(new NamedThreadFactory("karaf-addons"));
        setOnlineRepoUrl();
        scheduler.scheduleWithFixedDelay(this::syncConfiguration, 1, 1, TimeUnit.MINUTES);
    protected void modified(final Map<String, Object> config) {
        configQueue.add(config);
        if (processingConfigQueue.compareAndSet(false, true)) {
            scheduler.execute(this::processConfigQueue);
    private void syncConfiguration() {
        logger.debug("Running scheduled sync job");
            Dictionary<String, Object> cfg = configurationAdmin.getConfiguration(ADDONS_PID).getProperties();
            if (cfg == null) {
                logger.debug("Configuration has no properties yet. Skipping update.");
            final Map<String, Object> cfgMap = new HashMap<>();
            final Enumeration<String> enumeration = cfg.keys();
            while (enumeration.hasMoreElements()) {
                final String key = enumeration.nextElement();
                cfgMap.put(key, cfg.get(key));
            if (!cfgMap.equals(configMapCache) && !processingConfigQueue.get()) {
                modified(cfgMap);
            logger.debug("Failed to retrieve the addons configuration from configuration admin: {}", e.getMessage());
    private synchronized void processConfigQueue() {
        if (!allKarsInstalled()) {
            // some kars are not installed, delay installation for 15s, we keep the processing flag
            // because further updates will be added to the queue and are therefore not interfering
            // with our order
            // we don't need to keep the job, if the service is shutdown, the scheduler is also shutting
            // down and in all other cases we are protected by the processing flag
            logger.info("Some .kar files are not installed yet. Delaying add-on installation by 15s.");
            scheduler.schedule(this::processConfigQueue, 15, TimeUnit.SECONDS);
        Map<String, Object> config;
        boolean changed = false;
        while ((config = configQueue.poll()) != null) {
            // cache the last processed config
            configMapCache = config;
            // online mode is either determined by the configuration or by the status of the online repository
            boolean onlineMode = ConfigParser.valueAsOrElse(config.get(CFG_REMOTE), Boolean.class,
                    getOnlineRepositoryMode());
            boolean repoConfigurationChanged = getOnlineRepositoryMode() != onlineMode
                    && setOnlineRepositoryMode(onlineMode);
            if (repoConfigurationChanged) {
                waitForConfigUpdateEvent();
            if (installAddons(config)) {
                featuresService.refreshFeatures(EnumSet.noneOf(FeaturesService.Option.class));
            logger.error("Failed to refresh bundles after processing config update", e);
        processingConfigQueue.set(false);
    public void addAddon(String type, String id) {
            changeAddonConfig(type, id, Collection::add);
            logger.warn("Adding add-on 'openhab-{}-{}' failed: {}", type, id, e.getMessage(), debugException(e));
    public void removeAddon(String type, String id) {
            changeAddonConfig(type, id, Collection::remove);
            logger.warn("Removing add-on 'openhab-{}-{}' failed: {}", type, id, e.getMessage(), debugException(e));
        if (event != null && PAX_URL_PID.equals(event.getPid()) && event.getType() == ConfigurationEvent.CM_UPDATED) {
            paxCfgUpdated = true;
    private @Nullable Exception debugException(Exception e) {
        return logger.isDebugEnabled() ? e : null;
    private boolean allKarsInstalled() {
            List<String> karRepos = karService.list();
            Configuration[] configurations = configurationAdmin
                    .listConfigurations("(service.factoryPid=org.apache.felix.fileinstall)");
            if (configurations.length > 0) {
                Dictionary<String, Object> felixProperties = configurations[0].getProperties();
                String addonsDirectory = (String) felixProperties.get("felix.fileinstall.dir");
                if (addonsDirectory != null) {
                    try (Stream<Path> files = Files.list(Path.of(addonsDirectory))) {
                        return files.map(Path::getFileName).map(Path::toString).filter(file -> file.endsWith(".kar"))
                                .map(karFileName -> karFileName.substring(0, karFileName.lastIndexOf(".")))
                                .allMatch(karRepos::contains);
        logger.warn("Could not determine addons folder, its content or the list of installed repositories!");
    private void waitForConfigUpdateEvent() {
        // wait up to 5 seconds for the config update event
        while (!paxCfgUpdated && counter++ < 50) {
                Thread.sleep(100);
            } catch (InterruptedException ignored) {
        logger.warn("Waited for 5s to receive config update, but configuration was not updated. Proceeding anyway.");
    private void changeAddonConfig(String type, String id, BiFunction<Collection<String>, String, Boolean> method)
        Configuration cfg = configurationAdmin.getConfiguration(OpenHAB.ADDONS_SERVICE_PID, null);
        Dictionary<String, Object> props = Objects.requireNonNullElse(cfg.getProperties(), new Hashtable<>());
        Object typeProp = props.get(type);
        String[] addonIds = typeProp != null ? typeProp.toString().split(",") : new String[0];
        Set<String> normalizedIds = new HashSet<>(); // sets don't allow duplicates
        Arrays.stream(addonIds).map(String::strip).forEach(normalizedIds::add);
        if (method.apply(normalizedIds, id)) {
            // collection changed
            props.put(type, String.join(",", normalizedIds));
            cfg.update(props);
    private void setOnlineRepoUrl() {
        Path versionFilePath = Path.of(OpenHAB.getUserDataFolder(), "etc", "version.properties");
        try (BufferedReader reader = Files.newBufferedReader(versionFilePath)) {
            Properties prop = new Properties();
            prop.load(reader);
            String repo = prop.getProperty("online-repo", "").strip();
            if (!repo.isEmpty()) {
                this.onlineRepoUrl = repo + "@id=openhab@snapshots";
                logger.warn("Cannot determine online repo url - online repo support will be disabled.");
            logger.warn("Cannot determine online repo url - online repo support will be disabled. Error: {}",
                    e.getMessage(), debugException(e));
     * Checks if the online repository is part of the maven repository list
     * @return <code>true</code> if present, <code>false</code> otherwise
    private boolean getOnlineRepositoryMode() {
        if (onlineRepoUrl != null) {
                Configuration paxCfg = configurationAdmin.getConfiguration(PAX_URL_PID, null);
                Dictionary<String, Object> properties = paxCfg.getProperties();
                Object repos = properties.get(PROPERTY_MVN_REPOS);
                if (repos instanceof String string) {
                    return List.of(string.split(",")).contains(onlineRepoUrl);
                logger.error("Failed getting the add-on management online/offline mode: {}", e.getMessage(),
                        debugException(e));
     * Enables or disables the online repository in the maven repository list
     * @param enabled the requested setting
     * @return <code>true</code> if the configuration was changed, <code>false</code> otherwise
    private boolean setOnlineRepositoryMode(boolean enabled) {
        String onlineRepoUrl = this.onlineRepoUrl;
                paxCfg.setBundleLocation("?");
                Dictionary<String, Object> properties = Objects.requireNonNullElse(paxCfg.getProperties(),
                        new Hashtable<>());
                List<String> repoCfg = new ArrayList<>();
                    repoCfg.addAll(Arrays.asList(string.split(",")));
                    repoCfg.remove("");
                if (enabled && !repoCfg.contains(onlineRepoUrl)) {
                    repoCfg.add(onlineRepoUrl);
                    logger.debug("Added repo '{}' to feature repo list.", onlineRepoUrl);
                } else if (!enabled && repoCfg.contains(onlineRepoUrl)) {
                    repoCfg.remove(onlineRepoUrl);
                    logger.debug("Removed repo '{}' from feature repo list.", onlineRepoUrl);
                    properties.put(PROPERTY_MVN_REPOS, String.join(",", repoCfg));
                    paxCfg.update(properties);
                logger.error("Failed setting the add-on management online/offline mode: {}", e.getMessage(),
    private boolean installAddons(final Map<String, Object> config) {
        final Set<String> currentAddons = new HashSet<>(); // the currently installed ones
        final Set<String> targetAddons = new HashSet<>(); // the target we want to have installed afterwards
        final Set<String> installAddons = new HashSet<>(); // the ones to be installed (the diff)
        for (String type : ADDON_TYPES) {
            Object configValue = config.get(type);
            if (configValue instanceof String addonString) {
                    Feature[] features = featuresService.listInstalledFeatures();
                    String typePrefix = PREFIX + type + "-";
                    Set<String> configFeatureNames = Arrays.stream(addonString.split(",")) //
                            .map(String::strip) //
                            .map(addon -> typePrefix + addon) //
                    for (String name : configFeatureNames) {
                        if (featuresService.getFeature(name) != null) {
                            targetAddons.add(name);
                            if (!anyMatchingFeature(features, withName(name))) {
                                installAddons.add(name);
                            logger.warn("The {} add-on '{}' does not exist - ignoring it.", type,
                                    name.substring(typePrefix.length()));
                    // we collect all installed add-ons of this type
                    getAllFeatureNamesWithPrefix(typePrefix).stream()
                            .filter(name -> anyMatchingFeature(features, withName(name))).forEach(currentAddons::add);
                    logger.error("Failed retrieving features: {}", e.getMessage(), debugException(e));
        // now calculate what we have to uninstall: all current ones that are not part of the target anymore
        Set<String> uninstallAddons = currentAddons.stream().filter(addon -> !targetAddons.contains(addon))
        // do the installation
        if (!installAddons.isEmpty()) {
            installFeatures(installAddons);
        // do the de-installation
        uninstallAddons.forEach(this::uninstallFeature);
        return !installAddons.isEmpty() || !uninstallAddons.isEmpty();
    private Set<String> getAllFeatureNamesWithPrefix(String prefix) {
            return Arrays.stream(featuresService.listFeatures()) //
                    .map(Feature::getName) //
                    .filter(name -> name.startsWith(prefix)) //
    private void installFeatures(Set<String> addons) {
                logger.debug("Installing '{}'", String.join(", ", addons));
            featuresService.installFeatures(addons, EnumSet.of(FeaturesService.Option.NoAutoRefreshBundles,
                    FeaturesService.Option.Upgrade, FeaturesService.Option.NoFailOnFeatureNotFound));
                Set<String> installed = new HashSet<>();
                Set<String> failed = new HashSet<>();
                for (String addon : addons) {
                    if (anyMatchingFeature(features, withName(addon))) {
                        installed.add(addon);
                        failed.add(addon);
                if (!installed.isEmpty() && logger.isDebugEnabled()) {
                    logger.debug("Installed '{}'", String.join(", ", installed));
                if (!failed.isEmpty()) {
                    logger.error("Failed installing '{}'", String.join(", ", failed));
                    configMapCache = null; // make sure we retry the installation
                installed.forEach(this::postInstalledEvent);
            logger.error("Failed installing '{}': {}", String.join(", ", addons), e.getMessage(), debugException(e));
    private void uninstallFeature(String name) {
            if (anyMatchingFeature(features, withName(name))) {
                featuresService.uninstallFeature(name);
                logger.debug("Uninstalled '{}'", name);
                postUninstalledEvent(name);
            logger.debug("Failed uninstalling '{}': {}", name, e.getMessage());
    private void postInstalledEvent(String featureName) {
        String extensionId = featureName.substring(PREFIX.length());
    private void postUninstalledEvent(String featureName) {
    private static boolean anyMatchingFeature(Feature[] features, Predicate<Feature> predicate) {
        return Arrays.stream(features).anyMatch(predicate);
    private static Predicate<Feature> withName(final String name) {
        return feature -> feature.getName().equals(name);
