 * This class is as {@link ConditionType} which logically combines {@link Condition} modules. The composite condition
 * hides internal logic between participating conditions and it can be used as a regular {@link Condition} module.
public class CompositeConditionType extends ConditionType {
    private final List<Condition> children;
     * Creates an instance of {@code CompositeConditionType} with ordered set of {@link Condition}s. It initializes
     * only base properties of the {@code CompositeConditionType}.
     * @param uid is the {@link ConditionType}'s identifier, or {@code null} if a random identifier
     *            should be generated.
     * @param configDescriptions is a {@link List} of configuration descriptions describing meta-data for the
     *            configuration of the future {@link Condition} instances.
     * @param inputs is a {@link List} with {@link Input}'s meta-information descriptions of the future
     *            {@link Condition} instances.
     * @param children is a {@link List} of {@link Condition}s.
    public CompositeConditionType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable List<Input> inputs, @Nullable List<Condition> children) {
        super(uid, configDescriptions, inputs);
     * all properties of the {@code CompositeConditionType}.
     * @param label a short and accurate, human-readable label of the {@code CompositeConditionType}.
     * @param description a detailed, human-readable description of usage of {@code CompositeConditionType} and
     *            its benefits.
     * @param tags defines categories that fit the {@code CompositeConditionType} and which can serve as
     *            criteria for searching or filtering it.
     * @param visibility determines whether the {@code CompositeConditionType} can be used by anyone if it is
     *            If {@code null} is provided the default visibility {@link Visibility#VISIBLE} will be
     *            used.
            @Nullable Visibility visibility, @Nullable List<Input> inputs, @Nullable List<Condition> children) {
        super(uid, configDescriptions, label, description, tags, visibility, inputs);
     * Gets the {@link Condition} modules of the {@code CompositeConditionType}.
     * @return a {@link List} of the {@link Condition} modules of this {@code CompositeConditionType}.
    public List<Condition> getChildren() {
