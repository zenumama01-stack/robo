import java.util.function.BinaryOperator;
import java.util.function.Function;
 * The {@link AddonInfoRegistry} provides access to {@link AddonInfo} objects.
 * It tracks {@link AddonInfoProvider} <i>OSGi</i> services to collect all {@link AddonInfo} objects.
 * @author Dennis Nobel - Initial contribution
 * @author Michael Grammling - Initial contribution, added locale support
@Component(immediate = true, service = AddonInfoRegistry.class)
public class AddonInfoRegistry {
    private final Collection<AddonInfoProvider> addonInfoProviders = new CopyOnWriteArrayList<>();
    @Reference(cardinality = ReferenceCardinality.MULTIPLE, policy = ReferencePolicy.DYNAMIC)
    protected void addAddonInfoProvider(AddonInfoProvider addonInfoProvider) {
        addonInfoProviders.add(addonInfoProvider);
    public void removeAddonInfoProvider(AddonInfoProvider addonInfoProvider) {
        addonInfoProviders.remove(addonInfoProvider);
     * Returns the add-on information for the specified add-on UID, or {@code null} if no add-on information could be
     * found.
     * @param uid the UID to be looked
     * @return a add-on information object (could be null)
    public @Nullable AddonInfo getAddonInfo(String uid) {
        return getAddonInfo(uid, null);
     * Returns the add-on information for the specified add-on UID and locale (language),
     * or {@code null} if no add-on information could be found.
     * If more than one provider provides information for the specified add-on UID and locale,
     * it returns a new {@link AddonInfo} containing merged information from all such providers.
     * @param uid the UID to be looked for
     * @param locale the locale to be used for the add-on information (could be null)
     * @return a localized add-on information object (could be null)
    public @Nullable AddonInfo getAddonInfo(String uid, @Nullable Locale locale) {
        return addonInfoProviders.stream().map(p -> p.getAddonInfo(uid, locale)).filter(Objects::nonNull)
                .collect(Collectors.toMap(a -> a.getUID(), Function.identity(), mergeAddonInfos)).get(uid);
     * A {@link BinaryOperator} to merge the field values from two {@link AddonInfo} objects into a third such object.
     * If the first object has a non-null field value the result object takes the first value, or if the second object
     * has a non-null field value the result object takes the second value. Otherwise the field remains null.
     * @param a the first {@link AddonInfo} (could be null)
     * @param b the second {@link AddonInfo} (could be null)
     * @return a new {@link AddonInfo} containing the combined field values (could be null)
    private static BinaryOperator<@Nullable AddonInfo> mergeAddonInfos = (a, b) -> {
        if (a == null) {
        } else if (b == null) {
        AddonInfo.Builder builder = AddonInfo.builder(a);
        if (a.getDescription().isBlank()) {
            builder.withDescription(b.getDescription());
        Set<String> keywords = new HashSet<>();
        if (a.getKeywords() instanceof String ka) {
            Arrays.stream(ka.split(",")).map(String::trim).filter(s -> !s.isEmpty()).forEach(keywords::add);
        if (b.getKeywords() instanceof String kb) {
            Arrays.stream(kb.split(",")).map(String::trim).filter(s -> !s.isEmpty()).forEach(keywords::add);
        if (!keywords.isEmpty()) {
            builder.withKeywords(keywords.stream().collect(Collectors.joining(",")));
        if (a.getConnection() == null && b.getConnection() != null) {
            builder.withConnection(b.getConnection());
        Set<String> countries = new HashSet<>(a.getCountries());
        countries.addAll(b.getCountries());
        if (!countries.isEmpty()) {
            builder.withCountries(countries.stream().toList());
        String aConfigDescriptionURI = a.getConfigDescriptionURI();
        if (aConfigDescriptionURI == null || aConfigDescriptionURI.isEmpty() && b.getConfigDescriptionURI() != null) {
            builder.withConfigDescriptionURI(b.getConfigDescriptionURI());
        if (a.getSourceBundle() == null && b.getSourceBundle() != null) {
            builder.withSourceBundle(b.getSourceBundle());
        String defaultServiceId = a.getType() + "." + a.getId();
        if (defaultServiceId.equals(a.getServiceId()) && !defaultServiceId.equals(b.getServiceId())) {
            builder.withServiceId(b.getServiceId());
        String defaultUID = a.getType() + Addon.ADDON_SEPARATOR + a.getId();
        if (defaultUID.equals(a.getUID()) && !defaultUID.equals(b.getUID())) {
            builder.withUID(b.getUID());
        Set<AddonDiscoveryMethod> discoveryMethods = new HashSet<>(a.getDiscoveryMethods());
        discoveryMethods.addAll(b.getDiscoveryMethods());
        if (!discoveryMethods.isEmpty()) {
            builder.withDiscoveryMethods(discoveryMethods.stream().toList());
     * Returns all add-on information this registry contains.
     * @return a set of all add-on information this registry contains (not null, could be empty)
    public Set<AddonInfo> getAddonInfos() {
        return getAddonInfos(null);
     * Returns all add-on information in the specified locale (language) this registry contains.
     * @return a localized set of all add-on information this registry contains
     *         (not null, could be empty)
    public Set<AddonInfo> getAddonInfos(@Nullable Locale locale) {
        return addonInfoProviders.stream().map(provider -> provider.getAddonInfos(locale)).flatMap(Set::stream)
                .collect(Collectors.toUnmodifiableSet());
