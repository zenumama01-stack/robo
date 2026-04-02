 * This class is as {@link ActionType} which logically combines {@link Action} instances. The composite action hides
 * internal logic and inner connections between participating {@link Action}s and it can be used as a regular
 * {@link Action} module.
public class CompositeActionType extends ActionType {
    private final List<Action> children;
     * Creates an instance of {@code CompositeActionType} with list of {@link Action}s. It initializes only base
     * properties of the {@code CompositeActionType}.
     * @param children is a {@link List} of {@link Action}s.
    public CompositeActionType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable List<Input> inputs, @Nullable List<Output> outputs, @Nullable List<Action> children) {
        super(uid, configDescriptions, inputs, outputs);
        this.children = children != null ? Collections.unmodifiableList(children) : List.of();
     * Creates an instance of {@code CompositeActionType} with list of {@link Action}s. It initializes all properties of
     * the {@code CompositeActionType}.
     * @param label a short and accurate, human-readable label of the {@link ActionType}.
     * @param description a detailed, human-readable description of usage of {@link ActionType} and its benefits.
            @Nullable Visibility visibility, @Nullable List<Input> inputs, @Nullable List<Output> outputs,
            @Nullable List<Action> children) {
        super(uid, configDescriptions, label, description, tags, visibility, inputs, outputs);
     * Gets the {@link Action} modules of the {@code CompositeActionType}.
     * @return a {@link List} of the {@link Action} modules of this {@code CompositeActionType}.
    public List<Action> getChildren() {
