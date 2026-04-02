import org.openhab.core.addon.marketplace.MarketplaceBundleInstaller;
 * A {@link MarketplaceAddonHandler} implementation, which handles add-ons as jar files (specifically, OSGi
 * bundles) and installs them through the standard OSGi bundle installation mechanism.
 * The bundles installed this way have their location set to the add-on ID to identify them and determine their
 * installation status.
public class CommunityBundleAddonHandler extends MarketplaceBundleInstaller implements MarketplaceAddonHandler {
    private final Logger logger = LoggerFactory.getLogger(CommunityBundleAddonHandler.class);
    public CommunityBundleAddonHandler(BundleContext bundleContext) {
        scheduler.execute(() -> {
            ensureCachedBundlesAreInstalled(bundleContext);
        // we support only certain extension types, and only as pure OSGi bundles
        return SUPPORTED_EXT_TYPES.contains(type) && contentType.equals(JAR_CONTENT_TYPE);
        return isBundleInstalled(bundleContext, id);
        Object urlObject = addon.getProperties().get(JAR_DOWNLOAD_URL_PROPERTY);
        if (!(urlObject instanceof String urlString)) {
            logger.error("Bundle {} has no JAR download URL", addon.getUid());
            throw new MarketplaceHandlerException("Bundle has no JAR download URL", null);
            sourceUrl = new URI(urlString).toURL();
        addBundleToCache(addon.getUid(), sourceUrl);
        installFromCache(bundleContext, addon.getUid());
        uninstallBundle(bundleContext, addon.getUid());
