import org.openhab.core.automation.internal.template.RuleTemplateRegistry;
import org.openhab.core.automation.util.ConfigurationNormalizer;
import org.openhab.core.common.registry.AbstractRegistry;
 * This is the main implementation of the {@link RuleRegistry}, which is registered as a service.
 * The {@link RuleRegistryImpl} provides basic functionality for managing {@link Rule}s.
 * <li>Add Rules with the {@link #add(Rule)}, {@link #added(Provider, Rule)}, {@link #addProvider(Provider)}
 * methods.</li>
 * <li>Get the existing rules with the {@link #get(String)}, {@link #getAll()}, {@link #getByTag(String)},
 * {@link #getByTags(String[])} methods.</li>
 * <li>Update the existing rules with the {@link #update(Rule)}, {@link #updated(Provider, Rule, Rule)} methods.</li>
 * <li>Remove Rules with the {@link #remove(String)} method.</li>
 * This class also persists the rules into the {@link StorageService} service and restores
 * them when the system is restarted.
 * The {@link RuleRegistry} manages the state (<b>enabled</b> or <b>disabled</b>) of the Rules:
 * <li>To check a Rule's state, use the {@link #isEnabled(String)} method.</li>
 * <li>To change a Rule's state, use the {@link #setEnabled(String, boolean)} method.</li>
 * <li>To check a Rule's status info, use the {@link #getStatusInfo(String)} method.</li>
 * <li>The status of a newly added Rule, or a Rule enabled with {@link #setEnabled(String, boolean)}, or an updated
 * Rule, is first set to {@link RuleStatus#UNINITIALIZED}.</li>
 * <li>After a Rule is added or enabled, or updated, a verification procedure is initiated. If the verification of the
 * modules IDs, connections between modules and configuration values of the modules is successful, and the module
 * handlers are correctly set, the status is set to {@link RuleStatus#IDLE}.</li>
 * <li>If a Rule is disabled with {@link #setEnabled(String, boolean)}, it's status is set to
 * @author Ana Dimova - Persistence implementation & updating rules from providers
 * @author Kai Kreuzer - refactored (managed) provider and registry implementation and other fixes
 * @author Benedikt Niehues - added events for rules
 * @author Victor Toni - return only copies of {@link Rule}s
 * @author Ravi Nadahar - added support for regenerating {@link Rule}s from {@link RuleTemplate}s.
@Component(service = RuleRegistry.class, immediate = true)
public class RuleRegistryImpl extends AbstractRegistry<Rule, String, RuleProvider>
        implements RuleRegistry, RegistryChangeListener<RuleTemplate> {
    private static final String SOURCE = RuleRegistryImpl.class.getSimpleName();
    private static final Set<TemplateState> PROCESSABLE_TEMPLATE_STATES = Set.of(TemplateState.PENDING,
            TemplateState.TEMPLATE_MISSING);
    private final Logger logger = LoggerFactory.getLogger(RuleRegistryImpl.class.getName());
    private @NonNullByDefault({}) ModuleTypeRegistry moduleTypeRegistry;
    private @NonNullByDefault({}) RuleTemplateRegistry templateRegistry;
     * Constructor that is responsible to invoke the super constructor with appropriate providerClazz
     * {@link RuleProvider} - the class of the providers that should be tracked automatically after activation.
    public RuleRegistryImpl() {
        super(RuleProvider.class);
     * Activates this component. Called from DS.
     * @param bundleContext this component context.
    protected void activate(BundleContext bundleContext) {
        super.activate(bundleContext);
        super.setEventPublisher(eventPublisher);
        super.unsetEventPublisher(eventPublisher);
    protected void setReadyService(ReadyService readyService) {
        super.setReadyService(readyService);
    protected void unsetReadyService(ReadyService readyService) {
        super.unsetReadyService(readyService);
    @Reference(cardinality = ReferenceCardinality.OPTIONAL, policy = ReferencePolicy.DYNAMIC, name = "ManagedRuleProvider")
    protected void setManagedProvider(ManagedRuleProvider managedProvider) {
        super.setManagedProvider(managedProvider);
    protected void unsetManagedProvider(ManagedRuleProvider managedProvider) {
        super.unsetManagedProvider(managedProvider);
     * Bind the {@link ModuleTypeRegistry} service - called from DS.
     * @param moduleTypeRegistry a {@link ModuleTypeRegistry} service.
    @Reference(cardinality = ReferenceCardinality.MANDATORY, policy = ReferencePolicy.STATIC)
    protected void setModuleTypeRegistry(ModuleTypeRegistry moduleTypeRegistry) {
     * Unbind the {@link ModuleTypeRegistry} service - called from DS.
    protected void unsetModuleTypeRegistry(ModuleTypeRegistry moduleTypeRegistry) {
        this.moduleTypeRegistry = null;
     * Bind the {@link RuleTemplateRegistry} service - called from DS.
     * @param templateRegistry a {@link RuleTemplateRegistry} service.
    protected void setTemplateRegistry(TemplateRegistry<RuleTemplate> templateRegistry) {
        if (templateRegistry instanceof RuleTemplateRegistry registry) {
            this.templateRegistry = registry;
            templateRegistry.addRegistryChangeListener(this);
     * Unbind the {@link RuleTemplateRegistry} service - called from DS.
    protected void unsetTemplateRegistry(TemplateRegistry<RuleTemplate> templateRegistry) {
        if (templateRegistry instanceof RuleTemplateRegistry) {
            this.templateRegistry = null;
            templateRegistry.removeRegistryChangeListener(this);
     * This method is used to register a {@link Rule} into the {@link RuleEngineImpl}. First the {@link Rule} become
     * @param rule a {@link Rule} instance which have to be added into the {@link RuleEngineImpl}.
     * @throws RuntimeException
     *             when passed module has a required configuration property and it is not specified
     *             in rule definition
     *             nor
     *             in the module's module type definition.
     * @throws IllegalArgumentException
     *             when a module id contains dot or when the rule with the same UID already exists.
    public Rule add(Rule rule) {
        super.add(rule);
        Rule ruleCopy = get(rule.getUID());
        if (ruleCopy == null) {
        return ruleCopy;
    protected void notifyListenersAboutAddedElement(Rule element) {
        postRuleAddedEvent(element);
        postRuleStatusInfoEvent(element.getUID(), new RuleStatusInfo(RuleStatus.UNINITIALIZED));
        super.notifyListenersAboutAddedElement(element);
    protected void notifyListenersAboutUpdatedElement(Rule oldElement, Rule element) {
        postRuleUpdatedEvent(element, oldElement);
        super.notifyListenersAboutUpdatedElement(oldElement, element);
     * @see RuleRegistryImpl#postEvent(org.openhab.core.events.Event)
    protected void postRuleAddedEvent(Rule rule) {
        postEvent(RuleEventFactory.createRuleAddedEvent(rule, SOURCE));
    protected void postRuleRemovedEvent(Rule rule) {
        postEvent(RuleEventFactory.createRuleRemovedEvent(rule, SOURCE));
    protected void postRuleUpdatedEvent(Rule rule, Rule oldRule) {
        postEvent(RuleEventFactory.createRuleUpdatedEvent(rule, oldRule, SOURCE));
        postEvent(RuleEventFactory.createRuleStatusInfoEvent(statusInfo, ruleUID, SOURCE));
    protected void notifyListenersAboutRemovedElement(Rule element) {
        super.notifyListenersAboutRemovedElement(element);
        postRuleRemovedEvent(element);
        Collection<Rule> result = new LinkedList<>();
        if (tag == null) {
            forEach(result::add);
            forEach(rule -> {
                if (rule.getTags().contains(tag)) {
                    result.add(rule);
        Set<String> tagSet = new HashSet<>(Arrays.asList(tags));
        if (tagSet.isEmpty()) {
                if (rule.getTags().containsAll(tagSet)) {
        Rule rule = get(ruleUID);
                    "Can't regenerate rule from template because no rule with UID \"" + ruleUID + "\" exists");
        if (rule.getTemplateUID() == null || rule.getTemplateState() == TemplateState.NO_TEMPLATE) {
                    "Can't regenerate rule from template because the rule isn't linked to a template");
            Rule resolvedRule = resolveRuleByTemplate(
                    RuleBuilder.create(rule).withActions((List<Action>) null).withConditions((List<Condition>) null)
                            .withTriggers((List<Trigger>) null).withTemplateState(TemplateState.PENDING).build());
            Provider<Rule> provider = getProvider(rule.getUID());
            if (provider == null) {
                logger.error("Regenerating rule '{}' from template failed because the provider is unknown",
            if (provider instanceof ManagedRuleProvider) {
                update(resolvedRule);
                updated(provider, rule, resolvedRule);
            if (resolvedRule.getTemplateState() == TemplateState.TEMPLATE_MISSING) {
                logger.warn("Failed to regenerate rule '{}' from template since the template is missing",
                logger.info("Rule '{}' was regenerated from template '{}'", rule.getUID(), rule.getTemplateUID());
            logger.error("Regenerating rule '{}' from template failed: {}", rule.getUID(), e.getMessage(), e);
     * The method checks if the rule has to be resolved by template or not. If the rule does not contain tempateUID it
     * returns same rule, otherwise it tries to resolve the rule created from template. If the template is available
     * the method creates a new rule based on triggers, conditions and actions from template. If the template is not
     * available returns the same rule.
     * @param rule a rule defined by template.
     * @return the resolved rule(containing modules defined by the template) or not resolved rule, if the template is
     *         missing.
    private Rule resolveRuleByTemplate(Rule rule) {
        TemplateState templateState = rule.getTemplateState();
        if (templateState == TemplateState.NO_TEMPLATE || templateState == TemplateState.INSTANTIATED) {
        String templateUID = rule.getTemplateUID();
        if (templateUID == null) {
        RuleTemplate template = templateRegistry.get(templateUID);
            if (templateState == TemplateState.TEMPLATE_MISSING) {
            logger.debug("Rule template {} does not exist.", templateUID);
            return RuleBuilder.create(rule).withTemplateState(TemplateState.TEMPLATE_MISSING).build();
            RuleImpl resolvedRule = (RuleImpl) RuleBuilder
                    .create(template, rule.getUID(), rule.getName(), rule.getConfiguration(), rule.getVisibility())
                    .build();
            resolveConfigurations(resolvedRule);
            return resolvedRule;
    protected void addProvider(Provider<Rule> provider) {
        super.addProvider(provider);
        forEach(provider, rule -> {
                Rule resolvedRule = resolveRuleByTemplate(rule);
                if (rule != resolvedRule && provider instanceof ManagedRuleProvider) {
                logger.error("Added rule '{}' is invalid", rule.getUID(), e);
    public void added(Provider<Rule> provider, Rule element) {
        String ruleUID = element.getUID();
        Rule resolvedRule = element;
            resolvedRule = resolveRuleByTemplate(element);
            logger.debug("Added rule '{}' is invalid", ruleUID, e);
        super.added(provider, element);
        if (element != resolvedRule) {
                super.updated(provider, element, resolvedRule);
    public void updated(Provider<Rule> provider, Rule oldElement, Rule element) {
        if (oldElement != null && uid.equals(oldElement.getUID())) {
                logger.error("The rule '{}' is not updated, the new version is invalid", uid, e);
            if (element != resolvedRule && provider instanceof ManagedRuleProvider) {
                super.updated(provider, oldElement, resolvedRule);
                    String.format("The rule '%s' is not updated, not matching with any existing rule", uid));
    protected void onAddElement(Rule element) throws IllegalArgumentException {
            resolveConfigurations(element);
            logger.debug("Added rule '{}' is invalid", uid, e);
    protected void onUpdateElement(Rule oldElement, Rule element) throws IllegalArgumentException {
            logger.debug("The new version of updated rule '{}' is invalid", uid, e);
     * This method serves to resolve and normalize the {@link Rule}s configuration values and its module configurations.
     * @param rule the {@link Rule}, whose configuration values and module configuration values should be resolved and
     *            normalized.
    private void resolveConfigurations(Rule rule) {
        List<ConfigDescriptionParameter> configDescriptions = rule.getConfigurationDescriptions();
        Configuration configuration = rule.getConfiguration();
        ConfigurationNormalizer.normalizeConfiguration(configuration,
                ConfigurationNormalizer.getConfigDescriptionMap(configDescriptions));
        Map<String, Object> configurationProperties = configuration.getProperties();
        if (templateState == TemplateState.INSTANTIATED || templateState == TemplateState.NO_TEMPLATE) {
            String uid = rule.getUID();
                validateConfiguration(configDescriptions, new HashMap<>(configurationProperties));
                resolveModuleConfigReferences(rule.getModules(), configurationProperties);
                ConfigurationNormalizer.normalizeModuleConfigurations(rule.getModules(), moduleTypeRegistry);
                throw new IllegalArgumentException(String.format("The rule '%s' has incorrect configurations", uid), e);
     * This method serves to validate the {@link Rule}s configuration values.
     * @param rule the {@link Rule}, whose configuration values should be validated.
    private void validateConfiguration(List<ConfigDescriptionParameter> configDescriptions,
            Map<String, Object> configurations) {
        if (configurations == null || configurations.isEmpty()) {
            if (isOptionalConfig(configDescriptions)) {
                StringBuilder statusDescription = new StringBuilder();
                String msg = " '%s';";
                for (ConfigDescriptionParameter configParameter : configDescriptions) {
                    if (configParameter.isRequired()) {
                        String name = configParameter.getName();
                        statusDescription.append(String.format(msg, name));
                        "Missing required configuration properties: " + statusDescription.toString());
                String configParameterName = configParameter.getName();
                processValue(configurations.remove(configParameterName), configParameter);
            if (!configurations.isEmpty()) {
                for (String name : configurations.keySet()) {
                throw new IllegalArgumentException("Extra configuration properties: " + statusDescription.toString());
     * Utility method for {@link Rule}s configuration validation.
     * @param configDescriptions the meta-data for {@link Rule}s configuration, used for validation.
     * @return {@code true} if all configuration properties are optional or {@code false} if there is at least one
     *         required property.
    private boolean isOptionalConfig(List<ConfigDescriptionParameter> configDescriptions) {
        if (configDescriptions != null && !configDescriptions.isEmpty()) {
            boolean required = false;
            for (ConfigDescriptionParameter param : configDescriptions) {
                required = required || param.isRequired();
            return !required;
     * Utility method for {@link Rule}s configuration validation. Validates the value of a configuration property.
     * @param configValue the value for {@link Rule}s configuration property, that should be validated.
     * @param configParameter the meta-data for {@link Rule}s configuration value, used for validation.
    private void processValue(@Nullable Object configValue, ConfigDescriptionParameter configParameter) {
        if (configValue != null) {
            Type type = configParameter.getType();
            if (configParameter.isMultiple()) {
                if (configValue instanceof List lConfigValues) {
                    for (Object value : lConfigValues) {
                        if (!checkType(type, value)) {
                            throw new IllegalArgumentException("Unexpected value for configuration property \""
                                    + configParameter.getName() + "\". Expected type: " + type);
                            "Unexpected value for configuration property \"" + configParameter.getName()
                                    + "\". Expected is Array with type for elements : " + type.toString() + "!");
            } else if (!checkType(type, configValue)) {
                        + configParameter.getName() + "\". Expected is " + type.toString() + "!");
        } else if (configParameter.isRequired()) {
                    "Required configuration property missing: \"" + configParameter.getName() + "\"!");
     * Avoid code duplication in {@link #processValue(Object, ConfigDescriptionParameter)} method.
     * @param type the {@link Type} of a parameter that should be checked.
     * @param configValue the value of a parameter that should be checked.
     * @return <code>true</code> if the type and value matching or <code>false</code> in the opposite.
    private boolean checkType(Type type, Object configValue) {
            case TEXT -> configValue instanceof String;
            case BOOLEAN -> configValue instanceof Boolean;
            case INTEGER -> configValue instanceof BigDecimal || configValue instanceof Integer
                    || configValue instanceof Double doubleValue && doubleValue.intValue() == doubleValue;
            case DECIMAL -> configValue instanceof BigDecimal || configValue instanceof Double;
     * This method serves to replace module configuration references with the {@link Rule}s configuration values.
     * @param modules the {@link Rule}'s modules, whose configuration values should be resolved.
     * @param ruleConfiguration the {@link Rule}'s configuration values that should be resolve module configuration
     *            values.
    private void resolveModuleConfigReferences(List<? extends Module> modules, Map<String, ?> ruleConfiguration) {
            for (Module module : modules) {
                    ReferenceResolver.updateConfiguration(module.getConfiguration(), ruleConfiguration, logger);
                    statusDescription.append(" in module[" + module.getId() + "]: " + e.getLocalizedMessage() + ";");
            String statusDescriptionStr = statusDescription.toString();
            if (!statusDescriptionStr.isEmpty()) {
                throw new IllegalArgumentException(String.format("Incorrect configurations: %s", statusDescriptionStr));
    public void added(RuleTemplate element) {
        processRuleStubs(element);
    public void removed(RuleTemplate element) {
        // Do nothing - resolved rules are independent from templates
    public void updated(RuleTemplate oldElement, RuleTemplate element) {
     * Processes any existing rule stubs (rules with a template specified that haven't yet been converted into "proper
     * rules") that references the specified rule template using the new or updated rule template.
     * @param template the {@link RuleTemplate} to use for processing matching rule stubs.
    protected void processRuleStubs(RuleTemplate template) {
        String templateUID = template.getUID();
        List<Rule> rules = stream().filter((r) -> PROCESSABLE_TEMPLATE_STATES.contains(r.getTemplateState())).toList();
        for (Rule unresolvedRule : rules) {
                Rule resolvedRule = resolveRuleByTemplate(unresolvedRule);
                Provider<Rule> provider = getProvider(unresolvedRule.getUID());
                } else if (provider != null) {
                    updated(provider, unresolvedRule, resolvedRule);
                    logger.error("Resolving the rule '{}' by template '{}' failed because the provider is not known",
                            unresolvedRule.getUID(), templateUID);
                logger.error("Resolving the rule '{}' by template '{}' failed", unresolvedRule.getUID(), templateUID,
