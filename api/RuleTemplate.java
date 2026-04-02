package org.openhab.core.automation.template;
 * This class is used to define {@code Rule Templates} which are shared combination of ready to use modules, which can
 * be configured to produce {@link Rule} instances.
 * The {@link RuleTemplate}s can be used by any creator of Rules, but they can be modified only by its creator. The
 * template modification is done by updating the {@link RuleTemplate}.
 * Templates can have {@code tags} - non-hierarchical keywords or terms for describing them.
 * @author Markus Rathgeb - Add default constructor for deserialization
public class RuleTemplate implements Template {
     * Holds the {@link RuleTemplate}'s identifier, specified by its creator or randomly generated.
     * Holds a list with the {@link Trigger}s participating in the {@link RuleTemplate}.
    private final List<Trigger> triggers;
     * Holds a list with the {@link Condition}s participating in the {@link RuleTemplate}.
    private final List<Condition> conditions;
     * Holds a list with the {@link Action}s participating in the {@link RuleTemplate}.
    private final List<Action> actions;
     * Holds a set of non-hierarchical keywords or terms for describing the {@link RuleTemplate}.
    private final Set<String> tags;
     * Holds the short, human-readable label of the {@link RuleTemplate}.
    private final @Nullable String label;
     * Describes the usage of the {@link RuleTemplate} and its benefits.
     * Determines {@link Visibility} of the {@link RuleTemplate}.
    private final Visibility visibility;
     * Defines a set of configuration properties of the future {@link Rule} instances.
    private final List<ConfigDescriptionParameter> configDescriptions;
     * Creates a {@link RuleTemplate} instance that will be used for creating {@link Rule}s from a set of modules,
     * belong to the template. When {@code null} is passed for the {@code uid} parameter, the {@link RuleTemplate}'s
     * identifier will be randomly generated.
     * @param uid the {@link RuleTemplate}'s identifier, or {@code null} if a random identifier should be
     *            generated.
     * @param label the short human-readable {@link RuleTemplate}'s label.
     * @param description a detailed human-readable {@link RuleTemplate}'s description.
     * @param tags the {@link RuleTemplate}'s assigned tags.
     * @param triggers the {@link RuleTemplate}'s triggers list, or {@code null} if the {@link RuleTemplate}
     *            should have no triggers.
     * @param conditions the {@link RuleTemplate}'s conditions list, or {@code null} if the {@link RuleTemplate}
     *            should have no conditions.
     * @param actions the {@link RuleTemplate}'s actions list, or {@code null} if the {@link RuleTemplate}
     *            should have no actions.
     * @param configDescriptions describing meta-data for the configuration of the future {@link Rule} instances.
     * @param visibility the {@link RuleTemplate}'s visibility.
    public RuleTemplate(@Nullable String uid, @Nullable String label, @Nullable String description,
            @Nullable Set<String> tags, @Nullable List<Trigger> triggers, @Nullable List<Condition> conditions,
        this.tags = tags == null ? Set.of() : Collections.unmodifiableSet(tags);
     * Gets the unique identifier of the {@link RuleTemplate}. It can be specified by the {@link RuleTemplate}'s
     * @return an identifier of this {@link RuleTemplate}. Can't be {@code null}.
     * Gets the {@link RuleTemplate}'s assigned tags.
     * @return the {@link RuleTemplate}'s assigned tags.
     * Gets the {@link RuleTemplate}'s human-readable label.
     * @return the {@link RuleTemplate}'s human-readable label, or {@code null} if not specified.
     * Gets the human-readable description of the purpose of the {@link RuleTemplate}.
     * @return the {@link RuleTemplate}'s human-readable description, or {@code null}.
     * Gets the {@link RuleTemplate}'s {@link Visibility}.
     * @return the {@link RuleTemplate}'s {@link Visibility} value.
     * Gets the {@link List} with {@link ConfigDescriptionParameter}s defining meta info for configuration properties of
     * the future {@link Rule} instances.
     * Gets a {@link Module} participating in the {@link RuleTemplate}.
     * @param moduleId unique identifier of a module in this {@link RuleTemplate}.
     * @return module with specified identifier or {@code null} when such does not exist.
    public @Nullable Module getModule(String moduleId) {
        for (Module module : getModules(Module.class)) {
     * Gets the modules of the {@link RuleTemplate}, corresponding to the specified class.
     * @param moduleClazz defines the class of the looking modules. It can be {@link Module}, {@link Trigger},
     *            {@link Condition} or {@link Action}.
     * @return the modules of defined type or empty list if the {@link RuleTemplate} has no modules that belong to the
     *         specified type.
    public <T extends Module> List<T> getModules(Class<T> moduleClazz) {
            result = (List<T>) Collections.unmodifiableList(modules);
     * Gets the triggers participating in {@link RuleTemplate}.
     * @return a list with the triggers that belong to this {@link RuleTemplate}.
     * Gets the conditions participating in {@link RuleTemplate}.
     * @return a list with the conditions that belong to this {@link RuleTemplate}.
     * Gets the actions participating in {@link RuleTemplate}.
     * @return a list with the actions that belong to this {@link RuleTemplate}.
     * Returns the hash code of this object depends on the hash code of the UID that it owns.
     * Two objects are equal if they own equal UIDs.
        if (!(obj instanceof RuleTemplate)) {
        RuleTemplate other = (RuleTemplate) obj;
