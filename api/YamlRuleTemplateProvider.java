 * {@link YamlRuleTemplateProvider} is an OSGi service, that allows definition of rule templates in YAML configuration
 * files. Files can be added, updated or removed at runtime. The rule templates are automatically registered with
 * {@link org.openhab.core.automation.internal.template.RuleTemplateRegistry}.
@Component(immediate = true, service = { RuleTemplateProvider.class, YamlRuleTemplateProvider.class,
        YamlModelListener.class })
public class YamlRuleTemplateProvider extends AbstractYamlRuleProvider<RuleTemplate>
        implements RuleTemplateProvider, YamlModelListener<YamlRuleTemplateDTO> {
    private final Logger logger = LoggerFactory.getLogger(YamlRuleTemplateProvider.class);
    private final Map<String, Collection<RuleTemplate>> ruleTemplatesMap = new ConcurrentHashMap<>();
    public YamlRuleTemplateProvider() {
        ruleTemplatesMap.clear();
        return ruleTemplatesMap.values().stream().flatMap(list -> list.stream()).filter(t -> uid.equals(t.getUID()))
        return ruleTemplatesMap.values().stream().flatMap(list -> list.stream()).toList();
    public Class<YamlRuleTemplateDTO> getElementClass() {
        return YamlRuleTemplateDTO.class;
    public void addedModel(String modelName, Collection<YamlRuleTemplateDTO> elements) {
        List<RuleTemplate> added = elements.stream().map(this::mapRuleTemplate).filter(Objects::nonNull).toList();
        Collection<RuleTemplate> modelRuleTemplates = Objects
                .requireNonNull(ruleTemplatesMap.computeIfAbsent(modelName, k -> new ArrayList<>()));
        modelRuleTemplates.addAll(added);
        added.forEach(t -> {
            logger.debug("model {} added rule template {}", modelName, t.getUID());
            notifyListenersAboutAddedElement(t);
    public void updatedModel(String modelName, Collection<YamlRuleTemplateDTO> elements) {
        List<RuleTemplate> updated = elements.stream().map(this::mapRuleTemplate).filter(Objects::nonNull).toList();
        updated.forEach(t -> {
            modelRuleTemplates.stream().filter(template -> template.getUID().equals(t.getUID())).findFirst()
                    .ifPresentOrElse(oldTemplate -> {
                        modelRuleTemplates.remove(oldTemplate);
                        modelRuleTemplates.add(t);
                        logger.debug("model {} updated rule template {}", modelName, t.getUID());
                        notifyListenersAboutUpdatedElement(oldTemplate, t);
    public void removedModel(String modelName, Collection<YamlRuleTemplateDTO> elements) {
        Collection<RuleTemplate> modelRuleTemplates = ruleTemplatesMap.getOrDefault(modelName, List.of());
            modelRuleTemplates.stream().filter(template -> template.getUID().equals(uid)).findFirst()
                        logger.debug("model {} removed rule template {}", modelName, uid);
                        notifyListenersAboutRemovedElement(oldTemplate);
                    }, () -> logger.debug("model {} rule template {} not found", modelName, uid));
        if (modelRuleTemplates.isEmpty()) {
            ruleTemplatesMap.remove(modelName);
    private @Nullable RuleTemplate mapRuleTemplate(YamlRuleTemplateDTO ruleTemplateDto) {
        Map<String, YamlConfigDescriptionParameterDTO> configDescriptionDtos = ruleTemplateDto.configDescriptions;
        List<ConfigDescriptionParameter> configDescriptions = null;
            configDescriptions = YamlConfigDescriptionParameterDTO.mapConfigDescriptions(configDescriptionDtos);
        List<YamlModuleDTO> triggerDTOs = ruleTemplateDto.triggers;
        List<YamlConditionDTO> conditionDTOs = ruleTemplateDto.conditions;
        List<YamlActionDTO> actionDTOs = ruleTemplateDto.actions;
                logger.error("Could not parse triggers for rule template {}: {}", ruleTemplateDto.uid, e.getMessage());
                logger.error("Could not parse conditions for rule template {}: {}", ruleTemplateDto.uid,
        List<Action> actions = null;
                actions = mapModules(actionDTOs, extractModuleIds(triggers, conditions), Action.class);
                logger.error("Could not parse actions for rule template {}: {}", ruleTemplateDto.uid, e.getMessage());
                ruleTemplateDto.tags, triggers, conditions, actions, configDescriptions, ruleTemplateDto.visibility);
