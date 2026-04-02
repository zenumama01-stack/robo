import org.openhab.core.automation.RuleStatusDetail;
 * Convenience {@link Rule} implementation with a {@link SimpleRuleActionHandler}.
 * This allows defining rules from a JSR-223 language by inheriting from this class and implementing
 * {@link SimpleRuleActionHandler}.
 * @author Kai Kreuzer - made it implement Rule
public abstract class SimpleRule implements Rule, SimpleRuleActionHandler {
    protected static final ConfigDescriptionParameter SCRIPT_TYPE_CONFIG_DESCRIPTION = ConfigDescriptionParameterBuilder
            .create("type", ConfigDescriptionParameter.Type.TEXT).withReadOnly(true).build();
    protected static final ConfigDescriptionParameter SOURCE_CONFIG_DESCRIPTION = ConfigDescriptionParameterBuilder
            .create("source", ConfigDescriptionParameter.Type.TEXT).withReadOnly(true).build();
    protected List<Trigger> triggers = new ArrayList<>();
    protected List<Condition> conditions = new ArrayList<>();
    protected List<Action> actions = new ArrayList<>();
    protected Configuration configuration = new Configuration();
    protected List<ConfigDescriptionParameter> configDescriptions = new ArrayList<>();
    protected @NonNullByDefault({}) String uid;
    protected @Nullable String name;
    protected Set<String> tags = new HashSet<>();
    protected Visibility visibility = Visibility.VISIBLE;
    protected @Nullable String description;
    protected transient volatile RuleStatusInfo status = new RuleStatusInfo(RuleStatus.UNINITIALIZED,
            RuleStatusDetail.NONE);
    public SimpleRule() {
        configDescriptions.add(SCRIPT_TYPE_CONFIG_DESCRIPTION);
        configDescriptions.add(SOURCE_CONFIG_DESCRIPTION);
     * This method is used to specify the identifier of the {@link Rule}.
     * @param ruleUID the identifier of the {@link Rule}.
    public void setUID(String ruleUID) {
        uid = ruleUID;
    public @Nullable String getTemplateUID() {
    public @Nullable String getName() {
     * This method is used to specify the {@link Rule}'s human-readable name.
     * @param ruleName the {@link Rule}'s human-readable name, or {@code null}.
    public void setName(@Nullable String ruleName) {
        name = ruleName;
    public Set<String> getTags() {
     * This method is used to specify the {@link Rule}'s assigned tags.
     * @param ruleTags the {@link Rule}'s assigned tags.
    public void setTags(@Nullable Set<String> ruleTags) {
        tags = ruleTags == null ? new HashSet<>() : ruleTags;
     * This method is used to specify human-readable description of the purpose and consequences of the
     * {@link Rule}'s execution.
     * @param ruleDescription the {@link Rule}'s human-readable description, or {@code null}.
    public void setDescription(@Nullable String ruleDescription) {
        description = ruleDescription;
    public Visibility getVisibility() {
        return visibility;
     * This method is used to specify the {@link Rule}'s {@link Visibility}.
     * @param visibility the {@link Rule}'s {@link Visibility} value.
    public void setVisibility(@Nullable Visibility visibility) {
        this.visibility = visibility == null ? Visibility.VISIBLE : visibility;
    public Configuration getConfiguration() {
     * This method is used to specify the {@link Rule}'s {@link Configuration}.
     * @param ruleConfiguration the new configuration values.
    public void setConfiguration(@Nullable Configuration ruleConfiguration) {
        this.configuration = ruleConfiguration == null ? new Configuration() : ruleConfiguration;
    public List<ConfigDescriptionParameter> getConfigurationDescriptions() {
        return configDescriptions;
     * This method is used to describe with {@link ConfigDescriptionParameter}s
     * the meta info for configuration properties of the {@link Rule}.
    public void setConfigurationDescriptions(@Nullable List<ConfigDescriptionParameter> configDescriptions) {
        this.configDescriptions = configDescriptions == null ? new ArrayList<>() : configDescriptions;
    public List<Condition> getConditions() {
        return conditions;
     * This method is used to specify the conditions participating in {@link Rule}.
     * @param conditions a list with the conditions that should belong to this {@link Rule}.
    public void setConditions(@Nullable List<Condition> conditions) {
        this.conditions = conditions == null ? new ArrayList<>() : conditions;
    public List<Action> getActions() {
     * This method is used to specify the actions participating in {@link Rule}
     * @param actions a list with the actions that should belong to this {@link Rule}.
    public void setActions(@Nullable List<Action> actions) {
        this.actions = actions == null ? new ArrayList<>() : actions;
    public List<Trigger> getTriggers() {
     * This method is used to specify the triggers participating in {@link Rule}
     * @param triggers a list with the triggers that should belong to this {@link Rule}.
    public void setTriggers(@Nullable List<Trigger> triggers) {
        this.triggers = triggers == null ? new ArrayList<>() : triggers;
    public List<Module> getModules() {
        List<Module> modules = new ArrayList<>();
        modules.addAll(triggers);
        modules.addAll(conditions);
        modules.addAll(actions);
        return Collections.unmodifiableList(modules);
    public <T extends Module> List<T> getModules(@Nullable Class<T> moduleClazz) {
        final List<T> result;
        if (Module.class == moduleClazz) {
            result = (List<T>) getModules();
        } else if (Trigger.class == moduleClazz) {
            result = (List<T>) triggers;
        } else if (Condition.class == moduleClazz) {
            result = (List<T>) conditions;
        } else if (Action.class == moduleClazz) {
            result = (List<T>) actions;
            result = List.of();
        result = prime * result + uid.hashCode();
        if (!(obj instanceof Rule)) {
        Rule other = (Rule) obj;
        if (!uid.equals(other.getUID())) {
