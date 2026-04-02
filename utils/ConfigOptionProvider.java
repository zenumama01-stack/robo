 * The {@link ConfigOptionProvider} can be implemented and registered as an <i>OSGi</i>
 * service to provide {@link ConfigDescription}s options.
 * @author Kai Kreuzer - added support for contexts
public interface ConfigOptionProvider {
     * Provides a collection of {@link ParameterOption}s.
     * @param uri the uri of the config description
     * @param param the parameter name for which the requested options shall be returned
     * @param context the defined context of the parameter
     * @param locale the locale in which the result is expected
     * @return the configuration options provided by this provider if any or {@code null} otherwise
    Collection<ParameterOption> getParameterOptions(URI uri, String param, @Nullable String context,
            @Nullable Locale locale);
