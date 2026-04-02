 * The {@link TransformationRegistryImpl} implements the {@link TransformationRegistry}
public class TransformationRegistryImpl extends AbstractRegistry<Transformation, String, TransformationProvider>
        implements TransformationRegistry {
            .compile("(?<filename>.+)(_(?<language>[a-z]{2}))?\\.(?<extension>[^.]*)$");
    public TransformationRegistryImpl(@Reference LocaleProvider localeProvider) {
        super(TransformationProvider.class);
    public @Nullable Transformation get(String uid, @Nullable Locale locale) {
        Transformation configuration = null;
        String language = Objects.requireNonNullElse(locale, localeProvider.getLocale()).getLanguage();
        Matcher uidMatcher = CONFIG_UID_PATTERN.matcher(uid);
        if (uidMatcher.matches()) {
            // try to get localized version of the uid if no locale information is present
            if (uidMatcher.group("language") == null) {
                configuration = get(uid + ":" + language);
            // check if legacy configuration and try to get localized version
            uidMatcher = FILENAME_PATTERN.matcher(uid);
            if (uidMatcher.matches() && uidMatcher.group("language") == null) {
                // try to get a localized version
                String localizedUid = uidMatcher.group("filename") + "_" + language + "."
                        + uidMatcher.group("extension");
                configuration = get(localizedUid);
        return (configuration != null) ? configuration : get(uid);
    public Collection<Transformation> getTransformations(Collection<String> types) {
        return getAll().stream().filter(e -> types.contains(e.getType())).toList();
    protected void setManagedProvider(ManagedTransformationProvider provider) {
    protected void unsetManagedProvider(ManagedTransformationProvider provider) {
    protected void addProvider(Provider<Transformation> provider) {
        // overridden to make method available for testing
