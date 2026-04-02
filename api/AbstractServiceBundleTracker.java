package org.openhab.core.service;
 * The {@link AbstractServiceBundleTracker} tracks a set of bundles (selected {@link #isRelevantBundle(Bundle)}
 * and sets the
 * {@link #readyMarker} when all registered bundles are active
public abstract class AbstractServiceBundleTracker extends BundleTracker<Bundle> implements ReadyService.ReadyTracker {
    private static final int STATE_MASK = Bundle.INSTALLED | Bundle.RESOLVED | Bundle.ACTIVE | Bundle.STARTING
            | Bundle.STOPPING | Bundle.UNINSTALLED;
    private final ReadyMarker readyMarker;
    private final Map<String, Integer> bundles = new ConcurrentHashMap<>();
    private boolean startLevel = false;
    private boolean ready = false;
    public AbstractServiceBundleTracker(final @Reference ReadyService readyService, BundleContext bc,
            ReadyMarker readyMarker) {
        super(bc, STATE_MASK, null);
        this.readyMarker = readyMarker;
        this.open();
        ready = false;
    private boolean allBundlesActive() {
        return bundles.values().stream().allMatch(i -> i == Bundle.ACTIVE);
    public Bundle addingBundle(@NonNullByDefault({}) Bundle bundle, @Nullable BundleEvent event) {
        int state = bundle.getState();
        if (isRelevantBundle(bundle)) {
            logger.debug("Added {}: {} ", bsn, stateToString(state));
            bundles.put(bsn, state);
            checkReady();
    public void modifiedBundle(@NonNullByDefault({}) Bundle bundle, @Nullable BundleEvent event,
            logger.debug("Modified {}: {}", bsn, stateToString(state));
    public void removedBundle(@NonNullByDefault({}) Bundle bundle, @Nullable BundleEvent event,
            logger.debug("Removed {}", bsn);
            bundles.remove(bsn);
        logger.debug("Readymarker '{}' added", readyMarker);
        startLevel = true;
        logger.debug("Readymarker '{}' removed", readyMarker);
        startLevel = false;
    private synchronized void checkReady() {
        boolean allBundlesActive = allBundlesActive();
        logger.trace("ready: {}, startlevel: {}, allActive: {}", ready, startLevel, allBundlesActive);
        if (!ready && startLevel && allBundlesActive) {
            logger.debug("Adding ready marker '{}': All bundles ready ({})", readyMarker, bundles);
            ready = true;
        } else if (ready && !allBundlesActive) {
            logger.debug("Removing ready marker '{}' : Not all bundles ready ({})", readyMarker, bundles);
    private String stateToString(int state) {
            case Bundle.UNINSTALLED -> "UNINSTALLED";
            case Bundle.INSTALLED -> "INSTALLED";
            case Bundle.RESOLVED -> "RESOLVED";
            case Bundle.STARTING -> "STARTING";
            case Bundle.STOPPING -> "STOPPING";
            case Bundle.ACTIVE -> "ACTIVE";
            default -> "UNKNOWN";
     * Decide if a bundle should be tracked by this bundle tracker
     * @param bundle the bundle
     * @return {@code true} if the bundle should be considered, {@code false} otherwise
    protected abstract boolean isRelevantBundle(Bundle bundle);
