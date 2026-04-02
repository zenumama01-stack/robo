import org.osgi.framework.BundleEvent;
import org.osgi.util.tracker.BundleTracker;
 * The {@link JarFileAddonService} is an add-on service that provides add-ons that are placed a .jar files in the
 * openHAB addons folder
@Component(immediate = true, service = AddonService.class, name = JarFileAddonService.SERVICE_NAME)
public class JarFileAddonService extends BundleTracker<Bundle> implements AddonService {
    public static final String SERVICE_ID = "jar";
    public static final String SERVICE_NAME = "jar-file-add-on-service";
    private static final Map<String, AddonType> ADDON_TYPE_MAP = Map.of( //
            "automation", new AddonType("automation", "Automation"), //
            "binding", new AddonType("binding", "Bindings"), //
            "misc", new AddonType("misc", "Misc"), //
            "persistence", new AddonType("persistence", "Persistence"), //
            "transformation", new AddonType("transformation", "Transformations"), //
            "ui", new AddonType("ui", "User Interfaces"), //
            "voice", new AddonType("voice", "Voice"));
    private final Logger logger = LoggerFactory.getLogger(JarFileAddonService.class);
    private final ScheduledExecutorService scheduler;
    private final Set<Bundle> trackedBundles = ConcurrentHashMap.newKeySet();
    private Map<String, Addon> addons = Map.of();
    public JarFileAddonService(final @Reference AddonInfoRegistry addonInfoRegistry, BundleContext context) {
        super(context, Bundle.ACTIVE, null);
        this.scheduler = ThreadPoolManager.getScheduledPool(ThreadPoolManager.THREAD_POOL_NAME_COMMON);
        open();
        Arrays.stream(context.getBundles()).filter(this::isRelevant).forEach(trackedBundles::add);
        scheduler.execute(this::refreshSource);
    public void deactivate() {
        close();
     * Checks if a bundle is loaded from a file and add-on information is available
     * @param bundle the bundle to check
     * @return <code>true</code> if bundle is considered, <code>false</code> otherwise
    public boolean isRelevant(Bundle bundle) {
        return bundle.getLocation().startsWith("file:") && bundle.getEntry("OH-INF/addon/addon.xml") != null;
    public final synchronized Bundle addingBundle(@NonNullByDefault({}) Bundle bundle,
            @NonNullByDefault({}) BundleEvent event) {
        if (isRelevant(bundle) && trackedBundles.add(bundle)) {
            logger.debug("Added {} to add-on list", bundle.getSymbolicName());
        return bundle;
    public final synchronized void modifiedBundle(@NonNullByDefault({}) Bundle bundle, @Nullable BundleEvent event,
            @NonNullByDefault({}) Bundle object) {
        if (isRelevant(bundle)) {
    public final synchronized void removedBundle(@NonNullByDefault({}) Bundle bundle,
            @NonNullByDefault({}) BundleEvent event, Bundle object) {
        if (trackedBundles.remove(bundle)) {
            logger.debug("Removed {} from add-on list", bundle.getSymbolicName());
    public synchronized void refreshSource() {
        addons = trackedBundles.stream().map(this::toAddon).filter(Objects::nonNull).map(Objects::requireNonNull)
                .collect(Collectors.toMap(Addon::getUid, addon -> addon));
    private @Nullable Addon toAddon(Bundle bundle) {
        return addonInfoRegistry.getAddonInfos().stream()
                .filter(info -> bundle.getSymbolicName().equals(info.getSourceBundle())).findAny()
                .map(info -> toAddon(bundle, info)).orElse(null);
    private Addon toAddon(Bundle bundle, AddonInfo addonInfo) {
        String uid = ADDON_ID_PREFIX + addonInfo.getUID();
        return Addon.create(uid).withId(addonInfo.getId()).withType(addonInfo.getType()).withInstalled(true)
                .withVersion(bundle.getVersion().toString()).withLabel(addonInfo.getName())
                .withConfigDescriptionURI(addonInfo.getConfigDescriptionURI())
                .withDescription(Objects.requireNonNullElse(addonInfo.getDescription(), bundle.getSymbolicName()))
                .withContentType(ADDONS_CONTENT_TYPE).withLoggerPackages(List.of(bundle.getSymbolicName())).build();
        if (trackedBundles.size() != addons.size()) {
        return List.copyOf(addons.values());
        return addons.get(queryId);
        return List.copyOf(ADDON_TYPE_MAP.values());
        throw new UnsupportedOperationException();
