import static org.openhab.core.addon.Addon.CODE_MATURITY_LEVELS;
import java.io.Reader;
import java.net.URLConnection;
import java.util.Date;
import java.util.Optional;
import org.openhab.core.addon.marketplace.AbstractRemoteAddonService;
import org.openhab.core.addon.marketplace.BundleVersion;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseCategoryResponseDTO;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseCategoryResponseDTO.DiscoursePosterInfo;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseCategoryResponseDTO.DiscourseTopicItem;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseCategoryResponseDTO.DiscourseUser;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseTopicResponseDTO;
import org.openhab.core.addon.marketplace.internal.community.model.DiscourseTopicResponseDTO.DiscoursePostLink;
import org.osgi.framework.Constants;
import org.osgi.service.component.annotations.Modified;
 * This class is an {@link org.openhab.core.addon.AddonService} retrieving posts on community.openhab.org (Discourse).
 * @author Yannick Schaus - Initial contribution
@Component(immediate = true, configurationPid = CommunityMarketplaceAddonService.SERVICE_PID, //
        property = Constants.SERVICE_PID + "="
                + CommunityMarketplaceAddonService.SERVICE_PID, service = AddonService.class)
@ConfigurableService(category = "system", label = CommunityMarketplaceAddonService.SERVICE_NAME, description_uri = CommunityMarketplaceAddonService.CONFIG_URI)
public class CommunityMarketplaceAddonService extends AbstractRemoteAddonService {
    public static final String CODE_CONTENT_SUFFIX = "_content";
    public static final String JSON_CONTENT_PROPERTY = "json" + CODE_CONTENT_SUFFIX;
    public static final String YAML_CONTENT_PROPERTY = "yaml" + CODE_CONTENT_SUFFIX;
    // constants for the configuration properties
    static final String SERVICE_NAME = "Community Marketplace";
    static final String SERVICE_PID = "org.openhab.marketplace";
    static final String CONFIG_URI = "system:marketplace";
    static final String CONFIG_API_KEY = "apiKey";
    static final String CONFIG_SHOW_UNPUBLISHED_ENTRIES_KEY = "showUnpublished";
    static final String CONFIG_ENABLED_KEY = "enable";
    private static final String COMMUNITY_BASE_URL = "https://community.openhab.org";
    private static final String COMMUNITY_MARKETPLACE_URL = COMMUNITY_BASE_URL + "/c/marketplace/69/l/latest";
    private static final String COMMUNITY_TOPIC_URL = COMMUNITY_BASE_URL + "/t/";
    private static final Pattern BUNDLE_NAME_PATTERN = Pattern.compile(".*/(.*?)-\\d+\\.\\d+\\.\\d+.*");
    private static final String SERVICE_ID = "marketplace";
    private static final Pattern CODE_MARKUP_PATTERN = Pattern.compile(
            "<pre(?: data-code-wrap=\"[a-z]+\")?><code class=\"lang-(?<lang>[a-z]+)\">(?<content>.*?)</code></pre>",
            Pattern.DOTALL);
    private static final Integer BUNDLES_CATEGORY = 73;
    private static final Integer RULETEMPLATES_CATEGORY = 74;
    private static final Integer UIWIDGETS_CATEGORY = 75;
    private static final Integer BLOCKLIBRARIES_CATEGORY = 76;
    private static final Integer TRANSFORMATIONS_CATEGORY = 80;
    private static final String PUBLISHED_TAG = "published";
    private final Logger logger = LoggerFactory.getLogger(CommunityMarketplaceAddonService.class);
    private @Nullable String apiKey = null;
    private boolean showUnpublished = false;
    private boolean enabled = true;
    public CommunityMarketplaceAddonService(final @Reference EventPublisher eventPublisher,
            @Reference ConfigurationAdmin configurationAdmin, @Reference StorageService storageService,
            @Reference AddonInfoRegistry addonInfoRegistry, Map<String, Object> config) {
        super(eventPublisher, configurationAdmin, storageService, addonInfoRegistry, SERVICE_PID);
        modified(config);
    @Modified
    public void modified(@Nullable Map<String, Object> config) {
        if (config != null) {
            this.apiKey = (String) config.get(CONFIG_API_KEY);
            this.showUnpublished = ConfigParser.valueAsOrElse(config.get(CONFIG_SHOW_UNPUBLISHED_ENTRIES_KEY),
                    Boolean.class, false);
            this.enabled = ConfigParser.valueAsOrElse(config.get(CONFIG_ENABLED_KEY), Boolean.class, true);
    @Reference(cardinality = ReferenceCardinality.AT_LEAST_ONE, policy = ReferencePolicy.DYNAMIC)
    protected void addAddonHandler(MarketplaceAddonHandler handler) {
        this.addonHandlers.add(handler);
    protected void removeAddonHandler(MarketplaceAddonHandler handler) {
        this.addonHandlers.remove(handler);
        return SERVICE_NAME;
    protected List<Addon> getRemoteAddons() {
            return List.of();
            List<DiscourseCategoryResponseDTO> pages = new ArrayList<>();
            URL url = URI.create(COMMUNITY_MARKETPLACE_URL).toURL();
            int pageNb = 1;
            while (url != null) {
                URLConnection connection = url.openConnection();
                connection.addRequestProperty("Accept", "application/json");
                if (this.apiKey != null) {
                    connection.addRequestProperty("Api-Key", this.apiKey);
                try (Reader reader = new InputStreamReader(connection.getInputStream())) {
                    DiscourseCategoryResponseDTO parsed = gson.fromJson(reader, DiscourseCategoryResponseDTO.class);
                    if (parsed.topicList.topics.length != 0) {
                        pages.add(parsed);
                    if (parsed.topicList.moreTopicsUrl != null) {
                        // Discourse URL for next page is wrong
                        url = URI.create(COMMUNITY_MARKETPLACE_URL + "?page=" + pageNb++).toURL();
                        url = null;
            List<DiscourseUser> users = pages.stream().flatMap(p -> Stream.of(p.users)).toList();
            pages.stream().flatMap(p -> Stream.of(p.topicList.topics)).filter(t -> showUnpublished
                    || (t.tags != null && Arrays.stream(t.tags).anyMatch(tag -> PUBLISHED_TAG.equals(tag.name))))
                    .map(t -> Optional.ofNullable(convertTopicItemToAddon(t, users)))
                    .forEach(a -> a.ifPresent(addons::add));
            logger.warn("Unable to retrieve marketplace add-ons: {}", e.getMessage());
        return addons;
        String queryId = uid.startsWith(ADDON_ID_PREFIX) ? uid : ADDON_ID_PREFIX + uid;
        // check if it is an installed add-on (cachedAddons also contains possibly incomplete results from the remote
        // side, we need to retrieve them from Discourse)
        if (installedAddonIds.contains(queryId)) {
            return cachedAddons.stream().filter(e -> queryId.equals(e.getUid())).findAny().orElse(null);
        if (!remoteEnabled()) {
        // retrieve from remote
            URL url = URI.create(COMMUNITY_TOPIC_URL + uid.replace(ADDON_ID_PREFIX, "")).toURL();
                DiscourseTopicResponseDTO parsed = gson.fromJson(reader, DiscourseTopicResponseDTO.class);
                return convertTopicToAddon(parsed);
    public @Nullable String getAddonId(URI addonURI) {
        if (addonURI.toString().startsWith(COMMUNITY_TOPIC_URL)) {
            return addonURI.toString().substring(0, addonURI.toString().indexOf("/", COMMUNITY_BASE_URL.length()));
    private @Nullable AddonType getAddonType(@Nullable Integer category, List<String> tags) {
        // check if we can determine the addon type from the category
        if (TRANSFORMATIONS_CATEGORY.equals(category)) {
            return AddonType.TRANSFORMATION;
        } else if (RULETEMPLATES_CATEGORY.equals(category)) {
            return AddonType.AUTOMATION;
        } else if (UIWIDGETS_CATEGORY.equals(category)) {
            return AddonType.UI;
        } else if (BLOCKLIBRARIES_CATEGORY.equals(category)) {
        } else if (BUNDLES_CATEGORY.equals(category)) {
            // try to get it from tags if we have tags
            return AddonType.DEFAULT_TYPES.stream().filter(type -> tags.contains(type.getId())).findFirst()
                    .orElse(null);
        // or return null
    private String getContentType(@Nullable Integer category, List<String> tags) {
            return TRANSFORMATIONS_CONTENT_TYPE;
            return RULETEMPLATES_CONTENT_TYPE;
            return UIWIDGETS_CONTENT_TYPE;
            return BLOCKLIBRARIES_CONTENT_TYPE;
            if (tags.contains("kar")) {
                return KAR_CONTENT_TYPE;
                // default to plain jar bundle for addons
                return JAR_CONTENT_TYPE;
        // empty string if content type could not be defined
     * Transforms a {@link DiscourseTopicItem} to an {@link Addon}
     * @param topic the topic
     * @return the list item
    private @Nullable Addon convertTopicItemToAddon(DiscourseTopicItem topic, List<DiscourseUser> users) {
            List<String> tags = topic.tags == null ? List.of() : Arrays.stream(topic.tags).map(t -> t.name).toList();
            String uid = ADDON_ID_PREFIX + topic.id.toString();
            AddonType addonType = getAddonType(topic.categoryId, tags);
            if (addonType == null) {
                logger.debug("Ignoring topic '{}' because no add-on type could be found", topic.id);
            String type = addonType.getId();
            String id = topic.id.toString(); // this will be replaced after installation by the correct id if available
            String contentType = getContentType(topic.categoryId, tags);
            String title = topic.title;
            boolean compatible = true;
            int compatibilityStart = topic.title.lastIndexOf("["); // version range always starts with [
            if (topic.title.lastIndexOf(" ") < compatibilityStart) { // check includes [ not present
                String potentialRange = topic.title.substring(compatibilityStart);
                Matcher matcher = BundleVersion.RANGE_PATTERN.matcher(potentialRange);
                        compatible = coreVersion.inRange(potentialRange);
                        title = topic.title.substring(0, compatibilityStart).trim();
                        logger.debug("{} is {}compatible with core version {}", topic.title, compatible ? "" : "NOT ",
                                coreVersion);
                        logger.debug("Failed to determine compatibility for addon {}: {}", topic.title, e.getMessage());
                        compatible = true;
                    logger.debug("Range pattern does not match '{}'", potentialRange);
            String link = COMMUNITY_TOPIC_URL + topic.id.toString();
            int likeCount = topic.likeCount;
            int views = topic.views;
            int postsCount = topic.postsCount;
            Date createdDate = topic.createdAt;
            String author = "";
            for (DiscoursePosterInfo posterInfo : topic.posters) {
                if (posterInfo.description.contains("Original Poster")) {
                    author = users.stream().filter(u -> u.id.equals(posterInfo.userId)).findFirst().get().name;
            String maturity = tags.stream().filter(CODE_MATURITY_LEVELS::contains).findAny().orElse(null);
            Map<String, Object> properties = Map.of("created_at", createdDate, //
                    "like_count", likeCount, //
                    "views", views, //
                    "posts_count", postsCount, //
                    "tags", tags.toArray(String[]::new));
            // try to use a handler to determine if the add-on is installed
            boolean installed = addonHandlers.stream()
                    .anyMatch(handler -> handler.supports(type, contentType) && handler.isInstalled(uid));
            return Addon.create(uid).withType(type).withId(id).withContentType(contentType)
                    .withImageLink(topic.imageUrl).withAuthor(author).withProperties(properties).withLabel(title)
                    .withInstalled(installed).withMaturity(maturity).withCompatible(compatible).withLink(link).build();
        } catch (RuntimeException e) {
            logger.debug("Ignoring marketplace add-on '{}' due: {}", topic.title, e.getMessage());
     * Unescapes occurrences of XML entities found in the supplied content.
     * @param content the content with potentially escaped entities
     * @return the unescaped content
    private String unescapeEntities(String content) {
        return content.replace("&quot;", "\"").replace("&amp;", "&").replace("&apos;", "'").replace("&lt;", "<")
                .replace("&gt;", ">");
     * Transforms a {@link DiscourseTopicResponseDTO} to an {@link Addon}
    private Addon convertTopicToAddon(DiscourseTopicResponseDTO topic) {
        String type = (addonType != null) ? addonType.getId() : "";
        Date createdDate = topic.postStream.posts[0].createdAt;
        Date updatedDate = topic.postStream.posts[0].updatedAt;
        Date lastPostedDate = topic.lastPosted;
        Map<String, Object> properties = new HashMap<>(10);
        properties.put("created_at", createdDate);
        properties.put("updated_at", updatedDate);
        properties.put("last_posted", lastPostedDate);
        properties.put("like_count", likeCount);
        properties.put("views", views);
        properties.put("posts_count", postsCount);
        properties.put("tags", tags.toArray(String[]::new));
        String detailedDescription = topic.postStream.posts[0].cooked;
        String id = null;
        // try to extract contents or links
        if (topic.postStream.posts[0].linkCounts != null) {
            for (DiscoursePostLink postLink : topic.postStream.posts[0].linkCounts) {
                if (postLink.url.endsWith(".jar")) {
                    properties.put(JAR_DOWNLOAD_URL_PROPERTY, postLink.url);
                    id = determineIdFromUrl(postLink.url);
                if (postLink.url.endsWith(".kar")) {
                    properties.put(KAR_DOWNLOAD_URL_PROPERTY, postLink.url);
                if (postLink.url.endsWith(".json")) {
                    properties.put(JSON_DOWNLOAD_URL_PROPERTY, postLink.url);
                if (postLink.url.endsWith(".yaml")) {
                    properties.put(YAML_DOWNLOAD_URL_PROPERTY, postLink.url);
        if (id == null) {
            id = topic.id.toString(); // this is a fallback if we couldn't find a better id
        Matcher codeMarkup = CODE_MARKUP_PATTERN.matcher(detailedDescription);
        if (codeMarkup.find()) {
            properties.put(codeMarkup.group("lang") + CODE_CONTENT_SUFFIX,
                    unescapeEntities(codeMarkup.group("content")));
        Addon.Builder builder = Addon.create(uid).withType(type).withId(id).withContentType(contentType)
                .withLabel(title).withImageLink(topic.imageUrl).withLink(COMMUNITY_TOPIC_URL + topic.id.toString())
                .withAuthor(topic.postStream.posts[0].displayUsername).withMaturity(maturity)
                .withDetailedDescription(detailedDescription).withInstalled(installed).withProperties(properties);
        return builder.build();
    private @Nullable String determineIdFromUrl(String url) {
        Matcher matcher = BUNDLE_NAME_PATTERN.matcher(url);
            String bundleName = matcher.group(1);
            return bundleName.substring(bundleName.lastIndexOf(".") + 1);
            logger.warn("Could not determine bundle name from url: {}", url);
