 * The {@link TranslationProvider} is a service interface for internationalization support within
 * the platform. This service can be used to translate specific keys into its according
 * text by considering the specified {@link Locale} (language). Any module which supports
 * resource files is managed by this provider and used for translation. This service uses
 * the i18n mechanism of Java.
 * @author Thomas Höfer - Added getText operation with arguments
public interface TranslationProvider {
     * Returns a translation for the specified key in the specified locale (language) by only
     * considering the translations within the specified module.
     * If no translation could be found, the specified default text is returned.<br>
     * If the specified locale is {@code null}, the default locale is used.
     * @param bundle the module to be used for the look-up (could be null)
     * @param key the key to be translated (could be null or empty)
     * @param defaultText the default text to be used (could be null or empty)
     * @param locale the locale (language) to be used (could be null)
     * @return the translated text or the default text (could be null or empty)
    String getText(@Nullable Bundle bundle, @Nullable String key, @Nullable String defaultText,
     * considering the translations within the specified module. The operation will inject the
     * given arguments into the translation.
     * @param arguments the arguments to be injected into the translation (could be null)
    String getText(@Nullable Bundle bundle, @Nullable String key, @Nullable String defaultText, @Nullable Locale locale,
            @Nullable Object @Nullable... arguments);
