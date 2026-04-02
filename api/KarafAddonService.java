 * This service is an implementation of an openHAB {@link AddonService} using the Karaf features service. This
 * exposes all openHAB add-ons through the REST API and allows UIs to dynamically install and uninstall them.
@Component(name = "org.openhab.core.karafaddons")
public class KarafAddonService implements AddonService {
    private static final String ADDONS_CONTENT_TYPE = "application/vnd.openhab.feature;type=karaf";
    private final Logger logger = LoggerFactory.getLogger(KarafAddonService.class);
    public KarafAddonService(final @Reference FeatureInstaller featureInstaller,
            final @Reference FeaturesService featuresService, @Reference AddonInfoRegistry addonInfoRegistry) {
        return "karaf";
        return "openHAB Distribution";
            return Arrays.stream(featuresService.listFeatures()).filter(this::isAddon).map(f -> getAddon(f, locale))
                    .sorted(Comparator.comparing(Addon::getLabel)).toList();
            logger.error("Exception while retrieving features: {}", e.getMessage());
    private boolean isAddon(Feature feature) {
        return feature.getName().startsWith(FeatureInstaller.PREFIX)
                && FeatureInstaller.ADDON_TYPES.contains(getAddonType(feature.getName()));
        Feature feature;
            feature = featuresService.getFeature(FeatureInstaller.PREFIX + id);
            return getAddon(feature, locale);
            logger.error("Exception while querying feature '{}'", id);
    private Addon getAddon(Feature feature, @Nullable Locale locale) {
        String name = getName(feature.getName());
        String type = getAddonType(feature.getName());
        boolean isInstalled = featuresService.isInstalled(feature);
        Addon.Builder addon = Addon.create(uid).withType(type).withId(name).withContentType(ADDONS_CONTENT_TYPE)
                .withVersion(feature.getVersion()).withAuthor(ADDONS_AUTHOR, true).withInstalled(isInstalled);
            addon = addon.withLabel(feature.getDescription()).withLink(getDefaultDocumentationLink(type, name));
        List<String> packages = feature.getBundles().stream().filter(bundle -> !bundle.isDependency()).map(bundle -> {
            String location = bundle.getLocation();
            location = location.substring(0, location.lastIndexOf("/")); // strip version
            location = location.substring(location.lastIndexOf("/") + 1); // strip groupId and protocol
        addon.withLoggerPackages(packages);
        featureInstaller.addAddon(getAddonType(id), getName(id));
        featureInstaller.removeAddon(getAddonType(id), getName(id));
    private String getAddonType(String name) {
        String str = name.startsWith(FeatureInstaller.PREFIX) ? name.substring(FeatureInstaller.PREFIX.length()) : name;
        int index = str.indexOf(Addon.ADDON_SEPARATOR);
    private String getName(String name) {
        return index == -1 ? "" : str.substring(index + Addon.ADDON_SEPARATOR.length());
