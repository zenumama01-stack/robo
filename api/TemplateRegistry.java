 * This interface provides functionality to get available {@link Template}s. The {@link Template} can be returned
 * localized depending on locale parameter. When the parameter is not specified or there is no such localization
 * resources the returned template is localized with default locale.
public interface TemplateRegistry<E extends Template> extends Registry<E, String> {
     * Gets a template specified by unique identifier.
     * @param uid the unique identifier in scope of registered templates.
     * @return the desired template instance or {@code null} if a template with such UID does not exist or the passed
     *         UID is {@code null}.
    E get(String uid, @Nullable Locale locale);
     * Gets the templates filtered by tag.
     * @param tag determines the tag that the templates must have, to be included in the returned result. If it is
     *            {@code null} then the result will contain all available templates.
     * @return a collection of templates, which correspond to the specified tag.
    Collection<E> getByTag(@Nullable String tag);
     * @return a collection of localized templates, which correspond to the specified tag.
    Collection<E> getByTag(@Nullable String tag, @Nullable Locale locale);
     * Gets the templates filtered by tags.
     * @param tags determines the set of tags that the templates must have, to be included in the returned result. If it
     *            is {@code null} then the result will contain all templates.
     * @return a collection of templates, which correspond to the specified set of tags.
    Collection<E> getByTags(String... tags);
     * @param tags determines the set of tags that the templates must have, to be included in the returned result. If
     *            it is {@code null} then the result will contain all templates.
     * @return the templates, which correspond to the specified set of tags.
    Collection<E> getByTags(@Nullable Locale locale, String... tags);
     * Gets all available templates, localized by specified locale.
     * @return a collection of localized templates, corresponding to the parameterized type.
    Collection<E> getAll(@Nullable Locale locale);
