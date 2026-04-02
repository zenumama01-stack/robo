 * A {@link ProfileTypeProvider} is responsible for providing {@link ProfileType}s.
public interface ProfileTypeProvider {
     * Returns all profile types for the given {@link Locale}.
     * @param locale (can be null)
     * @return all profile types or empty list if no profile type exists
    Collection<ProfileType> getProfileTypes(@Nullable Locale locale);
