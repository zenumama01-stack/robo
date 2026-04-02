 * This class is as {@link TriggerType} which logically combines {@link Trigger} modules. The composite trigger hides
 * internal logic between participating {@link Trigger}s and it can be used as a regular {@link Trigger} module.
public class CompositeTriggerType extends TriggerType {
    private final List<Trigger> children;
     * Creates an instance of {@code CompositeTriggerType} with ordered set of {@link Trigger} modules. It initializes
     * only base properties of the {@code CompositeTriggerType}.
     * @param uid the {@link TriggerType}'s identifier, or {@code null} if a random identifier should be
     * @param configDescriptions describing meta-data for the configuration of the future {@link Trigger} instances.
     *            {@link Trigger} instances.
     * @param children is a {@link List} of {@link Trigger} modules.
    public CompositeTriggerType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable List<Output> outputs, @Nullable List<Trigger> children) {
        super(uid, configDescriptions, outputs);
     * all properties of the {@code CompositeTriggerType}.
     * @param label a short and accurate, human-readable label of the {@link TriggerType}.
     * @param description a detailed, human-readable description of usage of {@link TriggerType} and its
     *            benefits.
     * @param tags defines categories that fit the {@link TriggerType} and which can serve as criteria for
     * @param visibility determines whether the {@link TriggerType} can be used by anyone if it is
            @Nullable Visibility visibility, @Nullable List<Output> outputs, @Nullable List<Trigger> children) {
        super(uid, configDescriptions, label, description, tags, visibility, outputs);
     * Gets the {@link Trigger} modules of the {@code CompositeTriggerType}.
     * @return a {@link List} of the {@link Trigger} modules of this {@code CompositeTriggerType}.
    public List<Trigger> getChildren() {
