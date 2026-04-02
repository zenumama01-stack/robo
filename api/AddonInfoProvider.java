 * The {@link AddonInfoProvider} is a service interface providing {@link AddonInfo} objects. All registered
 * {@link AddonInfoProvider} services are tracked by the {@link AddonInfoRegistry} and provided as one common
 * collection.
 * @see AddonInfoRegistry
public interface AddonInfoProvider {
     * Returns the binding information for the specified binding UID and locale (language),
     * or {@code null} if no binding information could be found.
     * @param uid the UID to be looked for (could be null or empty)
     * @param locale the locale to be used for the binding information (could be null)
     * @return a localized binding information object (could be null)
    @Nullable
    AddonInfo getAddonInfo(@Nullable String uid, @Nullable Locale locale);
     * Returns all binding information in the specified locale (language) this provider contains.
     * @return a localized set of all binding information this provider contains
     *         (could be empty)
    Set<AddonInfo> getAddonInfos(@Nullable Locale locale);
