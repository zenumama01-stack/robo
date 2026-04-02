import org.openhab.core.common.registry.Identifiable;
 * The {@link AddonInfo} class contains general information about an add-on.
 * <p>
 * Any add-on information is provided by a {@link AddonInfoProvider} and can also be retrieved through the
 * {@link AddonInfoRegistry}.
 * @author Michael Grammling - Initial contribution
 * @author Andre Fuechsel - Made author tag optional
 * @author Jan N. Klug - Refactored to cover all add-ons
public class AddonInfo implements Identifiable<String> {
    private static final Set<String> SUPPORTED_ADDON_TYPES = Set.of("automation", "binding", "misc", "persistence",
    private final String name;
    private final String description;
    private final @Nullable String keywords;
    private final @Nullable String connection;
    private final @Nullable String configDescriptionURI;
    private final String serviceId;
    private @Nullable String sourceBundle;
    private @Nullable List<AddonDiscoveryMethod> discoveryMethods;
    private AddonInfo(String id, String type, @Nullable String uid, String name, String description,
            @Nullable String keywords, @Nullable String connection, List<String> countries,
            @Nullable String configDescriptionURI, @Nullable String serviceId, @Nullable String sourceBundle,
            @Nullable List<AddonDiscoveryMethod> discoveryMethods) throws IllegalArgumentException {
        // mandatory fields
            throw new IllegalArgumentException("The ID must neither be null nor empty!");
        if (!SUPPORTED_ADDON_TYPES.contains(type)) {
            throw new IllegalArgumentException(
                    "The type must be one of [" + String.join(", ", SUPPORTED_ADDON_TYPES) + "]");
        if (name.isBlank()) {
            throw new IllegalArgumentException("The name must neither be null nor empty!");
        if (description.isBlank()) {
            throw new IllegalArgumentException("The description must neither be null nor empty!");
        this.uid = uid != null ? uid : type + Addon.ADDON_SEPARATOR + id;
        // optional fields
        this.serviceId = Objects.requireNonNullElse(serviceId, type + "." + id);
        this.sourceBundle = sourceBundle;
        this.discoveryMethods = discoveryMethods;
     * Returns an unique identifier for the add-on (e.g. "binding-hue").
     * @return an identifier for the add-on
    public String getUID() {
     * Returns the id part of the UID
     * @return the identifier
     * Returns a human-readable name for the add-on (e.g. "HUE Binding").
     * @return a human-readable name for the add-on (neither null, nor empty)
    public String getServiceId() {
        return serviceId;
     * Returns a human-readable description for the add-on
     * (e.g. "Discovers and controls HUE bulbs").
     * @return a human-readable description for the add-on
    public String getDescription() {
     * Returns a comma-separated list of keywords related to the add-on. e.g. "bluetooth".
     * @return a comma-separated list of keywords, or null if no keywords string available
    public @Nullable String getKeywords() {
     * Returns the link to a concrete {@link org.openhab.core.config.core.ConfigDescription}.
     * @return the link to a concrete ConfigDescription (could be <code>null</code>>)
    public @Nullable String getConfigDescriptionURI() {
    public @Nullable String getSourceBundle() {
        return sourceBundle;
    public @Nullable String getConnection() {
    public List<AddonDiscoveryMethod> getDiscoveryMethods() {
        List<AddonDiscoveryMethod> discoveryMethods = this.discoveryMethods;
        return discoveryMethods != null ? discoveryMethods : List.of();
    public static Builder builder(String id, String type) {
        return new Builder(id, type);
    public static Builder builder(AddonInfo addonInfo) {
        return new Builder(addonInfo);
        private @Nullable String uid;
        private String name = "";
        private String description = "";
        private @Nullable String keywords;
        private @Nullable String connection;
        private @Nullable String configDescriptionURI = "";
        private @Nullable String serviceId;
        private Builder(String id, String type) {
        private Builder(AddonInfo addonInfo) {
            this.id = addonInfo.id;
            this.type = addonInfo.type;
            this.uid = addonInfo.uid;
            this.name = addonInfo.name;
            this.description = addonInfo.description;
            this.keywords = addonInfo.keywords;
            this.connection = addonInfo.connection;
            this.countries = addonInfo.countries;
            this.configDescriptionURI = addonInfo.configDescriptionURI;
            this.serviceId = addonInfo.serviceId;
            this.sourceBundle = addonInfo.sourceBundle;
            this.discoveryMethods = addonInfo.discoveryMethods;
        public Builder withUID(@Nullable String uid) {
        public Builder withName(String name) {
        public Builder withKeywords(@Nullable String keywords) {
        public Builder withConnection(@Nullable String connection) {
        public Builder withCountries(@Nullable String countries) {
            this.countries = countries == null || countries.isBlank() ? List.of() : List.of(countries.split(","));
        public Builder withServiceId(@Nullable String serviceId) {
            this.serviceId = serviceId;
        public Builder withSourceBundle(@Nullable String sourceBundle) {
        public Builder withDiscoveryMethods(@Nullable List<AddonDiscoveryMethod> discoveryMethods) {
         * Build an {@link AddonInfo} from this builder
         * @return the add-on info object
         * @throws IllegalArgumentException if any of the information in this builder is invalid
        public AddonInfo build() throws IllegalArgumentException {
            return new AddonInfo(id, type, uid, name, description, keywords, connection, countries,
                    configDescriptionURI, serviceId, sourceBundle, discoveryMethods);
