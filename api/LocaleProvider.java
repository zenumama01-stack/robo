 * The interface describe a provider for a locale.
public interface LocaleProvider {
     * Get a locale.
     * The locale could be used e.g. as a fallback if there is no other one defined explicitly.
     * @return a locale (non-null)
