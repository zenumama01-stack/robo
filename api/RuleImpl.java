 * This is the internal implementation of a {@link Rule}, which comes with full getters and setters.
 * @author Kai Kreuzer - Introduced transient status and made it implement the Rule interface
public class RuleImpl implements Rule {
    protected List<Trigger> triggers;
    protected List<Condition> conditions;
    protected List<Action> actions;
    protected Configuration configuration;
    protected List<ConfigDescriptionParameter> configDescriptions;
    protected @Nullable String templateUID;
    protected TemplateState templateStatus;
    protected String uid;
    protected Set<String> tags;
    protected Visibility visibility;
     * Constructor for creating an empty {@link Rule} with a specified rule identifier.
     * When {@code null} is passed for the {@code uid} parameter, the {@link Rule}'s identifier will be randomly
     * generated.
     * @param uid the rule's identifier, or {@code null} if a random identifier should be generated.
    public RuleImpl(@Nullable String uid) {
        this(uid, null, null, null, null, null, null, null, null, null, TemplateState.NO_TEMPLATE, null);
     * Utility constructor for creating a {@link Rule} from a set of modules, or from a template.
     * @param uid the {@link Rule}'s identifier, or {@code null} if a random identifier should be generated.
     * @param name the rule's name
     * @param description the rule's description
     * @param tags the tags
     * @param triggers the {@link Rule}'s triggers list, or {@code null} if the {@link Rule} should have no triggers or
     *            will be created from a template.
     * @param conditions the {@link Rule}'s conditions list, or {@code null} if the {@link Rule} should have no
     *            conditions, or will be created from a template.
     * @param actions the {@link Rule}'s actions list, or {@code null} if the {@link Rule} should have no actions, or
     * @param configDescriptions metadata describing the configuration of the {@link Rule}.
     * @param configuration the values that will configure the modules of the {@link Rule}.
     * @param templateUID the {@link RuleTemplate} identifier of the template that will be used by the
     *            {@link RuleRegistry} to validate the {@link Rule}'s configuration, as well as to create and configure
     *            the {@link Rule}'s modules, or null if the {@link Rule} should not be created from a template.
     * @param visibility the {@link Rule}'s visibility
    public RuleImpl(@Nullable String uid, final @Nullable String name, final @Nullable String description,
            final @Nullable Set<String> tags, @Nullable List<Trigger> triggers, @Nullable List<Condition> conditions,
            @Nullable List<Action> actions, @Nullable List<ConfigDescriptionParameter> configDescriptions,
            @Nullable Configuration configuration, @Nullable String templateUID, TemplateState templateStatus,
            @Nullable Visibility visibility) {
        this.uid = uid == null ? UUID.randomUUID().toString() : uid;
        this.tags = tags == null ? Set.of() : Set.copyOf(tags);
        this.triggers = triggers == null ? List.of() : List.copyOf(triggers);
        this.conditions = conditions == null ? List.of() : List.copyOf(conditions);
        this.actions = actions == null ? List.of() : List.copyOf(actions);
        this.configDescriptions = configDescriptions == null ? List.of() : List.copyOf(configDescriptions);
        this.configuration = configuration == null ? new Configuration()
                : new Configuration(configuration.getProperties());
        this.templateUID = templateUID;
        this.templateStatus = templateStatus;
        return templateUID;
     * This method is used to specify the {@link RuleTemplate} identifier of the template that will be used to by the
     * {@link RuleRegistry} to resolve the {@link RuleImpl}: to validate the {@link RuleImpl}'s configuration, as well
     * as to create and configure the {@link RuleImpl}'s modules.
    public void setTemplateUID(@Nullable String templateUID) {
    public TemplateState getTemplateState() {
        return templateStatus;
     * This method is used to specify the current rule template state.
     * @param templateState the {@link TemplateState} to set.
    public void setTemplateStatus(TemplateState templateState) {
        this.templateStatus = Objects.requireNonNull(templateState);
     * This method is used to specify the {@link RuleImpl}'s human-readable name.
     * @param ruleName the {@link RuleImpl}'s human-readable name, or {@code null}.
     * This method is used to specify the {@link RuleImpl}'s assigned tags.
     * @param ruleTags the {@link RuleImpl}'s assigned tags.
        tags = ruleTags == null ? Set.of() : Collections.unmodifiableSet(ruleTags);
     * {@link RuleImpl}'s execution.
     * @param ruleDescription the {@link RuleImpl}'s human-readable description, or {@code null}.
     * This method is used to specify the {@link RuleImpl}'s {@link Visibility}.
     * @param visibility the {@link RuleImpl}'s {@link Visibility} value.
     * This method is used to specify the {@link RuleImpl}'s {@link Configuration}.
     * This method is used to describe with {@link ConfigDescriptionParameter}s the meta info for configuration
     * properties of the {@link RuleImpl}.
        this.configDescriptions = configDescriptions == null ? List.of()
                : Collections.unmodifiableList(configDescriptions);
     * This method is used to specify the conditions participating in {@link RuleImpl}.
     * @param conditions a list with the conditions that should belong to this {@link RuleImpl}.
        this.conditions = conditions == null ? List.of() : Collections.unmodifiableList(conditions);
     * This method is used to specify the actions participating in {@link RuleImpl}
     * @param actions a list with the actions that should belong to this {@link RuleImpl}.
        this.actions = actions == null ? List.of() : Collections.unmodifiableList(actions);
     * This method is used to specify the triggers participating in {@link RuleImpl}
     * @param triggers a list with the triggers that should belong to this {@link RuleImpl}.
        this.triggers = triggers == null ? List.of() : Collections.unmodifiableList(triggers);
        if (!(obj instanceof RuleImpl)) {
        RuleImpl other = (RuleImpl) obj;
        if (!uid.equals(other.uid)) {
    private List<Condition> conditions = new CopyOnWriteArrayList<>();
    private @Nullable String argument;
    public RuleImpl() {
    public RuleImpl(Rule rule) {
        this.conditions = rule.getConditions();
        this.argument = rule.getArgument();
    public void setConditions(List<Condition> conditions) {
        this.conditions = new CopyOnWriteArrayList<>(conditions);
    public @Nullable String getArgument() {
    public void setArgument(@Nullable String argument) {
        this.argument = argument;
