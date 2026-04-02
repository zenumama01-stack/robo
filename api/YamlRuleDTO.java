import org.openhab.core.model.yaml.internal.config.YamlConfigDescriptionParameterDTO;
 * The {@link YamlRuleDTO} is a data transfer object used to serialize a rule in a YAML configuration file.
@YamlElementName("rules")
public class YamlRuleDTO implements ModularDTO<YamlRuleDTO, ObjectMapper, JsonNode>, YamlElement, Cloneable {
    protected static final Pattern UID_SEGMENT_PATTERN = Pattern.compile("[a-zA-Z0-9_][a-zA-Z0-9_-]*");
    public String template;
    public TemplateState templateState;
    public Map<@NonNull String, @NonNull YamlConfigDescriptionParameterDTO> configDescriptions;
    public List<@NonNull YamlConditionDTO> conditions;
    public List<@NonNull YamlActionDTO> actions;
    public List<@NonNull YamlModuleDTO> triggers;
    public YamlRuleDTO() {
     * Creates a new instance based on the specified {@link Rule}.
     * @param rule the {@link Rule}.
    public YamlRuleDTO(@NonNull Rule rule) {
        this.template = rule.getTemplateUID();
        this.templateState = rule.getTemplateState();
        this.label = rule.getName();
        this.tags = rule.getTags();
        this.config = rule.getConfiguration().getProperties();
        List<@NonNull ConfigDescriptionParameter> configDescriptions = rule.getConfigurationDescriptions();
        if (!configDescriptions.isEmpty()) {
            Map<String, YamlConfigDescriptionParameterDTO> configDescriptionDtos = new LinkedHashMap<>(
                    configDescriptions.size());
                configDescriptionDtos.put(parameter.getName(), new YamlConfigDescriptionParameterDTO(parameter));
            this.configDescriptions = configDescriptionDtos;
        List<@NonNull Action> actions = rule.getActions();
        if (!actions.isEmpty()) {
            List<YamlActionDTO> actionDtos = new ArrayList<>(actions.size());
                actionDtos.add(new YamlActionDTO(action));
            this.actions = actionDtos;
        List<@NonNull Condition> conditions = rule.getConditions();
        if (!conditions.isEmpty()) {
            List<YamlConditionDTO> conditionsDtos = new ArrayList<>(conditions.size());
                conditionsDtos.add(new YamlConditionDTO(condition));
            this.conditions = conditionsDtos;
        List<@NonNull Trigger> triggers = rule.getTriggers();
            List<YamlModuleDTO> triggerDtos = new ArrayList<>(triggers.size());
                triggerDtos.add(new YamlModuleDTO(trigger));
            this.triggers = triggerDtos;
        return uid == null ? "" : uid;
        uid = id;
    public @NonNull YamlRuleDTO toDto(@NonNull JsonNode node, @NonNull ObjectMapper mapper)
            throws SerializationException {
        YamlPartialRuleDTO partial;
        YamlRuleDTO result = new YamlRuleDTO();
            partial = mapper.treeToValue(node, YamlPartialRuleDTO.class);
            result.uid = partial.uid;
            result.template = partial.template;
            result.templateState = TemplateState.typeOf(partial.templateState);
            result.label = partial.label;
            result.tags = partial.tags;
            result.description = partial.description;
            result.visibility = Visibility.typeOf(partial.visibility);
            if (result.visibility == null) {
                result.visibility = Visibility.VISIBLE;
            result.config = partial.config;
            if (partial.configDescriptions != null && partial.configDescriptions.isContainerNode()) {
                Map<String, YamlConfigDescriptionParameterDTO> configDescriptions = new LinkedHashMap<>(
                        partial.configDescriptions.size());
                YamlConfigDescriptionParameterDTO parameterDTO;
                if (partial.configDescriptions.isArray()) {
                    JsonNode parameterNode, nameNode;
                    for (Iterator<JsonNode> iterator = partial.configDescriptions.elements(); iterator.hasNext();) {
                        parameterNode = iterator.next();
                        if (parameterNode instanceof ObjectNode objectNode
                                && (nameNode = objectNode.remove("name")) != null && nameNode.isTextual()) {
                            parameterDTO = mapper.treeToValue(parameterNode, YamlConfigDescriptionParameterDTO.class);
                            configDescriptions.put(nameNode.asText(), parameterDTO);
                            if (!(parameterNode instanceof ObjectNode objectNode)) {
                                throw new SerializationException(
                                        "Invalid 'configDescriptions': Array must contain objects");
                            if (objectNode.get("name") == null) {
                                throw new SerializationException("Invalid 'configDescription': 'name' is mandatory");
                            throw new SerializationException("Invalid 'configDescription': 'name' must be a string");
                    for (Entry<String, JsonNode> parameter : partial.configDescriptions.properties()) {
                        parameterDTO = mapper.treeToValue(parameter.getValue(),
                                YamlConfigDescriptionParameterDTO.class);
                        configDescriptions.put(parameter.getKey(), parameterDTO);
                result.configDescriptions = configDescriptions;
            if (partial.actions != null && !partial.actions.isEmpty()) {
                if (!partial.actions.isArray()) {
                    throw new SerializationException("Expected actions to be an array node");
                List<YamlActionDTO> actions = new ArrayList<>(partial.actions.size());
                JsonNode actionNode;
                YamlActionDTO action;
                for (Iterator<JsonNode> iterator = partial.actions.elements(); iterator.hasNext();) {
                    actionNode = iterator.next();
                    action = mapper.treeToValue(actionNode, YamlActionDTO.class);
                    action.type = ModuleTypeAliases.aliasToType(Action.class, action.type);
                    translateMIMETypeAliases(action);
                    actions.add(action);
                result.actions = actions;
            if (partial.conditions != null && !partial.conditions.isEmpty()) {
                if (!partial.conditions.isArray()) {
                    throw new SerializationException("Expected conditions to be an array node");
                List<YamlConditionDTO> conditions = new ArrayList<>(partial.conditions.size());
                JsonNode conditionNode;
                YamlConditionDTO condition;
                for (Iterator<JsonNode> iterator = partial.conditions.elements(); iterator.hasNext();) {
                    conditionNode = iterator.next();
                    condition = mapper.treeToValue(conditionNode, YamlConditionDTO.class);
                    condition.type = ModuleTypeAliases.aliasToType(Condition.class, condition.type);
                    translateMIMETypeAliases(condition);
                result.conditions = conditions;
            if (partial.triggers != null && !partial.triggers.isEmpty()) {
                if (!partial.triggers.isArray()) {
                    throw new SerializationException("Expected triggers to be an array node");
                List<YamlModuleDTO> triggers = new ArrayList<>(partial.triggers.size());
                JsonNode triggerNode;
                YamlModuleDTO trigger;
                for (Iterator<JsonNode> iterator = partial.triggers.elements(); iterator.hasNext();) {
                    triggerNode = iterator.next();
                    trigger = mapper.treeToValue(triggerNode, YamlModuleDTO.class);
                    trigger.type = ModuleTypeAliases.aliasToType(Trigger.class, trigger.type);
                    translateMIMETypeAliases(trigger);
                result.triggers = triggers;
        } catch (JsonProcessingException | IllegalArgumentException e) {
            throw new SerializationException(e.getMessage(), e);
    private void translateMIMETypeAliases(YamlModuleDTO module) {
        Map<@NonNull String, @NonNull Object> config;
        String translatedType;
        if ((config = module.config) != null && config.containsKey("script")
                && config.get("type") instanceof String type) {
            if (!type.equals(translatedType = MIMETypeAliases.aliasToType(type))) {
                config.put("type", translatedType);
    public YamlRuleDTO cloneWithoutId() {
        YamlRuleDTO copy;
            copy = (YamlRuleDTO) super.clone();
            copy.uid = null;
            return new YamlRuleDTO();
        // Check that uid is present
        if (uid == null || uid.isBlank()) {
            addToList(errors, "invalid rule: uid is missing while mandatory");
        // Check that uid only contains valid characters
        String[] segments = uid.split(AbstractUID.SEPARATOR);
        for (String segment : segments) {
            if (!UID_SEGMENT_PATTERN.matcher(segment).matches()) {
                addToList(errors, "invalid rule \"%s\": segment \"%s\" in the uid doesn't match the expected syntax %s"
                        .formatted(uid, segment, UID_SEGMENT_PATTERN.pattern()));
        if (label == null || label.isBlank()) {
            addToList(errors, "invalid rule \"%s\": label is missing while mandatory".formatted(uid));
        // Check that the rule either has configuration (rule stub) or that it has at least one module
        if ((config == null || config.isEmpty()) && (triggers == null || triggers.isEmpty())
                && (conditions == null || conditions.isEmpty()) && (actions == null || actions.isEmpty())) {
            addToList(errors, "invalid rule \"%s\": the rule is empty".formatted(uid));
        // Check that module IDs are unique
        Set<String> ids = new HashSet<>();
        ok &= enumerateModuleIds(triggers, ids, errors);
        ok &= enumerateModuleIds(conditions, ids, errors);
        ok &= enumerateModuleIds(actions, ids, errors);
    private boolean enumerateModuleIds(@Nullable List<@NonNull ? extends YamlModuleDTO> modules,
            @NonNull Set<String> ids, @Nullable List<@NonNull String> errors) {
        if (modules == null) {
        for (YamlModuleDTO module : modules) {
            if ((id = module.id) == null || id.isBlank()) {
            if (ids.contains(id)) {
                String moduleType;
                if (module instanceof YamlActionDTO) {
                    moduleType = "action";
                } else if (module instanceof YamlConditionDTO) {
                    moduleType = "condition";
                    moduleType = "trigger";
                        "invalid rule \"%s\": Illegal %s ID \"%s\" - IDs must be unique across all modules in the rule"
                                .formatted(uid, moduleType, id));
            ids.add(id);
        return Objects.hash(actions, conditions, config, configDescriptions, description, label, tags, template,
                templateState, triggers, uid, visibility);
        if (!(obj instanceof YamlRuleDTO)) {
        YamlRuleDTO other = (YamlRuleDTO) obj;
        return Objects.equals(actions, other.actions) && Objects.equals(conditions, other.conditions)
                && Objects.equals(config, other.config) && Objects.equals(configDescriptions, other.configDescriptions)
                && Objects.equals(description, other.description) && Objects.equals(label, other.label)
                && Objects.equals(tags, other.tags) && Objects.equals(template, other.template)
                && templateState == other.templateState && Objects.equals(triggers, other.triggers)
                && Objects.equals(uid, other.uid) && visibility == other.visibility;
        builder.append("YamlRuleDTO [");
            builder.append("uid=").append(uid).append(", ");
            builder.append("template=").append(template).append(", ");
        if (templateState != null) {
            builder.append("templateState=").append(templateState).append(", ");
            builder.append("tags=").append(tags).append(", ");
        if (visibility != null) {
            builder.append("visibility=").append(visibility).append(", ");
            builder.append("config=").append(config).append(", ");
        if (configDescriptions != null) {
            builder.append("configDescriptions=").append(configDescriptions).append(", ");
            builder.append("conditions=").append(conditions).append(", ");
            builder.append("actions=").append(actions).append(", ");
            builder.append("triggers=").append(triggers);
     * A data transfer object for partial deserialization of a rule.
    protected static class YamlPartialRuleDTO {
        @JsonAlias({ "templateUid", "templateUID" })
        public String visibility;
        public JsonNode configDescriptions;
        public JsonNode conditions;
        public JsonNode actions;
        public JsonNode triggers;
