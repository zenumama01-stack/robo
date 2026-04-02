 * This class provides the functionality responsible for printing the automation objects as a result of commands.
 * @author Yordan Mihaylov - updates related to api changes
public class Printer {
    private static final int TABLE_WIDTH = 100;
    private static final int COLUMN_ID = 7;
    private static final int COLUMN_UID = 93;
    private static final int COLUMN_RULE_UID = 36;
    private static final int COLUMN_RULE_NAME = 36;
    private static final int COLUMN_RULE_STATUS = 15;
    private static final int COLUMN_PROPERTY = 28;
    private static final int COLUMN_PROPERTY_VALUE = 72;
    private static final int COLUMN_CONFIG_PARAMETER = 20;
    private static final int COLUMN_CONFIG_PARAMETER_VALUE = 52;
    private static final int COLUMN_CONFIG_PARAMETER_PROP = 16;
    private static final int COLUMN_CONFIG_PARAMETER_PROP_VALUE = 36;
    private static final String ID = "ID";
    private static final String UID = "UID";
    private static final String NAME = "NAME";
    private static final String STATUS = "STATUS";
    private static final String TAGS = "TAGS";
    private static final String LABEL = "LABEL";
    private static final String VISIBILITY = "VISIBILITY";
    private static final String DESCRIPTION = "DESCRIPTION";
    private static final String CONFIGURATION_DESCRIPTIONS = "CONFIGURATION DESCRIPTIONS ";
    private static final String ACTIONS = "ACTIONS";
    private static final String TRIGGERS = "TRIGGERS";
    private static final String CONDITIONS = "CONDITIONS";
    private static final String INPUTS = "INPUTS";
    private static final String OUTPUTS = "OUTPUTS";
    private static final String CHILDREN = "CHILDREN";
    private static final String TYPE = "TYPE";
    private static final String CONFIGURATION = "CONFIGURATION";
    private static final String MIN = "MIN";
    private static final String MAX = "MAX";
    private static final String DEFAULT = "DEFAULT";
    private static final String CONTEXT = "CONTEXT";
    private static final String PATTERN = "PATTERN";
    private static final String OPTIONS = "OPTIONS";
    private static final String STEP_SIZE = "STEP_SIZE";
    private static final String FILTER_CRITERIA = "FILTER CRITERIA ";
    private static final String REQUIRED = "REQUIRED";
    private static final String NOT_REQUIRED = "NOT REQUIRED";
     * This method is responsible for printing the list with indexes, UIDs, names and statuses of the {@link Rule}s.
     * @param autoCommands
     * @param ruleUIDs
    static String printRules(AutomationCommandsPluggable autoCommands, Map<String, String> ruleUIDs) {
        int[] columnWidths = new int[] { COLUMN_ID, COLUMN_RULE_UID, COLUMN_RULE_NAME, COLUMN_RULE_STATUS };
        List<String> columnValues = new ArrayList<>();
        columnValues.add(ID);
        columnValues.add(UID);
        columnValues.add(NAME);
        columnValues.add(STATUS);
        String titleRow = Utils.getRow(columnWidths, columnValues);
        List<String> rulesRows = new ArrayList<>();
        for (int i = 1; i <= ruleUIDs.size(); i++) {
            String id = String.valueOf(i);
            String uid = ruleUIDs.get(id);
                columnValues.set(0, id);
                columnValues.set(1, uid);
                Rule rule = autoCommands.getRule(uid);
                columnValues.set(2, rule.getName());
                columnValues.set(3, autoCommands.getRuleStatus(uid).toString());
                rulesRows.add(Utils.getRow(columnWidths, columnValues));
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, rulesRows, titleRow);
     * This method is responsible for printing the list with indexes and UIDs of the {@link Template}s.
     * @param templateUIDs is a map with keys UIDs of the {@link Template}s and values the {@link Template}s.
     * @return a formated string, representing the sorted list with indexed UIDs of the available {@link Template}s.
    static String printTemplates(Map<String, String> templateUIDs) {
        int[] columnWidths = new int[] { COLUMN_ID, COLUMN_UID };
        List<String> columnTitles = new ArrayList<>();
        columnTitles.add(ID);
        columnTitles.add(UID);
        String titleRow = Utils.getRow(columnWidths, columnTitles);
        List<String> templates = new ArrayList<>();
        collectListRecords(templateUIDs, templates, columnWidths);
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, templates, titleRow);
     * This method is responsible for printing the list with indexes and UIDs of the {@link ModuleType}s.
     * @param moduleTypeUIDs is a map with keys UIDs of the {@link ModuleType}s and values the {@link ModuleType}s.
     * @return a formated string, representing the sorted list with indexed UIDs of the available {@link ModuleType}s.
    static String printModuleTypes(Map<String, String> moduleTypeUIDs) {
        List<String> moduleTypes = new ArrayList<>();
        collectListRecords(moduleTypeUIDs, moduleTypes, columnWidths);
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, moduleTypes, titleRow);
     * This method is responsible for printing the {@link Rule}.
     * @param rule the {@link Rule} for printing.
     * @return a formated string, representing the {@link Rule} info.
    static String printRule(Rule rule, RuleStatus status) {
        int[] columnWidths = new int[] { TABLE_WIDTH };
        List<String> ruleProperty = new ArrayList<>();
        ruleProperty.add(rule.getUID() + " [ " + status + " ]");
        String titleRow = Utils.getRow(columnWidths, ruleProperty);
        List<String> ruleContent = new ArrayList<>();
        columnWidths = new int[] { COLUMN_PROPERTY, COLUMN_PROPERTY_VALUE };
        ruleProperty.set(0, UID);
        ruleProperty.add(rule.getUID());
        ruleContent.add(Utils.getRow(columnWidths, ruleProperty));
        if (rule.getName() != null) {
            ruleProperty.set(0, NAME);
            ruleProperty.set(1, rule.getName());
        if (rule.getDescription() != null) {
            ruleProperty.set(0, DESCRIPTION);
            ruleProperty.set(1, rule.getDescription());
        ruleProperty.set(0, TAGS);
        ruleProperty.set(1, getTagsRecord(rule.getTags()));
        ruleContent.addAll(
                collectRecords(columnWidths, CONFIGURATION, rule.getConfiguration().getProperties().entrySet()));
        ruleContent.addAll(collectRecords(columnWidths, CONFIGURATION_DESCRIPTIONS,
                getConfigurationDescriptionRecords(rule.getConfigurationDescriptions())));
        ruleContent.addAll(collectRecords(columnWidths, TRIGGERS, rule.getTriggers()));
        ruleContent.addAll(collectRecords(columnWidths, CONDITIONS, rule.getConditions()));
        ruleContent.addAll(collectRecords(columnWidths, ACTIONS, rule.getActions()));
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, ruleContent, titleRow);
     * This method is responsible for printing the {@link Template}.
     * @param template the {@link Template} for printing.
     * @return a formated string, representing the {@link Template} info.
    static String printTemplate(Template template) {
        List<String> templateProperty = new ArrayList<>();
        templateProperty.add(template.getUID());
        String titleRow = Utils.getRow(columnWidths, templateProperty);
        List<String> templateContent = new ArrayList<>();
        templateProperty.set(0, UID);
        templateContent.add(Utils.getRow(columnWidths, templateProperty));
        if (template.getLabel() != null) {
            templateProperty.set(0, LABEL);
            templateProperty.set(1, template.getLabel());
        if (template.getDescription() != null) {
            templateProperty.set(0, DESCRIPTION);
            templateProperty.set(1, template.getDescription());
        templateProperty.set(0, VISIBILITY);
        templateProperty.set(1, template.getVisibility().toString());
        templateProperty.set(0, TAGS);
        templateProperty.set(1, getTagsRecord(template.getTags()));
        if (template instanceof RuleTemplate ruleTemplate) {
            templateContent.addAll(collectRecords(columnWidths, CONFIGURATION_DESCRIPTIONS,
                    getConfigurationDescriptionRecords(ruleTemplate.getConfigurationDescriptions())));
            templateContent.addAll(collectRecords(columnWidths, TRIGGERS, ruleTemplate.getTriggers()));
            templateContent.addAll(collectRecords(columnWidths, CONDITIONS, ruleTemplate.getConditions()));
            templateContent.addAll(collectRecords(columnWidths, ACTIONS, ruleTemplate.getActions()));
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, templateContent, titleRow);
     * This method is responsible for printing the {@link ModuleType}.
     * @param moduleType the {@link ModuleType} for printing.
     * @return a formated string, representing the {@link ModuleType} info.
    static String printModuleType(ModuleType moduleType) {
        List<String> moduleTypeProperty = new ArrayList<>();
        moduleTypeProperty.add(moduleType.getUID());
        String titleRow = Utils.getRow(columnWidths, moduleTypeProperty);
        List<String> moduleTypeContent = new ArrayList<>();
        moduleTypeProperty.set(0, UID);
        moduleTypeContent.add(Utils.getRow(columnWidths, moduleTypeProperty));
        if (moduleType.getLabel() != null) {
            moduleTypeProperty.set(0, LABEL);
            moduleTypeProperty.set(1, moduleType.getLabel());
        if (moduleType.getDescription() != null) {
            moduleTypeProperty.set(0, DESCRIPTION);
            moduleTypeProperty.set(1, moduleType.getDescription());
        moduleTypeProperty.set(0, VISIBILITY);
        moduleTypeProperty.set(1, moduleType.getVisibility().toString());
        moduleTypeProperty.set(0, TAGS);
        moduleTypeProperty.set(1, getTagsRecord(moduleType.getTags()));
        moduleTypeContent.addAll(collectRecords(columnWidths, CONFIGURATION_DESCRIPTIONS,
                getConfigurationDescriptionRecords(moduleType.getConfigurationDescriptions())));
        if (moduleType instanceof TriggerType type) {
            moduleTypeContent.addAll(collectRecords(columnWidths, OUTPUTS, type.getOutputs()));
        if (moduleType instanceof ConditionType type) {
            moduleTypeContent.addAll(collectRecords(columnWidths, INPUTS, type.getInputs()));
        if (moduleType instanceof ActionType type) {
        if (moduleType instanceof CompositeTriggerType type) {
            moduleTypeContent.addAll(collectRecords(columnWidths, CHILDREN, type.getChildren()));
        if (moduleType instanceof CompositeConditionType type) {
        if (moduleType instanceof CompositeActionType type) {
        return Utils.getTableContent(TABLE_WIDTH, columnWidths, moduleTypeContent, titleRow);
     * This method is responsible for printing the {@link RuleStatus}.
     * @param ruleUID specifies the rule, which status is requested.
     * @param status corresponds to the status of specified rule.
     * @return a string representing the response of the command {@link AutomationCommands#ENABLE_RULE}.
    static String printRuleStatus(String ruleUID, RuleStatus status) {
        List<String> title = new ArrayList<>();
        title.add(ruleUID + " [ " + status + " ]");
        String titleRow = Utils.getRow(new int[] { TABLE_WIDTH }, title);
        List<String> res = Utils.getTableTitle(titleRow, TABLE_WIDTH);
        for (String line : res) {
            sb.append(line + Utils.ROW_END);
     * This method is responsible for printing the strings, representing the auxiliary automation objects.
     * @param columnWidths represents the column widths of the table.
     * @param prop is a property name of the property with value the collection of the auxiliary automation objects for
     *            printing.
     * @param list with the auxiliary automation objects for printing.
     * @return list of strings, representing the auxiliary automation objects.
    private static List<String> collectRecords(int[] columnWidths, String prop, Collection<?> list) {
        List<String> res = new ArrayList<>();
        boolean isFirst = true;
        boolean isList = false;
        List<String> values = new ArrayList<>();
        values.add(prop);
        values.add("");
        if (list != null && !list.isEmpty()) {
            for (Object element : list) {
                if (element instanceof String string) {
                    res.add(Utils.getColumn(columnWidths[0], values.getFirst()) + string);
                        values.set(0, "");
                } else if (element instanceof Module module) {
                    List<String> moduleRecords = getModuleRecords(module);
                    for (String elementRecord : moduleRecords) {
                        res.add(Utils.getColumn(columnWidths[0], values.getFirst()) + elementRecord);
                    isList = true;
                        values.set(1, "[");
                        res.add(Utils.getRow(columnWidths, values));
                    if (element instanceof FilterCriteria criteria) {
                        values.set(1, getFilterCriteriaRecord(criteria));
                    } else if (element instanceof ParameterOption option) {
                        values.set(1, getParameterOptionRecord(option));
                    } else if (element instanceof Input input) {
                        values.set(1, getInputRecord(input));
                    } else if (element instanceof Output output) {
                        values.set(1, getOutputRecord(output));
                    } else if (element instanceof Entry) {
                        values.set(1, "  " + ((Entry<String, ?>) element).getKey() + " = \""
                                + ((Entry<String, ?>) element).getValue().toString() + "\"");
            if (isList) {
                values.set(1, "]");
     * This method is responsible for printing the {@link Module}.
     * @param module the {@link Module} for printing.
     * @return a formated string, representing the {@link Module}.
    private static List<String> getModuleRecords(Module module) {
        int[] columnWidths = new int[] { COLUMN_PROPERTY_VALUE };
        columnValues.add(module.getId());
        List<String> moduleContent = new ArrayList<>(
                Utils.getTableTitle(Utils.getRow(columnWidths, columnValues), COLUMN_PROPERTY_VALUE));
        columnWidths = new int[] { COLUMN_CONFIG_PARAMETER, COLUMN_CONFIG_PARAMETER_VALUE };
        columnValues.set(0, ID);
        moduleContent.add(Utils.getRow(columnWidths, columnValues));
        if (module.getLabel() != null) {
            columnValues.set(0, LABEL);
            columnValues.set(1, module.getLabel());
        if (module.getDescription() != null) {
            columnValues.set(0, DESCRIPTION);
            columnValues.set(1, module.getDescription());
        columnValues.set(0, TYPE);
        columnValues.set(1, module.getTypeUID());
        moduleContent.addAll(
                collectRecords(columnWidths, CONFIGURATION, module.getConfiguration().getProperties().entrySet()));
        Map<String, String> inputs = null;
        if (module instanceof Condition condition) {
            inputs = condition.getInputs();
            inputs = action.getInputs();
        if (inputs != null && !inputs.isEmpty()) {
            moduleContent.addAll(collectRecords(columnWidths, INPUTS, new ArrayList<>(inputs.entrySet())));
        return moduleContent;
    private static String getParameterOptionRecord(ParameterOption option) {
        return "  value=\"" + option.getValue() + "\", label=\"" + option.getLabel() + "\"";
    private static String getFilterCriteriaRecord(FilterCriteria criteria) {
        return "  name=\"" + criteria.getName() + "\", value=\"" + criteria.getValue() + "\"";
    private static String getInputRecord(Input input) {
        return "  name=\"" + input.getName() + "\", label=\"" + input.getLabel() + "\", decription=\""
                + input.getDescription() + "\", type=\"" + input.getType() + "\", "
                + (input.isRequired() ? REQUIRED : NOT_REQUIRED)
                + (input.getDefaultValue() != null ? "\", default=\"" + input.getDefaultValue() : "");
    private static String getOutputRecord(Output output) {
        return "  name=\"" + output.getName() + "\", label=\"" + output.getLabel() + "\", decription=\""
                + output.getDescription() + "\", type=\"" + output.getType() + "\"";
     * This method is responsible for printing the set of {@link ConfigDescriptionParameter}s.
     * @param configDescriptions set of {@link ConfigDescriptionParameter}s for printing.
     * @return a formated string, representing the set of {@link ConfigDescriptionParameter}s.
    private static List<String> getConfigurationDescriptionRecords(
            List<ConfigDescriptionParameter> configDescriptions) {
        List<String> configParamContent = new ArrayList<>();
            for (ConfigDescriptionParameter parameter : configDescriptions) {
                int[] columnWidths = new int[] { COLUMN_CONFIG_PARAMETER, COLUMN_CONFIG_PARAMETER_PROP,
                        COLUMN_CONFIG_PARAMETER_PROP_VALUE };
                configParamContent.add(Utils.getColumn(COLUMN_PROPERTY_VALUE, parameter.getName() + " : "));
                List<String> configParamProperty = new ArrayList<>();
                configParamProperty.add("");
                configParamProperty.add(TYPE);
                configParamProperty.add(parameter.getType().toString());
                configParamContent.add(Utils.getRow(columnWidths, configParamProperty));
                if (parameter.getLabel() != null) {
                    configParamProperty.set(1, LABEL);
                    configParamProperty.set(2, parameter.getLabel());
                if (parameter.getDescription() != null) {
                    configParamProperty.set(1, DESCRIPTION);
                    configParamProperty.set(2, parameter.getDescription());
                if (parameter.getDefault() != null) {
                    configParamProperty.set(1, DEFAULT);
                    configParamProperty.set(2, parameter.getDefault());
                if (parameter.getContext() != null) {
                    configParamProperty.set(1, CONTEXT);
                    configParamProperty.set(2, parameter.getContext());
                if (parameter.getPattern() != null) {
                    configParamProperty.set(1, PATTERN);
                    configParamProperty.set(2, parameter.getPattern());
                if (parameter.getStepSize() != null) {
                    configParamProperty.set(1, STEP_SIZE);
                    configParamProperty.set(2, parameter.getStepSize().toString());
                if (parameter.getMinimum() != null) {
                    configParamProperty.set(1, MIN);
                    configParamProperty.set(2, parameter.getMinimum().toString());
                if (parameter.getMaximum() != null) {
                    configParamProperty.set(1, MAX);
                    configParamProperty.set(2, parameter.getMaximum().toString());
                columnWidths = new int[] { COLUMN_CONFIG_PARAMETER_PROP, COLUMN_CONFIG_PARAMETER_PROP_VALUE };
                List<String> options = collectRecords(columnWidths, OPTIONS, parameter.getOptions());
                for (String option : options) {
                    configParamContent.add(Utils.getColumn(COLUMN_CONFIG_PARAMETER, "") + option);
                List<String> filters = collectRecords(columnWidths, FILTER_CRITERIA, parameter.getFilterCriteria());
                for (String filter : filters) {
                    configParamContent.add(Utils.getColumn(COLUMN_CONFIG_PARAMETER, "") + filter);
                configParamContent
                        .add(Utils.getColumn(COLUMN_PROPERTY_VALUE, Utils.printChars('-', COLUMN_PROPERTY_VALUE)));
        return configParamContent;
     * This method is responsible for printing the set of tags.
     * @param tags is the set of tags for printing.
     * @return a formatted string, representing the set of tags.
    private static String getTagsRecord(Set<String> tags) {
            return "[ ]";
        StringBuilder res = new StringBuilder().append("[ ");
        for (String tag : tags) {
            if (i < tags.size()) {
                res.append(tag + ", ");
                res.append(tag);
        return res.append(" ]").toString();
     * This method is responsible for constructing the rows of a table with 2 columns - first column is for the
     * numbering, second column is for the numbered records.
     * @param list is the list with row values for printing.
     * @param rows is used for accumulation of result
    private static void collectListRecords(Map<String, String> list, List<String> rows, int[] columnWidths) {
        for (int i = 1; i <= list.size(); i++) {
            columnValues.add(id);
            columnValues.add(uid);
            rows.add(Utils.getRow(columnWidths, columnValues));
