 * This class provides common functionality for creating {@link Trigger} instances by supplying their meta-information.
 * Each {@link TriggerType} is uniquely identifiable in scope of the {@link ModuleTypeRegistry} and defines
 * {@link ConfigDescriptionParameter}s that are meta-information for configuration of the future {@link Trigger}
 * instances and meta-information for {@link Output}s used from these {@link Trigger} instances.
public class TriggerType extends ModuleType {
     * Creates an instance of {@link TriggerType} with base properties - UID, a {@link List} of configuration
     * descriptions and a {@link List} of {@link Output} descriptions.
    public TriggerType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable List<Output> outputs) {
        super(uid, configDescriptions);
     * Creates an instance of {@link TriggerType} with UID, label, description, a {@link Set} of tags, visibility,
     * a {@link List} of configuration descriptions and a {@link List} of {@link Output} descriptions.
            @Nullable Visibility visibility, @Nullable List<Output> outputs) {
     * Gets the meta-information descriptions of {@link Output}s defined by this type.<br>
     * @return a {@link List} of {@link Output} definitions.
