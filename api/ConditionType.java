 * This class provides common functionality for creating {@link Condition} instances by supplying their
 * meta-information. Each {@link ConditionType} is uniquely identifiable in scope of the {@link ModuleTypeRegistry} and
 * defines {@link ConfigDescriptionParameter}s that are meta-information for configuration of the future
 * {@link Condition} instances and meta-information for {@link Input}s used from these {@link Condition} instances.
public class ConditionType extends ModuleType {
     * Creates an instance of {@link ConditionType} with base properties - UID, a {@link List} of configuration
     * descriptions and a {@link List} of {@link Input} descriptions.
     * @param uid the {@link ConditionType}'s identifier, or {@code null} if a random identifier should
     *            be generated.
     * @param configDescriptions describing meta-data for the configuration of the future {@link Condition} instances.
    public ConditionType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
        this(uid, configDescriptions, null, null, null, null, inputs);
     * Creates an instance of {@link ConditionType} with uid, label, description, a {@link Set} of tags, visibility,
     * a {@link List} of configuration descriptions and a {@link List} of {@link Input} descriptions.
     * @param label a short and accurate, human-readable label of the {@link ConditionType}.
     * @param description a detailed, human-readable description of usage of {@link ConditionType} and its
     * @param tags defines categories that fit the {@link ConditionType} and which can serve as criteria
     *            for searching or filtering it.
     * @param visibility determines whether the {@link ConditionType} can be used by anyone if it is
            @Nullable Visibility visibility, @Nullable List<Input> inputs) {
     * Gets the meta-information descriptions of {@link Input}s defined by this {@link ConditionType}.
     * @return a {@link List} of {@link Input} meta-information descriptions.
