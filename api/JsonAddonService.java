package org.openhab.core.addon.marketplace.internal.json;
import java.lang.reflect.Type;
import org.openhab.core.addon.marketplace.internal.json.model.AddonEntryDTO;
import com.google.gson.reflect.TypeToken;
 * This class implements an {@link org.openhab.core.addon.AddonService} retrieving JSON marketplace information.
 * @author Jan N. Klug - Refactored for JSON marketplaces
@Component(immediate = true, configurationPid = JsonAddonService.SERVICE_PID, //
        property = Constants.SERVICE_PID + "=" + JsonAddonService.SERVICE_PID, service = AddonService.class)
@ConfigurableService(category = "system", label = JsonAddonService.SERVICE_NAME, description_uri = JsonAddonService.CONFIG_URI)
public class JsonAddonService extends AbstractRemoteAddonService {
    static final String SERVICE_NAME = "Json 3rd Party Add-on Service";
    static final String CONFIG_URI = "system:jsonaddonservice";
    static final String SERVICE_PID = "org.openhab.jsonaddonservice";
    private static final String SERVICE_ID = "json";
    private static final String CONFIG_URLS = "urls";
    private static final String CONFIG_SHOW_UNSTABLE = "showUnstable";
    private final Logger logger = LoggerFactory.getLogger(JsonAddonService.class);
    private List<String> addonServiceUrls = List.of();
    private boolean showUnstable = false;
    public JsonAddonService(@Reference EventPublisher eventPublisher, @Reference StorageService storageService,
            @Reference ConfigurationAdmin configurationAdmin, @Reference AddonInfoRegistry addonInfoRegistry,
            Map<String, Object> config) {
            String urls = ConfigParser.valueAsOrElse(config.get(CONFIG_URLS), String.class, "");
            addonServiceUrls = Arrays.asList(urls.split("\\|")).stream().filter(this::isValidUrl).toList();
            showUnstable = ConfigParser.valueAsOrElse(config.get(CONFIG_SHOW_UNSTABLE), Boolean.class, false);
    private boolean isValidUrl(String urlString) {
        if (urlString.isBlank()) {
            (new URI(urlString)).toURL();
        } catch (IllegalArgumentException | URISyntaxException | MalformedURLException e) {
            logger.warn("JSON Addon Service invalid URL: {}", urlString);
    @SuppressWarnings("unchecked")
        return addonServiceUrls.stream().map(urlString -> {
                URL url = URI.create(urlString).toURL();
                    Type type = TypeToken.getParameterized(List.class, AddonEntryDTO.class).getType();
                    return (List<AddonEntryDTO>) Objects.requireNonNull(gson.fromJson(reader, type));
        }).flatMap(List::stream).filter(Objects::nonNull).map(e -> (AddonEntryDTO) e).filter(this::showAddon)
                .map(this::fromAddonEntry).toList();
     * Check if the addon UID is present and the entry is either stable or unstable add-ons are requested
     * @param addonEntry the add-on entry to check
     * @return {@code true} if the add-on entry should be processed, {@code false otherwise}
    private boolean showAddon(AddonEntryDTO addonEntry) {
        if (addonEntry.uid.isBlank()) {
            logger.debug("Skipping {} because the UID is not set", addonEntry);
        if (!showUnstable && !"stable".equals(addonEntry.maturity)) {
            logger.debug("Skipping {} because the the add-on is not stable and showUnstable is disabled.", addonEntry);
    public @Nullable Addon getAddon(String id, @Nullable Locale locale) {
        String queryId = id.startsWith(ADDON_ID_PREFIX) ? id : ADDON_ID_PREFIX + id;
    private Addon fromAddonEntry(AddonEntryDTO addonEntry) {
        String uid = ADDON_ID_PREFIX + addonEntry.uid;
        boolean installed = addonHandlers.stream().anyMatch(
                handler -> handler.supports(addonEntry.type, addonEntry.contentType) && handler.isInstalled(uid));
        Map<String, Object> properties = new HashMap<>();
        if (addonEntry.url.endsWith(".jar")) {
            properties.put(JAR_DOWNLOAD_URL_PROPERTY, addonEntry.url);
        } else if (addonEntry.url.endsWith(".kar")) {
            properties.put(KAR_DOWNLOAD_URL_PROPERTY, addonEntry.url);
        } else if (addonEntry.url.endsWith(".json")) {
            properties.put(JSON_DOWNLOAD_URL_PROPERTY, addonEntry.url);
        } else if (addonEntry.url.endsWith(".yaml")) {
            properties.put(YAML_DOWNLOAD_URL_PROPERTY, addonEntry.url);
            compatible = coreVersion.inRange(addonEntry.compatibleVersions);
            logger.debug("Failed to determine compatibility for addon {}: {}", addonEntry.id, e.getMessage());
        return Addon.create(uid).withType(addonEntry.type).withId(addonEntry.id).withInstalled(installed)
                .withDetailedDescription(addonEntry.description).withContentType(addonEntry.contentType)
                .withAuthor(addonEntry.author).withVersion(addonEntry.version).withLabel(addonEntry.title)
                .withCompatible(compatible).withMaturity(addonEntry.maturity).withProperties(properties)
                .withLink(addonEntry.link).withImageLink(addonEntry.imageUrl).withKeywords(addonEntry.keywords)
                .withConfigDescriptionURI(addonEntry.configDescriptionURI).withLoggerPackages(addonEntry.loggerPackages)
                .withConnection(addonEntry.connection).withCountries(addonEntry.countries).build();
