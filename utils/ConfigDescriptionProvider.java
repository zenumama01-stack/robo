 * The {@link ConfigDescriptionProvider} can be implemented and registered as an <i>OSGi</i>
 * service to provide {@link ConfigDescription}s.
public interface ConfigDescriptionProvider {
     * Provides a collection of {@link ConfigDescription}s.
     * @param locale locale
     * @return the configuration descriptions provided by this provider (not
     *         null, could be empty)
    Collection<ConfigDescription> getConfigDescriptions(@Nullable Locale locale);
     * Provides a {@link ConfigDescription} for the given URI.
     * @param uri uri of the config description
     * @return config description or null if no config description could be found
    ConfigDescription getConfigDescription(URI uri, @Nullable Locale locale);
