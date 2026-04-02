import java.util.jar.JarOutputStream;
import java.util.jar.Manifest;
import java.util.zip.ZipEntry;
 * Utility class for creation, installation, update and uninstallation of
 * synthetic bundles for the purpose of testing. The synthetic bundles content
 * should be stored into separate sub-directories of {@value #BUNDLE_POOL_PATH}
 * (which itself is situated in the test bundle's source directory). The
 * synthetic bundle is packed as a JAR and installed into the test runtime.
 * @author Dennis Nobel - Generalized the mechanism for creation of bundles by list of extensions to include
 * @author Simon Kaufmann - Install method returns when the bundle is fully loaded
 * @author Stefan Bussweiler - The list of extensions to include is extended with JSON
 * @author Andre Fuechsel - Implemented method for adding fragment
 * @author Kai Kreuzer - Applied formatting and license to the file
 * @author Dimitar Ivanov - The extension to include can be configured or default ones can be used; update method is
 *         introduced
public class SyntheticBundleInstaller {
    private static final int WAIT_TIMOUT = 30; // [seconds]
    private static final String BUNDLE_POOL_PATH = "/test-bundle-pool";
    private static final String XML_ADDON_INFO = "openhab.xmlAddonInfo";
    private static final String XML_CONFIG = "openhab.xmlConfig";
     * A list of default extensions to be included in the synthetic bundle.
    private static final Set<String> DEFAULT_EXTENSIONS = Set.of("*.xml", "*.properties", "*.json", ".keep");
     * Install synthetic bundle, denoted by its name, into the test runtime (by using the given bundle context). Only
     * the default extensions set
     * ({@link #DEFAULT_EXTENSIONS}) will be included into the synthetic bundle
     * @param bundleContext the bundle context of the test runtime
     * @param testBundleName the symbolic name of the sub-directory of {@value #BUNDLE_POOL_PATH}, which contains the
     *            files for the synthetic bundle
     * @return the synthetic bundle representation
     * @throws Exception thrown when error occurs while installing or starting the synthetic bundle
    public static Bundle install(BundleContext bundleContext, String testBundleName) throws Exception {
        return install(bundleContext, testBundleName, DEFAULT_EXTENSIONS);
     * Install synthetic bundle, denoted by its name, into the test runtime (by using the given bundle context).
     *            files
     *            for the synthetic bundle
     * @param extensionsToInclude a list of extension to be included into the synthetic bundle. In order to use the list
     *            of default extensions ({@link #DEFAULT_EXTENSIONS})
    public static Bundle install(BundleContext bundleContext, String testBundleName, Set<String> extensionsToInclude)
        String bundlePath = BUNDLE_POOL_PATH + "/" + testBundleName + "/";
        byte[] syntheticBundleBytes = createSyntheticBundle(bundleContext.getBundle(), bundlePath, testBundleName,
                extensionsToInclude);
        Bundle syntheticBundle = bundleContext.installBundle(testBundleName,
                new ByteArrayInputStream(syntheticBundleBytes));
        syntheticBundle.start(Bundle.ACTIVE);
        waitUntilLoadingFinished(bundleContext, syntheticBundle);
        return syntheticBundle;
     * @param extensionsToInclude a list of extension to be included into the synthetic bundle
    public static Bundle install(BundleContext bundleContext, String testBundleName, String... extensionsToInclude)
        Set<String> extensionsSet = new HashSet<>(Arrays.asList(extensionsToInclude));
        return install(bundleContext, testBundleName, extensionsSet);
     * Updates given bundle into the test runtime (the content is changed, but the symbolic name of the bundles remains
     * the same) with a new content, prepared in another resources directory.
     * @param bundleToUpdateName the symbolic name of the bundle to be updated
     * @param updateDirName the location of the new content, that the target bundle will be updated with
     * @return the Bundle representation of the updated bundle
    public static Bundle update(BundleContext bundleContext, String bundleToUpdateName, String updateDirName)
        return update(bundleContext, bundleToUpdateName, updateDirName, DEFAULT_EXTENSIONS);
    public static Bundle update(BundleContext bundleContext, String bundleToUpdateName, String updateDirName,
            Set<String> extensionsToInclude) throws Exception {
        // Stop the bundle to update first
            if (bundleToUpdateName.equals(bundle.getSymbolicName())) {
                // we have to uninstall the bundle to update its contents
        // New bytes are taken from the update path
        String updatePath = BUNDLE_POOL_PATH + "/" + updateDirName + "/";
        byte[] updatedBundleBytes = createSyntheticBundle(bundleContext.getBundle(), updatePath, bundleToUpdateName,
        // The updated bytes are installed with the same name
        Bundle syntheticBundle = bundleContext.installBundle(bundleToUpdateName,
                new ByteArrayInputStream(updatedBundleBytes));
        // Starting the bundle
            String... extensionsToInclude) throws Exception {
        return update(bundleContext, bundleToUpdateName, updateDirName, extensionsSet);
     * Install synthetic bundle fragment, denoted by its name, into the test
     * runtime (by using the given bundle context). Only the default extensions
     * set ({@link #DEFAULT_EXTENSIONS}) will be included into the synthetic
     * bundle fragment.
     * @param testBundleName the name of the sub-directory of {@value #BUNDLE_POOL_PATH}, which contains the files for
     *            the synthetic bundle
     * @throws Exception thrown when error occurs while installing or starting the synthetic bundle fragment
    public static Bundle installFragment(BundleContext bundleContext, String testBundleName) throws Exception {
        return installFragment(bundleContext, testBundleName, DEFAULT_EXTENSIONS);
     * Install synthetic bundle fragment, denoted by its name, into the test runtime (by using the given bundle
     * context). Only the default extensions set ({@link #DEFAULT_EXTENSIONS}) will be included into the synthetic
    public static Bundle installFragment(BundleContext bundleContext, String testBundleName,
        return bundleContext.installBundle(testBundleName, new ByteArrayInputStream(syntheticBundleBytes));
    private static boolean isBundleAvailable(BundleContext context, String bsn) {
        for (Bundle bundle : context.getBundles()) {
            final String bsnCurrentBundle = bundle.getSymbolicName();
            if (bsnCurrentBundle != null) {
                if (bsnCurrentBundle.equals(bsn) && bundle.getState() == Bundle.ACTIVE) {
    private static boolean isXmlThingTypeBundleAvailable(BundleContext context) {
        return isBundleAvailable(context, "org.openhab.core.thing");
    private static boolean isXmlAddonInfoBundleAvailable(BundleContext context) {
        return isBundleAvailable(context, "org.openhab.core.addon");
    private static boolean isXmlConfigBundleAvailable(BundleContext context) {
        return isBundleAvailable(context, "org.openhab.core.config");
     * Explicitly wait for the given bundle to finish its loading
     * @param bundle the bundle object representation
    public static void waitUntilLoadingFinished(BundleContext context, Bundle bundle) {
        if (isXmlThingTypeBundleAvailable(context)) {
            waitForReadyMarker(context, XML_THING_TYPE, bundle);
        if (isXmlAddonInfoBundleAvailable(context)) {
            waitForReadyMarker(context, XML_ADDON_INFO, bundle);
        if (isXmlConfigBundleAvailable(context)) {
            waitForReadyMarker(context, XML_CONFIG, bundle);
    private static void waitForReadyMarker(BundleContext context, String marker, Bundle bundle) {
        if (bundle.getHeaders().get(Constants.FRAGMENT_HOST) != null) {
        ServiceReference<?> readyServiceRef = context.getServiceReference(ReadyService.class.getName());
        ReadyService readyService = (ReadyService) context.getService(readyServiceRef);
        ReadyMarker expected = new ReadyMarker(marker, identifier);
        while (!readyService.isReady(expected)) {
            if (System.nanoTime() - startTime > TimeUnit.SECONDS.toNanos(WAIT_TIMOUT)) {
                fail(MessageFormat.format("Timout waiting for marker {0} at bundle {1}", marker, identifier));
        context.ungetService(readyServiceRef);
     * Uninstalls the synthetic bundle (or bundle fragment) from the test runtime.
     * @param bundle the bundle to uninstall
     * @throws BundleException if error is met during the bundle uninstall
    public static void uninstall(final Bundle bundle) throws BundleException {
     * Uninstalls the synthetic bundle (or bundle fragment), denoted by its name, from the test runtime.
     * This method should only be used if the bundle itself provides a symbolic name.
     * If possible you should use {@link #uninstall(Bundle)} and give the bundle that has been provided by one of the
     * install methods.
     * @param testBundleName the name of the test bundle to be uninstalled
    public static void uninstall(BundleContext bundleContext, String testBundleName) throws BundleException {
            if (testBundleName.equals(bundle.getSymbolicName())) {
    private static byte[] createSyntheticBundle(Bundle bundle, String bundlePath, String bundleName,
        Manifest manifest = getManifest(bundle, bundlePath);
        JarOutputStream jarOutputStream = manifest != null ? new JarOutputStream(outputStream, manifest)
                : new JarOutputStream(outputStream);
        List<String> files = collectFilesFrom(bundle, bundlePath, bundleName, extensionsToInclude);
        for (String file : files) {
            addFileToArchive(bundle, bundlePath, file, jarOutputStream);
        jarOutputStream.close();
        return outputStream.toByteArray();
    private static void addFileToArchive(Bundle bundle, String bundlePath, String fileInBundle,
            JarOutputStream jarOutputStream) throws IOException {
        String filePath = bundlePath + fileInBundle;
        URL resource = bundle.getResource(filePath);
        ZipEntry zipEntry = new ZipEntry(fileInBundle);
        jarOutputStream.putNextEntry(zipEntry);
        resource.openStream().transferTo(jarOutputStream);
        jarOutputStream.closeEntry();
    private static List<String> collectFilesFrom(Bundle bundle, String bundlePath, String bundleName,
        URL url = getBaseURL(bundle, bundleName);
            String path = url.getPath();
            URI baseURI = url.toURI();
            List<URL> list = collectEntries(bundle, path, extensionsToInclude);
            for (URL entryURL : list) {
                String fileEntry = convertToFileEntry(baseURI, entryURL);
                result.add(fileEntry);
    private static URL getBaseURL(Bundle bundle, String bundleName) {
        Enumeration<URL> entries = bundle.findEntries("/", bundleName, true);
        return entries != null ? entries.nextElement() : null;
    private static List<URL> collectEntries(Bundle bundle, String path, Set<String> extensionsToInclude) {
        List<URL> result = new ArrayList<>();
        for (String filePattern : extensionsToInclude) {
            Enumeration<URL> entries = bundle.findEntries(path, filePattern, true);
            if (entries != null) {
                result.addAll(Collections.list(entries));
    private static String convertToFileEntry(URI baseURI, URL entryURL) throws URISyntaxException {
        URI entryURI = entryURL.toURI();
        URI relativeURI = baseURI.relativize(entryURI);
        return relativeURI.toString();
    private static Manifest getManifest(Bundle bundle, String bundlePath) throws IOException {
        String filePath = bundlePath + "META-INF/MANIFEST.MF";
        return new Manifest(resource.openStream());
