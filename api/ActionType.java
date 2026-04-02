package org.openhab.core.automation.type;
 * This class provides common functionality for creating {@link Action} instances by supplying their meta-information.
 * Each {@link ActionType} is uniquely identifiable in scope of the {@link ModuleTypeRegistry} and defines
 * {@link ConfigDescriptionParameter}s that are meta-information for configuration of the future {@link Action}
 * instances and meta-information for {@link Input}s and {@link Output}s used from these {@link Action} instances.
public class ActionType extends ModuleType {
     * Contains meta-information describing the incoming connections of the {@link Action} module to the other
    private final List<Input> inputs;
     * Contains meta-information describing the outgoing connections of the {@link Action} module to the other
     * {@link Action}s.
    private final List<Output> outputs;
     * Creates an instance of {@link ActionType} with base properties - UID, a {@link List} of configuration
     * descriptions and a {@link List} of {@link Input} definitions.
     * @param uid the {@link ActionType}'s identifier, or {@code null} if a random identifier should be
     * @param configDescriptions describing meta-data for the configuration of the future {@link Action} instances.
     * @param inputs a {@link List} with {@link Input} meta-information descriptions of the future
     *            {@link Action} instances.
    public ActionType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable List<Input> inputs) {
        this(uid, configDescriptions, inputs, null);
     * Creates an instance of the {@link ActionType} with UID, a {@link List} of configuration descriptions,
     * a {@link List} of {@link Input} definitions and a {@link List} of {@link Output} descriptions.
     * @param outputs a {@link List} with {@link Output} meta-information descriptions of the future
            @Nullable List<Input> inputs, @Nullable List<Output> outputs) {
        this(uid, configDescriptions, null, null, null, null, inputs, outputs);
     * Creates an instance of {@link ActionType} with uid, label, description, a {@link Set} of tags, visibility,
     * a {@link List} of configuration descriptions, a {@link List} of {@link Input} descriptions and a {@link List}
     * of {@link Output} descriptions.
     * @param label is a short and accurate name of the {@link ActionType}.
     * @param description is a short and understandable description of which can be used the {@link ActionType}.
     * @param tags defines categories that fit the {@link ActionType} and which can serve as criteria for
     *            searching or filtering it.
     * @param visibility determines whether the {@link ActionType} can be used by anyone if it is
     *            {@link Visibility#VISIBLE} or only by its creator if it is {@link Visibility#HIDDEN}.
            @Nullable String label, @Nullable String description, @Nullable Set<String> tags,
            @Nullable Visibility visibility, @Nullable List<Input> inputs, @Nullable List<Output> outputs) {
        super(uid, configDescriptions, label, description, tags, visibility);
        this.inputs = inputs != null ? Collections.unmodifiableList(inputs) : List.of();
        this.outputs = outputs != null ? Collections.unmodifiableList(outputs) : List.of();
     * Gets the meta-information descriptions of {@link Input}s defined by this type.
     * @return a {@link List} with {@link Input} definitions.
     * Gets the meta-information descriptions of {@link Output}s defined by this type.
     * @return a {@link List} with {@link Output} definitions.
