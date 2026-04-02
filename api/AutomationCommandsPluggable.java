 * This class provides functionality for defining and executing automation commands for importing, exporting, removing
 * and listing the automation objects.
public class AutomationCommandsPluggable extends AutomationCommands implements ConsoleCommandExtension {
     * This constant defines the command group name.
    public static final String NAME = "automation";
     * This constant describes the commands group.
    public static final String DESCRIPTION = "Commands for managing Automation Rules, Templates and ModuleTypes resources.";
     * This constant is defined for compatibility and is used to switch to a particular provider of {@code ModuleType}
     * automation objects.
    private static final int MODULE_TYPE_REGISTRY = 3;
     * This constant is defined for compatibility and is used to switch to a particular provider of {@code Template}
    private static final int TEMPLATE_REGISTRY = 2;
     * This constant is defined for compatibility and is used to switch to a particular provider of {@code Rule}
    private static final int RULE_REGISTRY = 1;
     * This field holds the reference to the {@code RuleRegistry} providing the {@code Rule} automation objects.
    protected final RuleRegistry ruleRegistry;
     * This field holds the reference to the {@code RuleManager}.
    protected final RuleManager ruleManager;
     * This field holds the reference to the {@code TemplateRegistry} providing the {@code Template} automation objects.
    protected final TemplateRegistry<RuleTemplate> templateRegistry;
     * This field holds the reference to the {@code ModuleTypeRegistry} providing the {@code ModuleType} automation
    protected final ModuleTypeRegistry moduleTypeRegistry;
    public AutomationCommandsPluggable(ComponentContext componentContext, //
            final @Reference ModuleTypeRegistry moduleTypeRegistry, //
            final @Reference TemplateRegistry<RuleTemplate> templateRegistry, //
            final @Reference RuleManager ruleManager) {
        initialize(componentContext.getBundleContext(), moduleTypeRegistry, templateRegistry, ruleRegistry);
    protected void deactivate(ComponentContext componentContext) {
        if (args.length == 0) {
            console.println(String.join("\n", getUsages()));
        String command = args[0];// the first argument is the subcommand name
        String[] params = new String[args.length - 1];// extract the remaining arguments except the first one
            System.arraycopy(args, 1, params, 0, params.length);
        String res = super.executeCommand(command, params);
        console.println(res);
                buildCommandUsage(LIST_MODULE_TYPES + " [-st] <filter> <language>",
                        "lists all Module Types. If filter is present, lists only matching Module Types."
                                + " If language is missing, the default language will be used."),
                buildCommandUsage(LIST_TEMPLATES + " [-st] <filter> <language>",
                        "lists all Templates. If filter is present, lists only matching Templates."
                buildCommandUsage(LIST_RULES + " [-st] <filter>",
                        "lists all Rules. If filter is present, lists only matching Rules"),
                buildCommandUsage(REMOVE_MODULE_TYPES + " [-st] <url>",
                        "Removes the Module Types, loaded from the given url"),
                buildCommandUsage(REMOVE_TEMPLATES + " [-st] <url>",
                        "Removes the Templates, loaded from the given url"),
                buildCommandUsage(REMOVE_RULE + " [-st] <uid>", "Removes the rule, specified by given UID"),
                buildCommandUsage(REMOVE_RULES + " [-st] <filter>",
                        "Removes the rules. If filter is present, removes only matching Rules"),
                buildCommandUsage(IMPORT_MODULE_TYPES + " [-p] <parserType> [-st] <url>",
                        "Imports Module Types from given url. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(IMPORT_TEMPLATES + " [-p] <parserType> [-st] <url>",
                        "Imports Templates from given url. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(IMPORT_RULES + " [-p] <parserType> [-st] <url>",
                        "Imports Rules from given url. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(EXPORT_MODULE_TYPES + " [-p] <parserType> [-st] <file>",
                        "Exports Module Types in a file. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(EXPORT_TEMPLATES + " [-p] <parserType> [-st] <file>",
                        "Exports Templates in a file. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(EXPORT_RULES + " [-p] <parserType> [-st] <file>",
                        "Exports Rules in a file. If parser type missing, \"json\" parser will be set as default"),
                buildCommandUsage(ENABLE_RULE + " [-st] <uid> <enable>",
                        "Enables the Rule, specified by given UID. If enable parameter is missing, "
                                + "the result of the command will be visualization of enabled/disabled state of the rule, "
                                + "if its value is \"true\" or \"false\", "
                                + "the result of the command will be to set enable/disable on the Rule."));
    public String getCommand() {
        return NAME;
        return DESCRIPTION;
    public @Nullable Rule getRule(String uid) {
        return ruleRegistry.get(uid);
    public @Nullable RuleTemplate getTemplate(String templateUID, @Nullable Locale locale) {
        return templateRegistry.get(templateUID, locale);
        return templateRegistry.getAll(locale);
    public @Nullable ModuleType getModuleType(String typeUID, @Nullable Locale locale) {
        return moduleTypeRegistry.get(typeUID, locale);
    public Collection<TriggerType> getTriggers(@Nullable Locale locale) {
        return moduleTypeRegistry.getTriggers(locale);
    public Collection<ConditionType> getConditions(@Nullable Locale locale) {
        return moduleTypeRegistry.getConditions(locale);
    public Collection<ActionType> getActions(@Nullable Locale locale) {
        return moduleTypeRegistry.getActions(locale);
    public String removeRule(String uid) {
        if (ruleRegistry.remove(uid) != null) {
            return String.format("Rule with id '%s' does not exist.", uid);
    public String removeRules(String ruleFilter) {
        for (Rule r : ruleRegistry.getAll()) {
            if (r.getUID().contains(ruleFilter)) {
                ruleRegistry.remove(r.getUID());
    protected @Nullable AutomationCommand parseCommand(String command, String[] params) {
        if (IMPORT_MODULE_TYPES.equalsIgnoreCase(command)) {
            return new AutomationCommandImport(IMPORT_MODULE_TYPES, params, MODULE_TYPE_REGISTRY, this);
        if (EXPORT_MODULE_TYPES.equalsIgnoreCase(command)) {
            return new AutomationCommandExport(EXPORT_MODULE_TYPES, params, MODULE_TYPE_REGISTRY, this);
        if (LIST_MODULE_TYPES.equalsIgnoreCase(command)) {
            return new AutomationCommandList(LIST_MODULE_TYPES, params, MODULE_TYPE_REGISTRY, this);
        if (IMPORT_TEMPLATES.equalsIgnoreCase(command)) {
            return new AutomationCommandImport(IMPORT_TEMPLATES, params, TEMPLATE_REGISTRY, this);
        if (EXPORT_TEMPLATES.equalsIgnoreCase(command)) {
            return new AutomationCommandExport(EXPORT_TEMPLATES, params, TEMPLATE_REGISTRY, this);
        if (LIST_TEMPLATES.equalsIgnoreCase(command)) {
            return new AutomationCommandList(LIST_TEMPLATES, params, TEMPLATE_REGISTRY, this);
        if (IMPORT_RULES.equalsIgnoreCase(command)) {
            return new AutomationCommandImport(IMPORT_RULES, params, RULE_REGISTRY, this);
        if (EXPORT_RULES.equalsIgnoreCase(command)) {
            return new AutomationCommandExport(EXPORT_RULES, params, RULE_REGISTRY, this);
        if (LIST_RULES.equalsIgnoreCase(command)) {
            return new AutomationCommandList(LIST_RULES, params, RULE_REGISTRY, this);
        if (REMOVE_TEMPLATES.equalsIgnoreCase(command)) {
            return new AutomationCommandRemove(REMOVE_TEMPLATES, params, TEMPLATE_REGISTRY, this);
        if (REMOVE_MODULE_TYPES.equalsIgnoreCase(command)) {
            return new AutomationCommandRemove(REMOVE_MODULE_TYPES, params, MODULE_TYPE_REGISTRY, this);
        if (REMOVE_RULE.equalsIgnoreCase(command)) {
            return new AutomationCommandRemove(REMOVE_RULE, params, RULE_REGISTRY, this);
        if (REMOVE_RULES.equalsIgnoreCase(command)) {
            return new AutomationCommandRemove(REMOVE_RULES, params, RULE_REGISTRY, this);
        if (ENABLE_RULE.equalsIgnoreCase(command)) {
            return new AutomationCommandEnableRule(ENABLE_RULE, params, RULE_REGISTRY, this);
     * Build a command usage string.
     * You should always use that function to use a usage string that complies
     * to a standard format.
     * @param description
     *            the description of the command
     * @return a usage string that complies to a standard format
    protected String buildCommandUsage(final String description) {
        return String.format("%s - %s", getCommand(), description);
     * @param syntax
     *            the syntax format
    protected String buildCommandUsage(final String syntax, final String description) {
        return String.format("%s %s - %s", getCommand(), syntax, description);
    public Collection<Rule> getRules() {
    public @Nullable RuleStatus getRuleStatus(String ruleUID) {
        RuleStatusInfo rsi = ruleManager.getStatusInfo(ruleUID);
        return rsi != null ? rsi.getStatus() : null;
        ruleManager.setEnabled(uid, isEnabled);
