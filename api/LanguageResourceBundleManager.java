import org.openhab.core.common.osgi.ResourceBundleClassLoader;
 * The {@link LanguageResourceBundleManager} class manages all available i18n resources for one
 * specific <i>OSGi</i> bundle. Any i18n resource is searched within the {@link #RESOURCE_DIRECTORY} of the bundle and
 * <i>not</i> within the general bundle classpath. For the translation, the
 * i18n mechanism of Java ({@link ResourceBundle}) is used.
 * This implementation uses the user defined {@link ResourceBundleClassLoader} to map the bundle resource files to usual
 * URLs which the Java {@link ResourceBundle} can handle.
public class LanguageResourceBundleManager {
    /** The directory within the bundle where the resource files are searched. */
    protected static final String RESOURCE_DIRECTORY = "/OH-INF/i18n";
    /** The file pattern to filter out resource files. */
    private static final String RESOURCE_FILE_PATTERN = "*.properties";
    private LocaleProvider localeProvider;
    private ClassLoader resourceClassLoader;
    private List<String> resourceNames;
    public LanguageResourceBundleManager(LocaleProvider localeProvider, @Nullable Bundle bundle) {
            throw new IllegalArgumentException("The Bundle must not be null!");
        this.resourceClassLoader = new ResourceBundleClassLoader(bundle, RESOURCE_DIRECTORY, RESOURCE_FILE_PATTERN);
        this.resourceNames = determineResourceNames();
    public Bundle getBundle() {
        return this.bundle;
     * Releases any cached resources which were managed by this class from the {@link ResourceBundle}.
    public void clearCache() {
        ResourceBundle.clearCache(this.resourceClassLoader);
     * Returns {@code true} if the specified resource is managed by this instance
     * and therefore the according module is responsible for translations,
     * @param resource the resource to check (could be null or empty)
     * @return true if the specified resource is managed by this instance, otherwise false
    public boolean containsResource(@Nullable String resource) {
            return this.resourceNames.contains(resource);
     * Returns {@code true} if this instance and therefore the according module provides
     * resource information, otherwise {@code false}.
     * @return true if the according bundle provides resource information, otherwise false
    public boolean containsResources() {
        return !resourceNames.isEmpty();
    private List<String> determineResourceNames() {
        List<String> resourceNames = new ArrayList<>();
        Enumeration<URL> resourceFiles = this.bundle.findEntries(RESOURCE_DIRECTORY, RESOURCE_FILE_PATTERN, true);
                String baseName = resourceFileName.replaceFirst("[._]+.*", "");
                if (!resourceNames.contains(baseName)) {
                    resourceNames.add(baseName);
        return resourceNames;
     * considering the specified resource section. The resource is equal to a base name and
     * therefore it is mapped to one translation package (all files which belong to the base
     * If no translation could be found, {@code null} is returned. If the location is not specified, the default
     * location is used.
     * @param resource the resource to be used for look-up (could be null or empty)
     * @return the translated text, or null if the key could not be translated
    public @Nullable String getText(@Nullable String resource, @Nullable String key, @Nullable Locale locale) {
        if ((key != null) && (!key.isEmpty())) {
            Locale effectiveLocale = locale != null ? locale : localeProvider.getLocale();
                return getTranslatedText(resource, key, effectiveLocale);
                for (String resourceName : this.resourceNames) {
                    String text = getTranslatedText(resourceName, key, effectiveLocale);
     * Returns a translation for the specified key in the specified locale (language)
     * by considering all resources in the according bundle.
    public @Nullable String getText(@Nullable String key, @Nullable Locale locale) {
        return getText(null, key, locale);
    private @Nullable String getTranslatedText(@Nullable String resourceName, @Nullable String key,
        if (resourceName != null && locale != null && key != null && !key.isEmpty()) {
                // Modify the search order so that the following applies:
                // 1.) baseName + "_" + language + "_" + country
                // 2.) baseName + "_" + language
                // 3.) baseName
                // 4.) null -> leads to a default text
                // Not using the default fallback strategy helps that not the default locale
                // search order is applied between 2.) and 3.).
                ResourceBundle resourceBundle = ResourceBundle.getBundle(resourceName, locale, this.resourceClassLoader,
                return resourceBundle.getString(key);
            } catch (NullPointerException | IllegalArgumentException | MissingResourceException ex) {
