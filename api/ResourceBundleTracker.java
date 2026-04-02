 * The {@link ResourceBundleTracker} class tracks all <i>OSGi</i> bundles which are in the {@link Bundle#RESOLVED} state
 * or which it already passed (e.g. {@link Bundle#STARTING} or {@link Bundle#ACTIVE}). Only bundles which contains i18n
 * resource files are considered within this tracker.
 * This tracker must be started by calling {@link #open()} and stopped by calling {@link #close()}.
 * @author Ana Dimova - fragments support
@SuppressWarnings({ "deprecation", "rawtypes" })
public class ResourceBundleTracker extends BundleTracker {
    private Map<Bundle, LanguageResourceBundleManager> bundleLanguageResourceMap;
    private PackageAdmin pkgAdmin;
    public ResourceBundleTracker(BundleContext bundleContext, LocaleProvider localeProvider) {
        super(bundleContext, Bundle.RESOLVED | Bundle.STARTING | Bundle.STOPPING | Bundle.ACTIVE, null);
        pkgAdmin = (PackageAdmin) bundleContext
                .getService(bundleContext.getServiceReference(PackageAdmin.class.getName()));
        this.bundleLanguageResourceMap = new LinkedHashMap<>();
    public synchronized void open() {
    public synchronized void close() {
        this.bundleLanguageResourceMap.clear();
    public synchronized @Nullable Object addingBundle(@Nullable Bundle bundle, @Nullable BundleEvent event) {
        if (isFragmentBundle(bundle)) {
            List<Bundle> hosts = returnHostBundles(bundle);
                addResourceBundle(host);
            addResourceBundle(bundle);
    public synchronized void removedBundle(@Nullable Bundle bundle, @Nullable BundleEvent event,
            @Nullable Object object) {
        LanguageResourceBundleManager languageResource = this.bundleLanguageResourceMap.remove(bundle);
            languageResource.clearCache();
                this.bundleLanguageResourceMap.remove(host);
            this.bundleLanguageResourceMap.remove(bundle);
     * Returns the {@link LanguageResourceBundleManager} instance for the specified bundle,
     * or {@code null} if it cannot be found within that tracker.
     * @param bundle the bundle which points to the specific resource manager (could be null)
     * @return the specific resource manager (could be null)
    public @Nullable LanguageResourceBundleManager getLanguageResource(@Nullable Bundle bundle) {
            return this.bundleLanguageResourceMap.get(bundle);
     * Returns all {@link LanguageResourceBundleManager} instances managed by this tracker.
     * @return the list of all resource managers (not null, could be empty)
    public Collection<LanguageResourceBundleManager> getAllLanguageResources() {
        return this.bundleLanguageResourceMap.values();
    private List<Bundle> returnHostBundles(@Nullable Bundle fragment) {
            hosts.addAll(Arrays.asList(bundles));
     * This method checks if the <i>OSGi</i> bundle parameter is a fragment bundle.
     * @param bundle the <i>OSGi</i> bundle that should be checked is it a fragment bundle.
     * @return <code>true</code> if the bundle is a fragment and <code>false</code> if it is a host.
    private boolean isFragmentBundle(@Nullable Bundle bundle) {
     * This method adds the localization resources provided by this <i>OSGi</i> bundle parameter if the bundle is not in
     * UNINSTALLED state.
     * @param bundle the <i>OSGi</i> bundle that was detected
    private void addResourceBundle(@Nullable Bundle bundle) {
        if (bundle != null && bundle.getState() != Bundle.UNINSTALLED) {
            LanguageResourceBundleManager languageResource = new LanguageResourceBundleManager(localeProvider, bundle);
            if (languageResource.containsResources()) {
                this.bundleLanguageResourceMap.put(bundle, languageResource);
