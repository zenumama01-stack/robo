 * Implementors are capable of creating {@link Profile} instances.
public interface ProfileFactory {
     * Creates a {@link Profile} instance for the given profile type identifier.
     * @param profileTypeUID the profile type identifier
     * @param callback the {@link ProfileCallback} instance to be used by the {@link Profile} instance
     * @param profileContext giving access to the profile's context like configuration, scheduler, etc.
     * @return the profile instance or {@code null} if this factory cannot handle the given link
    Profile createProfile(ProfileTypeUID profileTypeUID, ProfileCallback callback, ProfileContext profileContext);
     * Return the identifiers of all supported profile types.
     * @return a collection of all profile type identifier which this class is capable of creating
    Collection<ProfileTypeUID> getSupportedProfileTypeUIDs();
