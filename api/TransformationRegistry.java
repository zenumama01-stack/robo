 * The {@link TransformationRegistry} is the interface for the transformation registry
public interface TransformationRegistry extends Registry<Transformation, String> {
    Pattern CONFIG_UID_PATTERN = Pattern.compile("config:(?<type>\\w+):(?<name>\\w+)(:(?<language>\\w+))?");
     * Get a localized version of the transformation for a given UID
     * @param uid the configuration UID
     * @param locale a locale (system locale is used if <code>null</code>)
     * @return the requested {@link Transformation} (or <code>null</code> if not found).
    Transformation get(String uid, @Nullable Locale locale);
     * Get all transformations which match the given types
     * @param types a {@link Collection} of configuration types
     * @return a {@link Collection} of {@link Transformation}s
    Collection<Transformation> getTransformations(Collection<String> types);
