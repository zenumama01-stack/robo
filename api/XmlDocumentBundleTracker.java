package org.openhab.core.config.core.xml.osgi;
import java.util.concurrent.locks.ReadWriteLock;
import org.openhab.core.service.ReadyMarkerUtils;
 * The {@link XmlDocumentBundleTracker} tracks files in the specified XML folder
 * of modules and tries to parse them as XML file with the specified
 * {@link XmlDocumentReader}. Any converted XML files are assigned to its
 * according bundle and added to an {@link XmlDocumentProvider} for further
 * processing. For each module an own {@link XmlDocumentProvider} is created by
 * using the specified {@link XmlDocumentProviderFactory}.
 * @author Benedikt Niehues - Changed resource handling so that resources can be
 *         patched by fragments.
 * @author Simon Kaufmann - Tracking of remaining bundles
 * @author Markus Rathgeb - Harden the usage
 * @param <T> the result type of the conversion
public class XmlDocumentBundleTracker<@NonNull T> extends BundleTracker<Bundle> {
     * Execute given method while take the lock and unlock before return.
     * @param lock the lock that should be taken
     * @param supplier the method to execute. The return value of this method is returned
     * @return the return value of the supplier
    private static <T> T withLock(final Lock lock, final Supplier<T> supplier) {
            return supplier.get();
     * We use three open states, so we can ensure that open is only allowed to be called if the current state is
     * "created" and cannot be called after a close.
     * If the open is called asynchronously, there is a little chance that the serialized call hierarchy would be:
     * create, close, open
     * This can be handled correctly using three states and checking the transition.
    private enum OpenState {
        CREATED,
        OPENED,
        CLOSED
    public static final String THREAD_POOL_NAME = "file-processing";
    private final Logger logger = LoggerFactory.getLogger(XmlDocumentBundleTracker.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool(THREAD_POOL_NAME);
    private final String xmlDirectory;
    private final XmlDocumentReader<T> xmlDocumentTypeReader;
    private final XmlDocumentProviderFactory<T> xmlDocumentProviderFactory;
    private final Map<Bundle, XmlDocumentProvider<T>> bundleDocumentProviderMap = new ConcurrentHashMap<>();
    private final Map<Bundle, Future<?>> queue = new ConcurrentHashMap<>();
    private final Set<Bundle> finishedBundles = new CopyOnWriteArraySet<>();
    private final Map<String, ReadyMarker> bundleReadyMarkerRegistrations = new ConcurrentHashMap<>();
    private final String readyMarkerKey;
    private final ReadWriteLock lockOpenState = new ReentrantReadWriteLock();
    private OpenState openState = OpenState.CREATED;
    private @Nullable BundleTracker<?> relevantBundlesTracker;
     * @param bundleContext the bundle context to be used for tracking bundles (must not
     *            be null)
     * @param xmlDirectory the directory to search for XML files (must neither be null,
     *            nor empty)
     * @param xmlDocumentTypeReader the XML converter to be used (must not be null)
     * @param xmlDocumentProviderFactory the result object processor to be used (must not be null)
     * @param readyMarkerKey the key to use for registering {@link ReadyMarker}s
     * @throws IllegalArgumentException if any of the arguments is null
    public XmlDocumentBundleTracker(BundleContext bundleContext, String xmlDirectory,
            XmlDocumentReader<T> xmlDocumentTypeReader, XmlDocumentProviderFactory<T> xmlDocumentProviderFactory,
            String readyMarkerKey, ReadyService readyService) throws IllegalArgumentException {
        super(bundleContext, Bundle.ACTIVE, null);
        this.readyMarkerKey = readyMarkerKey;
        this.xmlDirectory = xmlDirectory;
        this.xmlDocumentTypeReader = xmlDocumentTypeReader;
        this.xmlDocumentProviderFactory = xmlDocumentProviderFactory;
    private boolean isBundleRelevant(Bundle bundle) {
        return isNotFragment(bundle) && isResourcePresent(bundle, xmlDirectory);
    private boolean isNotFragment(Bundle bundle) {
        return bundle.getHeaders().get(Constants.FRAGMENT_HOST) == null;
    private Set<Bundle> getRelevantBundles() {
        BundleTracker<?> bundleTracker = relevantBundlesTracker;
        if (bundleTracker == null || bundleTracker.getBundles() == null) {
        return Set.of(bundleTracker.getBundles());
    public final synchronized void open() {
        final OpenState openState = withLock(lockOpenState.writeLock(), () -> {
            if (this.openState == OpenState.CREATED) {
                this.openState = OpenState.OPENED;
            return this.openState;
        if (openState != OpenState.OPENED) {
            logger.warn("Open XML document bundle tracker forbidden (state: {})", openState);
        relevantBundlesTracker = new BundleTracker(context,
                Bundle.RESOLVED | Bundle.STARTING | Bundle.STOPPING | Bundle.ACTIVE, null) {
            public @Nullable Object addingBundle(@NonNullByDefault({}) Bundle bundle,
                return withLock(lockOpenState.readLock(),
                        () -> openState == OpenState.OPENED && isBundleRelevant(bundle) ? bundle : null);
        relevantBundlesTracker.open();
        super.open();
    public final synchronized void close() {
        withLock(lockOpenState.writeLock(), () -> openState = OpenState.CLOSED);
        clearQueue();
        unregisterReadyMarkers();
        bundleDocumentProviderMap.clear();
        if (relevantBundlesTracker != null) {
            relevantBundlesTracker.close();
        finishedBundles.clear();
    private void clearQueue() {
        for (Future<?> future : queue.values()) {
            future.cancel(true);
    private @Nullable XmlDocumentProvider<T> acquireXmlDocumentProvider(Bundle bundle) {
        XmlDocumentProvider<T> xmlDocumentProvider = bundleDocumentProviderMap.get(bundle);
        if (xmlDocumentProvider == null) {
            xmlDocumentProvider = xmlDocumentProviderFactory.createDocumentProvider(bundle);
            logger.trace("Create an empty XmlDocumentProvider for the module '{}'.",
                    ReadyMarkerUtils.getIdentifier(bundle));
            bundleDocumentProviderMap.put(bundle, xmlDocumentProvider);
        return xmlDocumentProvider;
    private void releaseXmlDocumentProvider(Bundle bundle) {
            logger.debug("Releasing the XmlDocumentProvider for module '{}'.", ReadyMarkerUtils.getIdentifier(bundle));
            xmlDocumentProvider.release();
            logger.error("Could not release the XmlDocumentProvider for '{}'!", ReadyMarkerUtils.getIdentifier(bundle),
        bundleDocumentProviderMap.remove(bundle);
    private void addingObject(Bundle bundle, T object) {
        XmlDocumentProvider<T> xmlDocumentProvider = acquireXmlDocumentProvider(bundle);
        if (xmlDocumentProvider != null) {
            xmlDocumentProvider.addingObject(object);
    private void addingFinished(Bundle bundle) {
            xmlDocumentProvider.addingFinished();
            logger.error("Could not send adding finished event for the module '{}'!",
                    ReadyMarkerUtils.getIdentifier(bundle), ex);
        addingBundle(bundle);
        logger.trace("Removing the XML related objects from module '{}'...", ReadyMarkerUtils.getIdentifier(bundle));
        finishedBundles.remove(bundle);
        Future<?> future = queue.remove(bundle);
        releaseXmlDocumentProvider(bundle);
        unregisterReadyMarker(bundle);
     * This method creates a list where all resources are contained
     * except the ones from the host bundle which are also contained in
     * a fragment. So the fragment bundle resources can override the
     * host bundles resources.
     * @param xmlDocumentPaths the paths within the bundle/fragments
     * @param bundle the host bundle
     * @return the URLs of the resources, never {@code null}
    private Collection<URL> filterPatches(Enumeration<URL> xmlDocumentPaths, Bundle bundle) {
        List<URL> hostResources = new ArrayList<>();
        List<URL> fragmentResources = new ArrayList<>();
        while (xmlDocumentPaths.hasMoreElements()) {
            URL path = xmlDocumentPaths.nextElement();
            if (bundle.getEntry(path.getPath()) != null && bundle.getEntry(path.getPath()).equals(path)) {
                hostResources.add(path);
                fragmentResources.add(path);
        if (!fragmentResources.isEmpty()) {
            Map<String, URL> helper = new HashMap<>();
            for (URL url : hostResources) {
                helper.put(url.getPath(), url);
            for (URL url : fragmentResources) {
            return helper.values();
        return hostResources;
     * Checks for the existence of a given resource inside the bundle and its attached fragments.
     * Helper method which can be used in {@link #isBundleRelevant(Bundle)}.
     * @param path the directory name to look for
     * @return <code>true</code> if the bundle or one of its attached fragments contain the given directory
    private final boolean isResourcePresent(Bundle bundle, String path) {
        return bundle.getEntry(path) != null;
     * Add a bundle which potentially needs to be processed.
     * This method should be called in order to queue a new bundle for asynchronous processing.
     * It can be used e.g. by a {@link BundleTracker}, detecting a new bundle.
     * If the bundle actually will be put into the queue depends on the presence of the corresponding XML configuration
     * directory.
    private void addingBundle(Bundle bundle) {
        if (withLock(lockOpenState.readLock(), () -> openState != OpenState.OPENED)) {
        queue.put(bundle, scheduler.submit(new Runnable() {
            // this should remain an anonymous class and not be converted to a lambda because of
            // http://bugs.java.com/view_bug.do?bug_id=8073755
                    processBundle(bundle);
                } catch (final RuntimeException ex) {
                    // Check if our OSGi instance is still active.
                    // If the component has been deactivated while the execution hide the exception.
                    if (withLock(lockOpenState.readLock(), () -> openState == OpenState.OPENED)) {
    private void finishBundle(Bundle bundle) {
        queue.remove(bundle);
        finishedBundles.add(bundle);
        registerReadyMarker(bundle);
        Set<Bundle> remainingBundles = getRemainingBundles();
        if (!remainingBundles.isEmpty()) {
            logger.trace("Remaining bundles with {}: {}", xmlDirectory, remainingBundles);
            logger.trace("Finished loading bundles with {}", xmlDirectory);
            loadingCompleted();
    private Set<Bundle> getRemainingBundles() {
        return getRelevantBundles().stream().filter(b -> !finishedBundles.contains(b)).collect(Collectors.toSet());
    private void processBundle(Bundle bundle) {
        if (isNotFragment(bundle)) {
            Enumeration<URL> xmlDocumentPaths = bundle.findEntries(xmlDirectory, "*.xml", true);
            if (xmlDocumentPaths != null) {
                Collection<URL> filteredPaths = filterPatches(xmlDocumentPaths, bundle);
                parseDocuments(bundle, filteredPaths);
        finishBundle(bundle);
    private void parseDocuments(Bundle bundle, Collection<URL> filteredPaths) {
        int numberOfParsedXmlDocuments = 0;
        for (URL xmlDocumentURL : filteredPaths) {
            String moduleName = ReadyMarkerUtils.getIdentifier(bundle);
            String xmlDocumentFile = xmlDocumentURL.getFile();
            logger.debug("Reading the XML document '{}' in module '{}'...", xmlDocumentFile, moduleName);
                T object = xmlDocumentTypeReader.readFromXML(xmlDocumentURL);
                if (object != null) {
                    addingObject(bundle, object);
                numberOfParsedXmlDocuments++;
                // If we are not open, we can stop here.
                logger.warn("The XML document '{}' in module '{}' could not be parsed: {}", xmlDocumentFile, moduleName,
                        ex.getLocalizedMessage(), ex);
        if (numberOfParsedXmlDocuments > 0) {
            addingFinished(bundle);
    private void registerReadyMarker(Bundle bundle) {
        final String identifier = ReadyMarkerUtils.getIdentifier(bundle);
        if (!bundleReadyMarkerRegistrations.containsKey(identifier)) {
            ReadyMarker readyMarker = new ReadyMarker(readyMarkerKey, identifier);
            readyService.markReady(readyMarker);
            bundleReadyMarkerRegistrations.put(identifier, readyMarker);
    private void unregisterReadyMarker(Bundle bundle) {
        ReadyMarker readyMarker = bundleReadyMarkerRegistrations.remove(identifier);
        if (readyMarker != null) {
            readyService.unmarkReady(readyMarker);
    private void unregisterReadyMarkers() {
        for (ReadyMarker readyMarker : bundleReadyMarkerRegistrations.values()) {
        bundleReadyMarkerRegistrations.clear();
    private void loadingCompleted() {
        return super.toString() + "(" + xmlDirectory + ")";
