 * {@link YamlRuleProvider} is an OSGi service, that allows definition of rules in YAML configuration files. Files can
 * be added, updated or removed at runtime. The rules are automatically registered with
 * {@link org.openhab.core.automation.RuleRegistry}.
@Component(immediate = true, service = { RuleProvider.class, YamlRuleProvider.class, YamlModelListener.class })
public class YamlRuleProvider extends AbstractYamlRuleProvider<Rule>
        implements RuleProvider, YamlModelListener<YamlRuleDTO> {
    private final Logger logger = LoggerFactory.getLogger(YamlRuleProvider.class);
    private final Map<String, Collection<Rule>> rulesMap = new ConcurrentHashMap<>();
    public YamlRuleProvider() {
        rulesMap.clear();
        return rulesMap.values().stream().flatMap(list -> list.stream()).toList();
    public Class<YamlRuleDTO> getElementClass() {
        return YamlRuleDTO.class;
    public void addedModel(String modelName, Collection<YamlRuleDTO> elements) {
        List<Rule> added = elements.stream().map(this::mapRule).filter(Objects::nonNull).toList();
        Collection<Rule> modelRules = Objects
                .requireNonNull(rulesMap.computeIfAbsent(modelName, k -> new ArrayList<>()));
        modelRules.addAll(added);
        added.forEach(r -> {
            logger.debug("model {} added rule {}", modelName, r.getUID());
            notifyListenersAboutAddedElement(r);
    public void updatedModel(String modelName, Collection<YamlRuleDTO> elements) {
        List<Rule> updated = elements.stream().map(this::mapRule).filter(Objects::nonNull).toList();
        updated.forEach(r -> {
            modelRules.stream().filter(rule -> rule.getUID().equals(r.getUID())).findFirst()
                    .ifPresentOrElse(oldRule -> {
                        modelRules.remove(oldRule);
                        modelRules.add(r);
                        logger.debug("model {} updated rule {}", modelName, r.getUID());
                        notifyListenersAboutUpdatedElement(oldRule, r);
    public void removedModel(String modelName, Collection<YamlRuleDTO> elements) {
        Collection<Rule> modelRules = rulesMap.getOrDefault(modelName, List.of());
        elements.stream().map(element -> element.uid).forEach(uid -> {
            modelRules.stream().filter(rule -> rule.getUID().equals(uid)).findFirst().ifPresentOrElse(oldRule -> {
                logger.debug("model {} removed rule {}", modelName, uid);
                notifyListenersAboutRemovedElement(oldRule);
            }, () -> logger.debug("model {} rule {} not found", modelName, uid));
        if (modelRules.isEmpty()) {
            rulesMap.remove(modelName);
    private @Nullable Rule mapRule(YamlRuleDTO ruleDto) {
        RuleBuilder ruleBuilder = RuleBuilder.create(ruleDto.uid);
        if ((s = ruleDto.label) != null) {
            ruleBuilder.withName(s);
        if ((s = ruleDto.template) != null) {
            ruleBuilder.withTemplateUID(s);
        if (ruleDto.templateState != null) {
            ruleBuilder.withTemplateState(ruleDto.templateState);
        Set<String> tags = ruleDto.tags;
            ruleBuilder.withTags(tags);
        if ((s = ruleDto.description) != null) {
            ruleBuilder.withDescription(s);
        if (ruleDto.visibility != null) {
            ruleBuilder.withVisibility(ruleDto.visibility);
        Map<String, Object> configuration = ruleDto.config;
            ruleBuilder.withConfiguration(new Configuration(configuration));
        Map<String, YamlConfigDescriptionParameterDTO> configDescriptionDtos = ruleDto.configDescriptions;
        if (configDescriptionDtos != null) {
            ruleBuilder.withConfigurationDescriptions(
                    YamlConfigDescriptionParameterDTO.mapConfigDescriptions(configDescriptionDtos));
        List<YamlModuleDTO> triggerDTOs = ruleDto.triggers;
        List<YamlConditionDTO> conditionDTOs = ruleDto.conditions;
        List<YamlActionDTO> actionDTOs = ruleDto.actions;
        List<Trigger> triggers = null;
        if (triggerDTOs != null) {
                triggers = mapModules(triggerDTOs, extractModuleIds(conditionDTOs, actionDTOs), Trigger.class);
            } catch (SerializationException e) {
                logger.error("Could not parse triggers for rule {}: {}", ruleDto.uid, e.getMessage());
            ruleBuilder.withTriggers(triggers);
        List<Condition> conditions = null;
        if (conditionDTOs != null) {
                conditions = mapModules(conditionDTOs, extractModuleIds(triggers, actionDTOs), Condition.class);
                logger.error("Could not parse conditions for rule {}: {}", ruleDto.uid, e.getMessage());
            ruleBuilder.withConditions(conditions);
        if (actionDTOs != null) {
                ruleBuilder.withActions(mapModules(actionDTOs, extractModuleIds(triggers, conditions), Action.class));
                logger.error("Could not parse actions for rule {}: {}", ruleDto.uid, e.getMessage());
        return ruleBuilder.build();
