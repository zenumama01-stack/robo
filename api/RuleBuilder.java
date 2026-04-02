 * This class allows the easy construction of a {@link Rule} instance using the builder pattern.
public class RuleBuilder {
    private List<Trigger> triggers;
    private List<Condition> conditions;
    private List<Action> actions;
    private List<ConfigDescriptionParameter> configDescriptions;
    private @Nullable String templateUID;
    private TemplateState templateState;
    private @Nullable String name;
    private @Nullable Visibility visibility;
    protected RuleBuilder(Rule rule) {
        this.triggers = new LinkedList<>(rule.getTriggers());
        this.conditions = new LinkedList<>(rule.getConditions());
        this.actions = new LinkedList<>(rule.getActions());
        this.configuration = new Configuration(rule.getConfiguration());
        this.configDescriptions = new LinkedList<>(rule.getConfigurationDescriptions());
        this.templateUID = rule.getTemplateUID();
        this.templateState = TemplateState.NO_TEMPLATE;
        this.uid = rule.getUID();
        this.name = rule.getName();
        this.tags = new HashSet<>(rule.getTags());
        this.visibility = rule.getVisibility();
        this.description = rule.getDescription();
    public static RuleBuilder create(String ruleId) {
        Rule rule = new RuleImpl(ruleId);
        return new RuleBuilder(rule);
    public static RuleBuilder create(Rule r) {
        return create(r.getUID()).withActions(r.getActions()).withConditions(r.getConditions())
                .withTriggers(r.getTriggers()).withConfiguration(r.getConfiguration())
                .withConfigurationDescriptions(r.getConfigurationDescriptions()).withDescription(r.getDescription())
                .withName(r.getName()).withTags(r.getTags()).withTemplateUID(r.getTemplateUID())
                .withTemplateState(r.getTemplateState());
    public static RuleBuilder create(RuleTemplate template, String uid, @Nullable String name,
            Configuration configuration, Visibility visibility) {
        return create(uid).withActions(template.getActions()).withConditions(template.getConditions())
                .withTriggers(template.getTriggers()).withConfiguration(configuration)
                .withConfigurationDescriptions(template.getConfigurationDescriptions())
                .withDescription(template.getDescription()).withName(name).withTags(template.getTags())
                .withTemplateState(TemplateState.INSTANTIATED).withTemplateUID(template.getUID());
    public RuleBuilder withName(@Nullable String name) {
    public RuleBuilder withDescription(@Nullable String description) {
    public RuleBuilder withTemplateUID(@Nullable String uid) {
        this.templateUID = uid;
    public RuleBuilder withTemplateState(TemplateState templateState) {
        this.templateState = templateState;
    public RuleBuilder withVisibility(@Nullable Visibility visibility) {
    public RuleBuilder withTriggers(@Nullable Trigger... triggers) {
        return withTriggers(Arrays.asList(triggers));
    public RuleBuilder withTriggers(@Nullable List<? extends Trigger> triggers) {
        if (triggers != null) {
            List<Trigger> triggerList = new ArrayList<>(triggers.size());
            triggers.forEach(t -> triggerList.add(TriggerBuilder.create(t).build()));
            this.triggers = triggerList;
    public RuleBuilder withConditions(@Nullable Condition... conditions) {
        return withConditions(Arrays.asList(conditions));
    public RuleBuilder withConditions(@Nullable List<? extends Condition> conditions) {
        if (conditions != null) {
            List<Condition> conditionList = new ArrayList<>(conditions.size());
            conditions.forEach(c -> conditionList.add(ConditionBuilder.create(c).build()));
            this.conditions = conditionList;
    public RuleBuilder withActions(@Nullable Action... actions) {
        return withActions(Arrays.asList(actions));
    public RuleBuilder withActions(@Nullable List<? extends Action> actions) {
        if (actions != null) {
            List<Action> actionList = new ArrayList<>(actions.size());
            actions.forEach(a -> actionList.add(ActionBuilder.create(a).build()));
            this.actions = actionList;
    public RuleBuilder withTags(String... tags) {
        withTags(Set.of(tags));
    public RuleBuilder withTags(@Nullable Set<String> tags) {
        this.tags = tags != null ? Set.copyOf(tags) : Set.of();
    public RuleBuilder withConfiguration(@Nullable Configuration ruleConfiguration) {
        this.configuration = new Configuration(ruleConfiguration);
    public RuleBuilder withConfigurationDescriptions(@Nullable List<ConfigDescriptionParameter> configDescs) {
        this.configDescriptions = configDescs != null ? List.copyOf(configDescs) : List.of();
    public Rule build() {
        return new RuleImpl(uid, name, description, tags, triggers, conditions, actions, configDescriptions,
                configuration, templateUID, templateState, visibility);
