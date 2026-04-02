 * The {@link ProfileTypeRegistry} allows access to the {@link ProfileType}s provided by all
 * {@link ProfileTypeProvider}s.
public interface ProfileTypeRegistry {
     * Get the available {@link ProfileType}s from all providers using the default locale.
     * @return all profile types
    List<ProfileType> getProfileTypes();
     * Get the available {@link ProfileType}s from all providers.
     * @param locale the language to use (may be null)
    List<ProfileType> getProfileTypes(@Nullable Locale locale);
