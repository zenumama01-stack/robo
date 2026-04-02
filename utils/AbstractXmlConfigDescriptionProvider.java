 * The {@link AbstractXmlConfigDescriptionProvider} is a concrete implementation of the
 * {@link ConfigDescriptionProvider}
 * service interface.
 * This implementation manages any {@link ConfigDescription} objects associated to specific modules. If a specific
 * module disappears, any registered {@link ConfigDescription} objects associated with that module are released.
 * @author Dennis Nobel - Added locale support
 * @author Alex Tugarev - Extended for pattern and options
 * @author Chris Jackson - Modify to use config parameter builder
 * @author Thomas Höfer - Extended for unit
 * @author Markus Rathgeb - Use ConfigI18nLocalizerService
public abstract class AbstractXmlConfigDescriptionProvider extends AbstractXmlBasedProvider<URI, ConfigDescription>
        implements ConfigDescriptionProvider {
    public synchronized Collection<ConfigDescription> getConfigDescriptions(@Nullable Locale locale) {
        return getAll(locale);
    public synchronized @Nullable ConfigDescription getConfigDescription(URI uri, @Nullable Locale locale) {
        return get(uri, locale);
    protected @Nullable ConfigDescription localize(Bundle bundle, ConfigDescription configDescription,
        ConfigI18nLocalizationService configI18nLocalizerService = getConfigI18nLocalizerService();
        return configI18nLocalizerService.getLocalizedConfigDescription(bundle, configDescription, locale);
    protected abstract ConfigI18nLocalizationService getConfigI18nLocalizerService();
