package org.openhab.core.addon;
 * This class defines an add-on.
 * @author Kai Kreuzer - Initial contribution
 * @author Yannick Schaus - Add fields
public class Addon {
    public static final Set<String> CODE_MATURITY_LEVELS = Set.of("alpha", "beta", "mature", "stable");
    public static final String ADDON_SEPARATOR = "-";
    private final String uid;
    private final String id;
    private final String label;
    private final @Nullable String maturity;
    private final boolean compatible;
    private final String contentType;
    private final @Nullable String link;
    private final String author;
    private final boolean verifiedAuthor;
    private boolean installed;
    private final String type;
    private final @Nullable String description;
    private final @Nullable String detailedDescription;
    private final String configDescriptionURI;
    private final String keywords;
    private final List<String> countries;
    private final @Nullable String license;
    private final String connection;
    private final @Nullable String backgroundColor;
    private final @Nullable String imageLink;
    private final Map<String, Object> properties;
    private final List<String> loggerPackages;
     * Creates a new Addon instance
     * @param uid the id of the add-on (e.g. "binding-dmx", "json:transform-format" or "marketplace:123456")
     * @param type the type id of the add-on (e.g. "automation")
     * @param uid the technical name of the add-on (e.g. "influxdb")
     * @param label the label of the add-on
     * @param version the version of the add-on
     * @param maturity the maturity level of this version
     * @param compatible if this add-on is compatible with the current core version
     * @param contentType the content type of the add-on
     * @param link the link to find more information about the add-on (may be null)
     * @param author the author of the add-on
     * @param verifiedAuthor true, if the author is verified
     * @param installed true, if the add-on is installed, false otherwise
     * @param description the description of the add-on (may be null)
     * @param detailedDescription the detailed description of the add-on (may be null)
     * @param configDescriptionURI the URI to the configuration description for this add-on
     * @param keywords the keywords for this add-on
     * @param countries a list of ISO 3166 codes relevant to this add-on
     * @param license the SPDX license identifier
     * @param connection a string describing the type of connection (local or cloud, push or pull...) this add-on uses,
     *            if applicable.
     * @param backgroundColor for displaying the add-on (may be null)
     * @param imageLink the link to an image (png/svg) (may be null)
     * @param properties a {@link Map} containing addition information
     * @param loggerPackages a {@link List} containing the package names belonging to this add-on
     * @throws IllegalArgumentException when a mandatory parameter is invalid
    private Addon(String uid, String type, String id, String label, String version, @Nullable String maturity,
            boolean compatible, String contentType, @Nullable String link, String author, boolean verifiedAuthor,
            boolean installed, @Nullable String description, @Nullable String detailedDescription,
            String configDescriptionURI, String keywords, List<String> countries, @Nullable String license,
            String connection, @Nullable String backgroundColor, @Nullable String imageLink,
            @Nullable Map<String, Object> properties, List<String> loggerPackages) {
        if (uid.isBlank()) {
            throw new IllegalArgumentException("uid must not be empty");
        if (type.isBlank()) {
            throw new IllegalArgumentException("type must not be empty");
        if (id.isBlank()) {
            throw new IllegalArgumentException("id must not be empty");
        this.uid = uid;
        this.type = type;
        this.maturity = maturity;
        this.compatible = compatible;
        this.contentType = contentType;
        this.description = description;
        this.detailedDescription = detailedDescription;
        this.configDescriptionURI = configDescriptionURI;
        this.keywords = keywords;
        this.countries = countries;
        this.license = license;
        this.connection = connection;
        this.backgroundColor = backgroundColor;
        this.link = link;
        this.imageLink = imageLink;
        this.author = author;
        this.verifiedAuthor = verifiedAuthor;
        this.installed = installed;
        this.properties = properties == null ? Map.of() : properties;
        this.loggerPackages = loggerPackages;
     * The type of the addon (same as id of {@link AddonType})
    public String getType() {
     * The uid of the add-on (e.g. "binding-dmx", "json:transform-format" or "marketplace:123456")
    public String getUid() {
        return uid;
     * The id of the add-on (e.g. "influxdb")
     * The label of the add-on
    public String getLabel() {
     * The (optional) link to find more information about the add-on
    public @Nullable String getLink() {
        return link;
     * The author of the add-on
    public String getAuthor() {
        return author;
     * Whether the add-on author is verified or not
    public boolean isVerifiedAuthor() {
        return verifiedAuthor;
     * The version of the add-on
    public String getVersion() {
     * The maturity level of this version
    public @Nullable String getMaturity() {
        return maturity;
     * The (expected) compatibility of this add-on
    public boolean getCompatible() {
     * The content type of the add-on
    public String getContentType() {
        return contentType;
     * The description of the add-on
    public @Nullable String getDescription() {
     * The detailed description of the add-on
    public @Nullable String getDetailedDescription() {
        return detailedDescription;
     * The URI to the configuration description for this add-on
    public String getConfigDescriptionURI() {
        return configDescriptionURI;
     * The keywords for this add-on
    public String getKeywords() {
     * A list of ISO 3166 codes relevant to this add-on
    public List<String> getCountries() {
        return countries;
     * The SPDX License identifier for this addon
    public @Nullable String getLicense() {
        return license;
     * A string describing the type of connection (local, cloud, cloudDiscovery) this add-on uses, if applicable.
    public String getConnection() {
        return connection;
     * A set of additional properties relative to this add-on
    public Map<String, Object> getProperties() {
     * true, if the add-on is installed, false otherwise
    public boolean isInstalled() {
     * Sets the installed state
    public void setInstalled(boolean installed) {
     * The background color for rendering the add-on
    public @Nullable String getBackgroundColor() {
        return backgroundColor;
     * A link to an image (png/svg) for the add-on
    public @Nullable String getImageLink() {
        return imageLink;
     * The package names that are associated with this add-on
    public List<String> getLoggerPackages() {
        return loggerPackages;
     * Create a builder for an {@link Addon}
     * @param uid the UID of the add-on (e.g. "binding-dmx", "json:transform-format" or "marketplace:123456")
     * @return the builder
    public static Builder create(String uid) {
        return new Builder(uid);
    public static Builder create(Addon addon) {
        Addon.Builder builder = new Builder(addon.uid);
        builder.id = addon.id;
        builder.label = addon.label;
        builder.version = addon.version;
        builder.maturity = addon.maturity;
        builder.compatible = addon.compatible;
        builder.contentType = addon.contentType;
        builder.link = addon.link;
        builder.author = addon.author;
        builder.verifiedAuthor = addon.verifiedAuthor;
        builder.installed = addon.installed;
        builder.type = addon.type;
        builder.description = addon.description;
        builder.detailedDescription = addon.detailedDescription;
        builder.configDescriptionURI = addon.configDescriptionURI;
        builder.keywords = addon.keywords;
        builder.countries = addon.countries;
        builder.license = addon.license;
        builder.connection = addon.connection;
        builder.backgroundColor = addon.backgroundColor;
        builder.imageLink = addon.imageLink;
        builder.properties = addon.properties;
        builder.loggerPackages = addon.loggerPackages;
    public static class Builder {
        private String id;
        private String label;
        private String version = "";
        private @Nullable String maturity;
        private boolean compatible = true;
        private String contentType;
        private @Nullable String link;
        private String author = "";
        private boolean verifiedAuthor = false;
        private boolean installed = false;
        private String type;
        private @Nullable String description;
        private @Nullable String detailedDescription;
        private String configDescriptionURI = "";
        private String keywords = "";
        private List<String> countries = List.of();
        private @Nullable String license;
        private String connection = "";
        private @Nullable String backgroundColor;
        private @Nullable String imageLink;
        private Map<String, Object> properties = new HashMap<>();
        private List<String> loggerPackages = List.of();
        private Builder(String uid) {
        public Builder withType(String type) {
        public Builder withId(String id) {
        public Builder withLabel(String label) {
        public Builder withVersion(String version) {
        public Builder withMaturity(@Nullable String maturity) {
        public Builder withCompatible(boolean compatible) {
        public Builder withContentType(String contentType) {
        public Builder withLink(String link) {
        public Builder withAuthor(@Nullable String author) {
            this.author = Objects.requireNonNullElse(author, "");
        public Builder withAuthor(String author, boolean verifiedAuthor) {
        public Builder withInstalled(boolean installed) {
        public Builder withDescription(String description) {
        public Builder withDetailedDescription(String detailedDescription) {
        public Builder withConfigDescriptionURI(@Nullable String configDescriptionURI) {
            this.configDescriptionURI = Objects.requireNonNullElse(configDescriptionURI, "");
        public Builder withKeywords(String keywords) {
        public Builder withCountries(List<String> countries) {
        public Builder withLicense(@Nullable String license) {
        public Builder withConnection(String connection) {
        public Builder withBackgroundColor(String backgroundColor) {
        public Builder withImageLink(@Nullable String imageLink) {
        public Builder withProperty(String key, Object value) {
            this.properties.put(key, value);
        public Builder withProperties(Map<String, Object> properties) {
            this.properties.putAll(properties);
        public Builder withLoggerPackages(List<String> loggerPackages) {
        public Addon build() {
            return new Addon(uid, type, id, label, version, maturity, compatible, contentType, link, author,
                    verifiedAuthor, installed, description, detailedDescription, configDescriptionURI, keywords,
                    countries, license, connection, backgroundColor, imageLink,
                    properties.isEmpty() ? null : properties, loggerPackages);
