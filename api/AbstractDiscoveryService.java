package org.openhab.core.config.discovery;
import java.util.concurrent.CancellationException;
 * The {@link AbstractDiscoveryService} provides methods which handle the {@link DiscoveryListener}s.
 * Subclasses do not have to care about adding and removing those listeners.
 * They can use the protected methods {@link #thingDiscovered(DiscoveryResult)} and {@link #thingRemoved(ThingUID)} in
 * order to notify the registered {@link DiscoveryListener}s.
 * @author Oliver Libutzki - Initial contribution
 * @author Kai Kreuzer - Refactored API
 * @author Dennis Nobel - Added background discovery configuration through Configuration Admin
 * @author Andre Fuechsel - Added removeOlderResults
 * @author Laurent Garnier - Added discovery with an optional input parameter
public abstract class AbstractDiscoveryService implements DiscoveryService {
    private static final String DISCOVERY_THREADPOOL_NAME = "discovery";
    private final Logger logger = LoggerFactory.getLogger(AbstractDiscoveryService.class);
    private final Set<DiscoveryListener> discoveryListeners = new CopyOnWriteArraySet<>();
    protected @Nullable ScanListener scanListener;
    private volatile boolean backgroundDiscoveryEnabled;
    private final @Nullable String scanInputLabel;
    private final @Nullable String scanInputDescription;
    // All access must be guarded by "cachedResults"
    private final Map<ThingUID, DiscoveryResult> cachedResults = new HashMap<>();
    // This set is immutable and can safely be shared between threads
    private final Set<ThingTypeUID> supportedThingTypes;
    private final int timeout;
    private Instant timestampOfLastScan = Instant.MIN;
    private @Nullable ScheduledFuture<?> scheduledStop;
    protected @NonNullByDefault({}) TranslationProvider i18nProvider;
    protected @NonNullByDefault({}) LocaleProvider localeProvider;
     * @param supportedThingTypes the list of Thing types which are supported (can be null)
     * @param timeout the discovery timeout in seconds after which the discovery
     *            service automatically stops its forced discovery process (>= 0).
     * @param backgroundDiscoveryEnabledByDefault defines, whether the default for this discovery service is to
     *            enable background discovery or not.
     * @param scanInputLabel the label of the optional input parameter to start the discovery or null if no input
     *            parameter supported
     * @param scanInputDescription the description of the optional input parameter to start the discovery or null if no
     *            input parameter supported
     * @throws IllegalArgumentException if {@code timeout < 0}
    protected AbstractDiscoveryService(@Nullable Set<ThingTypeUID> supportedThingTypes, int timeout,
            boolean backgroundDiscoveryEnabledByDefault, @Nullable String scanInputLabel,
            @Nullable String scanInputDescription) throws IllegalArgumentException {
        if (timeout < 0) {
            throw new IllegalArgumentException("The timeout must be >= 0!");
        this.scheduler = ThreadPoolManager.getScheduledPool(DISCOVERY_THREADPOOL_NAME);
        this.supportedThingTypes = supportedThingTypes == null ? Set.of() : Set.copyOf(supportedThingTypes);
        this.timeout = timeout;
        this.backgroundDiscoveryEnabled = backgroundDiscoveryEnabledByDefault;
        this.scanInputLabel = scanInputLabel;
        this.scanInputDescription = scanInputDescription;
     * <b>For use by tests only</b>, allows setting a different {@link ScheduledExecutorService} like
     * {@link org.openhab.core.util.SameThreadExecutorService} for synchronous behavior during testing.
     * @param scheduler the {@link ScheduledExecutorService} to use.
    protected AbstractDiscoveryService(ScheduledExecutorService scheduler,
            @Nullable Set<ThingTypeUID> supportedThingTypes, int timeout, boolean backgroundDiscoveryEnabledByDefault,
            @Nullable String scanInputLabel, @Nullable String scanInputDescription) throws IllegalArgumentException {
     * Creates a new instance of this class with the specified parameters and no input parameter supported to start the
     * discovery.
            boolean backgroundDiscoveryEnabledByDefault) throws IllegalArgumentException {
        this(supportedThingTypes, timeout, backgroundDiscoveryEnabledByDefault, null, null);
     * Creates a new instance of this class with the specified parameters and background discovery enabled
     * and no input parameter supported to start the discovery.
     * @param timeout the discovery timeout in seconds after which the discovery service
     *            automatically stops its forced discovery process (>= 0).
     *            If set to 0, disables the automatic stop.
    protected AbstractDiscoveryService(@Nullable Set<ThingTypeUID> supportedThingTypes, int timeout)
        this(supportedThingTypes, timeout, true);
    protected AbstractDiscoveryService(int timeout) throws IllegalArgumentException {
        this(null, timeout);
     * Returns the list of {@code Thing} types which are supported by the {@link DiscoveryService}.
     * @return the list of Thing types which are supported by the discovery service
    public boolean isScanInputSupported() {
        return getScanInputLabel() != null && getScanInputDescription() != null;
    public @Nullable String getScanInputLabel() {
        return scanInputLabel;
    public @Nullable String getScanInputDescription() {
        return scanInputDescription;
     * Returns the amount of time in seconds after which the discovery service automatically
     * stops its forced discovery process.
     * @return the discovery timeout in seconds (>= 0).
    public int getScanTimeout() {
    public boolean isBackgroundDiscoveryEnabled() {
        return backgroundDiscoveryEnabled;
    public void addDiscoveryListener(@Nullable DiscoveryListener listener) {
        if (listener == null) {
        Map<ThingUID, DiscoveryResult> existingResults;
        synchronized (cachedResults) {
            existingResults = Map.copyOf(cachedResults);
        for (DiscoveryResult existingResult : existingResults.values()) {
            listener.thingDiscovered(this, existingResult);
    public void removeDiscoveryListener(@Nullable DiscoveryListener listener) {
    public void startScan(@Nullable ScanListener listener) {
        startScanInternal(null, listener);
    public void startScan(String input, @Nullable ScanListener listener) {
        startScanInternal(input, listener);
    private void startScanInternal(@Nullable String input, @Nullable ScanListener listener) {
        // we first stop any currently running scan and its scheduled stop call
            ScheduledFuture<?> scheduledStop = this.scheduledStop;
            if (scheduledStop != null) {
                scheduledStop.cancel(false);
                this.scheduledStop = null;
            scanListener = listener;
            // schedule an automatic call of stopScan when timeout is reached
            if (getScanTimeout() > 0) {
                scheduledStop = scheduler.schedule(() -> {
                        logger.debug("Exception occurred during execution: {}", e.getMessage(), e);
                }, getScanTimeout(), TimeUnit.SECONDS);
            timestampOfLastScan = Instant.now();
            if (isScanInputSupported() && input != null) {
                startScan(input);
                scanListener = null;
    public void abortScan() {
        ScanListener scanListener = null;
                scheduledStop.cancel(true);
            scanListener = this.scanListener;
            this.scanListener = null;
        if (scanListener != null) {
            Exception e = new CancellationException("Scan has been aborted.");
            scanListener.onErrorOccurred(e);
     * This method is called by the {@link #startScan(ScanListener)} implementation of the
     * {@link AbstractDiscoveryService}.
     * The abstract class schedules a call of {@link #stopScan()} after {@link #getScanTimeout()} seconds. If this
     * behavior is not appropriate, the {@link #startScan(ScanListener)} method should be overridden.
    protected abstract void startScan();
    // An abstract method would have required a change in all existing bindings implementing a DiscoveryService
    protected void startScan(String input) {
        logger.warn("Discovery with input parameter not implemented by service '{}'!", this.getClass().getName());
     * This method cleans up after a scan, i.e. it removes listeners and other required operations.
    protected void stopScan() {
            scanListener.onFinished();
     * Notifies the registered {@link DiscoveryListener}s about a discovered device.
     * @param discoveryResult the {@link DiscoveryResult} to send to listeners.
    protected void thingDiscovered(final DiscoveryResult discoveryResult) {
        thingDiscovered(discoveryResult, FrameworkUtil.getBundle(this.getClass()));
     * Notifies the registered {@link DiscoveryListener}s about a discovered device, localizing the results
     * using the specified {@link Bundle}.
     * @param bundle the {@link Bundle} to use when looking up translations.
    protected void thingDiscovered(final DiscoveryResult discoveryResult, @Nullable Bundle bundle) {
        final DiscoveryResult discoveryResultNew = getLocalizedDiscoveryResult(discoveryResult, bundle);
        for (DiscoveryListener discoveryListener : discoveryListeners) {
                    discoveryListener.thingDiscovered(this, discoveryResultNew);
                    logger.error("An error occurred while calling the discovery listener {}.",
                            discoveryListener.getClass().getName(), e);
            cachedResults.put(discoveryResultNew.getThingUID(), discoveryResultNew);
     * Notifies the registered {@link DiscoveryListener}s about a removed device.
     * @param thingUID The UID of the removed thing.
    protected void thingRemoved(ThingUID thingUID) {
                discoveryListener.thingRemoved(this, thingUID);
            cachedResults.remove(thingUID);
     * Call to remove all results of all {@link #supportedThingTypes} that are
     * older than the given timestamp. To remove all left over results after a
     * full scan, this method could be called {@link #getTimestampOfLastScan()}
     * as timestamp.
     * @param timestamp timestamp, older results will be removed
    protected void removeOlderResults(Instant timestamp) {
        removeOlderResults(timestamp, null, null);
     * @param bridgeUID if not {@code null} only results of that bridge are being removed
    protected void removeOlderResults(Instant timestamp, @Nullable ThingUID bridgeUID) {
        removeOlderResults(timestamp, null, bridgeUID);
     * Call to remove all results of the given types that are older than the
     * given timestamp. To remove all left over results after a full scan, this
     * method could be called {@link #getTimestampOfLastScan()} as timestamp.
     * @param thingTypeUIDs collection of {@code ThingType}s, only results of these
     *            {@code ThingType}s will be removed; if {@code null} then
     *            {@link DiscoveryService#getSupportedThingTypes()} will be used
     *            instead
    protected void removeOlderResults(Instant timestamp, @Nullable Collection<ThingTypeUID> thingTypeUIDs,
            @Nullable ThingUID bridgeUID) {
        Set<ThingUID> removedThings = new HashSet<>();
        Collection<ThingUID> removed;
        Collection<ThingTypeUID> toBeRemoved = thingTypeUIDs != null ? thingTypeUIDs : getSupportedThingTypes();
                removed = discoveryListener.removeOlderResults(this, timestamp, toBeRemoved, bridgeUID);
                if (removed != null) {
                    removedThings.addAll(removed);
        if (!removedThings.isEmpty()) {
                for (ThingUID uid : removedThings) {
                    cachedResults.remove(uid);
     * Called on component activation, if the implementation of this class is an
     * OSGi declarative service and does not override the method. The method
     * implementation calls {@link AbstractDiscoveryService#startBackgroundDiscovery()} if background
     * discovery is enabled by default and not overridden by the configuration.
     * @param configProperties configuration properties
        if (configProperties != null) {
            backgroundDiscoveryEnabled = ConfigParser.valueAsOrElse(
                    configProperties.get(DiscoveryService.CONFIG_PROPERTY_BACKGROUND_DISCOVERY), Boolean.class,
                    backgroundDiscoveryEnabled);
        if (backgroundDiscoveryEnabled) {
            startBackgroundDiscovery();
            logger.debug("Background discovery for discovery service '{}' enabled.", this.getClass().getName());
     * Called when the configuration for the discovery service is changed. If
     * background discovery should be enabled and is currently disabled, the
     * method {@link AbstractDiscoveryService#startBackgroundDiscovery()} is
     * called. If background discovery should be disabled and is currently
     * enabled, the method {@link AbstractDiscoveryService#stopBackgroundDiscovery()} is called. In
     * all other cases, nothing happens.
            boolean enabled = ConfigParser.valueAsOrElse(
            if (backgroundDiscoveryEnabled && !enabled) {
                stopBackgroundDiscovery();
                logger.debug("Background discovery for discovery service '{}' disabled.", this.getClass().getName());
            } else if (!backgroundDiscoveryEnabled && enabled) {
            backgroundDiscoveryEnabled = enabled;
     * Called on component deactivation, if the implementation of this class is
     * an OSGi declarative service and does not override the method. The method
     * implementation calls {@link AbstractDiscoveryService#stopBackgroundDiscovery()} if background
     * discovery is enabled at the time of component deactivation.
     * Can be overridden to start background discovery logic. This method is
     * called if background discovery is enabled when the component is being
     * activated (see {@link AbstractDiscoveryService#activate}.
        // can be overridden
     * Can be overridden to stop background discovery logic. This method is
     * deactivated (see {@link AbstractDiscoveryService#deactivate()}.
     * Get the timestamp of the last call of {@link #startScan()}.
     * @return timestamp as {@link Instant}
    protected synchronized Instant getTimestampOfLastScan() {
        return timestampOfLastScan;
    private String inferKey(DiscoveryResult discoveryResult, String lastSegment) {
        return "discovery." + discoveryResult.getThingUID().getAsString().replace(":", ".") + "." + lastSegment;
    protected DiscoveryResult getLocalizedDiscoveryResult(final DiscoveryResult discoveryResult,
            @Nullable Bundle bundle) {
        TranslationProvider i18nProvider = this.i18nProvider;
        LocaleProvider localeProvider = this.localeProvider;
        if (i18nProvider != null && localeProvider != null) {
            String currentLabel = discoveryResult.getLabel();
            String key = I18nUtil.stripConstantOr(currentLabel, () -> inferKey(discoveryResult, "label"));
            ParsedKey parsedKey = new ParsedKey(key);
            String label = i18nProvider.getText(bundle, parsedKey.key, currentLabel, localeProvider.getLocale(),
                    parsedKey.args);
            if (currentLabel.equals(label)) {
                return discoveryResult;
                return DiscoveryResultBuilder.create(discoveryResult).withLabel(label).build();
     * Utility class to parse the key with parameters into the key and optional arguments.
    private static final class ParsedKey {
        private static final int LIMIT = 2;
        private final String key;
        private final Object @Nullable [] args;
        private ParsedKey(String label) {
            String[] parts = label.split("\\s+", LIMIT);
            this.key = parts[0];
            if (parts.length == 1) {
                this.args = Arrays.stream(parts[1].replaceAll("\\[|\\]|\"", "").split(",")).filter(s -> !s.isBlank())
                        .map(String::trim).toArray(Object[]::new);
