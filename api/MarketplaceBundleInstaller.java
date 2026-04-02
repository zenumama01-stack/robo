import java.io.FileInputStream;
import org.osgi.framework.BundleException;
 * Handle the management of bundles related to marketplace add-ons that resists OSGi cache cleanups.
 * These operations cache incoming bundle files locally in a structure under the user data folder, and can make sure the
 * bundles are re-installed if they are present in the local cache but not installed in the OSGi framework.
 * They can be used by marketplace handler implementations dealing with OSGi bundles.
 * @author Yannick Schaus - Initial contribution and API
public abstract class MarketplaceBundleInstaller {
    private final Logger logger = LoggerFactory.getLogger(MarketplaceBundleInstaller.class);
    private static final Path BUNDLE_CACHE_PATH = Path.of(OpenHAB.getUserDataFolder(), "marketplace", "bundles");
     * Downloads a bundle file from a remote source and puts it in the local cache with the add-on ID.
     * @param sourceUrl the (online) source where the .jar file can be found
     * @throws MarketplaceHandlerException
    protected void addBundleToCache(String addonId, URL sourceUrl) throws MarketplaceHandlerException {
            throw new MarketplaceHandlerException("Cannot copy bundle to local cache: " + e.getMessage(), e);
     * Installs a bundle from its ID by looking up in the local cache
     * @param bundleContext the {@link BundleContext} to use to install the bundle
    protected void installFromCache(BundleContext bundleContext, String addonId) throws MarketplaceHandlerException {
                List<Path> bundleFiles = files.toList();
                if (bundleFiles.size() != 1) {
                try (FileInputStream fileInputStream = new FileInputStream(bundleFiles.getFirst().toFile())) {
                    Bundle bundle = bundleContext.installBundle(addonId, fileInputStream);
                        bundle.start();
                    } catch (BundleException e) {
                        logger.warn("The marketplace bundle was successfully installed but doesn't start: {}",
                                e.getMessage());
            } catch (IOException | BundleException e) {
                throw new MarketplaceHandlerException("Cannot install bundle from marketplace cache: " + e.getMessage(),
     * Determines whether a bundle associated to the given add-on ID is installed
     * @param bundleContext the {@link BundleContext} to use to look up the bundle
    protected boolean isBundleInstalled(BundleContext bundleContext, String addonId) {
        return bundleContext.getBundle(addonId) != null;
     * Uninstalls a bundle associated to the given add-on ID. Also removes it from the local cache.
    protected void uninstallBundle(BundleContext bundleContext, String addonId) throws MarketplaceHandlerException {
                    for (Path path : files.toList()) {
                        Files.delete(path);
            throw new MarketplaceHandlerException("Failed to delete bundle-files: " + e.getMessage(), e);
        if (isBundleInstalled(bundleContext, addonId)) {
            Bundle bundle = bundleContext.getBundle(addonId);
                bundle.stop();
                bundle.uninstall();
                throw new MarketplaceHandlerException("Failed uninstalling bundle: " + e.getMessage(), e);
     * Iterates over the local cache entries and re-installs bundles that are missing
     * @param bundleContext the {@link BundleContext} to use to look up the bundles
    protected void ensureCachedBundlesAreInstalled(BundleContext bundleContext) {
        if (Files.isDirectory(BUNDLE_CACHE_PATH)) {
            try (Stream<Path> files = Files.list(BUNDLE_CACHE_PATH)) {
                files.filter(Files::isDirectory).map(this::addonIdFromPath)
                        .filter(addonId -> !isBundleInstalled(bundleContext, addonId)).forEach(addonId -> {
                            logger.info("Reinstalling missing marketplace bundle: {}", addonId);
                                installFromCache(bundleContext, addonId);
                logger.warn("Failed to re-install bundles: {}", e.getMessage());
        return BUNDLE_CACHE_PATH.resolve(dir);
