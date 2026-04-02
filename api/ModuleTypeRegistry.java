 * This interface provides functionality to get available {@link ModuleType}s. The module types can be returned
 * localized depending on locale parameter. When it is not specified or there is no such localization resources the
 * returned module type is localized with default locale.
public interface ModuleTypeRegistry extends Registry<ModuleType, String> {
     * Gets the localized {@link ModuleType} by specified UID and locale.
     * @param moduleTypeUID the a unique id in scope of registered {@link ModuleType}s.
     * @param locale used for localization of the {@link ModuleType}.
     * @return the desired {@link ModuleType} instance or {@code null} if a module type with such UID does not exist or
     *         the passed UID is {@code null}.
    <T extends ModuleType> @Nullable T get(String moduleTypeUID, @Nullable Locale locale);
     * Gets the {@link ModuleType}s filtered by tag.
     * @param moduleTypeTag specifies the filter for getting the {@link ModuleType}s, if it is {@code null} then returns
     *            all {@link ModuleType}s.
     * @return the {@link ModuleType}s, which correspond to the specified filter.
    <T extends ModuleType> Collection<T> getByTag(@Nullable String moduleTypeTag);
     * This method is used for getting the {@link ModuleType}s filtered by tag.
    <T extends ModuleType> Collection<T> getByTag(@Nullable String moduleTypeTag, @Nullable Locale locale);
     * This method is used for getting the {@link ModuleType}s filtered by tags.
     * @param tags specifies the filter for getting the {@link ModuleType}s, if it is {@code null} then returns all
     *            {@link ModuleType}s.
     * @return the {@link ModuleType}s, which correspond to the filter.
    <T extends ModuleType> Collection<T> getByTags(String... tags);
     * @param tags specifies the filter for getting the {@link ModuleType}s, if it is {@code null} then returns
    <T extends ModuleType> Collection<T> getByTags(@Nullable Locale locale, String... tags);
     * This method is used for getting the {@link TriggerType}s. The returned {@link TriggerType}s are
     * localized by default locale.
     * @param tags specifies the filter for getting the {@link TriggerType}s, if it is {@code null} then returns all
     *            {@link TriggerType}s.
     * @return collection of all available {@link TriggerType}s, localized by default locale.
    Collection<TriggerType> getTriggers(String... tags);
     * This method is used for getting the {@link TriggerType}s, localized depending on passed locale parameter.
     * When the locale parameter is not specified or such localization resources are not available the returned
     * {@link TriggerType}s are localized by default locale.
     * @param locale defines the localization of returned {@link TriggerType}s.
     * @return a collection of all available {@link TriggerType}s, localized by default locale or the passed locale
     *         parameter.
    Collection<TriggerType> getTriggers(@Nullable Locale locale, String... tags);
     * This method is used for getting the {@link ConditionType}s. The returned {@link ConditionType}s are
     * @param tags specifies the filter for getting the {@link ConditionType}s, if it is {@code null} then returns all
     *            {@link ConditionType}s.
     * @return collection of all available {@link ConditionType}s, localized by default locale.
    Collection<ConditionType> getConditions(String... tags);
     * This method is used for getting the {@link ConditionType}s, localized depending on passed locale parameter.
     * When the locale parameter is not specified or such localization resources are not available the
     * returned {@link ConditionType}s are localized by default locale.
     * @param locale defines the localization of returned {@link ConditionType}s.
     * @return a collection of all available {@link ConditionType}s, localized by default locale or the passed locale
    Collection<ConditionType> getConditions(@Nullable Locale locale, String... tags);
     * This method is used for getting the {@link ActionType}s. The returned {@link ActionType}s are
     * @param tags specifies the filter for getting the {@link ActionType}s, if it is {@code null} then returns all
     *            {@link ActionType}s.
     * @return collection of all available {@link ActionType}s, localized by default locale.
    Collection<ActionType> getActions(String... tags);
     * This method is used for getting the {@link ActionType}s, localized depending on passed locale parameter.
     * {@link ActionType}s are localized by default locale.
     * @param locale defines the localization of returned {@link ActionType}s.
     * @return a collection of all available {@link ActionType}s, localized by default locale or the passed locale
    Collection<ActionType> getActions(@Nullable Locale locale, String... tags);
