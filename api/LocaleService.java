 * Provides helper method for working with locales.
 * @author Lyubomir Papazov - Initial contribution
public interface LocaleService {
     * Returns the locale in respect to the given "Accept-Language" HTTP header.
     * @param acceptLanguageHttpHeader value of the "Accept-Language" HTTP header (can be null).
     * @return Locale for the "Accept-Language" HTTP header or default locale if
     *         header is not set or can not be parsed.
    Locale getLocale(@Nullable String acceptLanguageHttpHeader);
