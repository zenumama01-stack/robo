import org.openhab.core.automation.module.script.rulesupport.internal.ScriptedCustomModuleHandlerFactory;
import org.openhab.core.automation.module.script.rulesupport.internal.ScriptedCustomModuleTypeProvider;
import org.openhab.core.automation.module.script.rulesupport.internal.ScriptedPrivateModuleHandlerFactory;
import org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleRuleActionHandler;
import org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleRuleActionHandlerDelegate;
import org.openhab.core.automation.type.ConditionType;
import org.openhab.core.automation.util.RuleBuilder;
 * This Registry is used for a single ScriptEngine instance. It allows the adding and removing of handlers.
 * It allows the removal of previously added modules on unload.
public class ScriptedAutomationManager {
    private final RuleSupportRuleRegistryDelegate ruleRegistryDelegate;
    private final Set<String> modules = new HashSet<>();
    private final Set<String> moduleHandlers = new HashSet<>();
    private final Set<String> privateHandlers = new HashSet<>();
    public ScriptedAutomationManager(RuleSupportRuleRegistryDelegate ruleRegistryDelegate,
            ScriptedCustomModuleHandlerFactory scriptedCustomModuleHandlerFactory,
            ScriptedCustomModuleTypeProvider scriptedCustomModuleTypeProvider,
            ScriptedPrivateModuleHandlerFactory scriptedPrivateModuleHandlerFactory) {
        this.ruleRegistryDelegate = ruleRegistryDelegate;
    public void removeModuleType(String uid) {
        if (modules.remove(uid)) {
            scriptedCustomModuleTypeProvider.removeModuleType(uid);
            removeHandler(uid);
    public void removeHandler(String typeUID) {
        if (moduleHandlers.remove(typeUID)) {
            scriptedCustomModuleHandlerFactory.removeModuleHandler(typeUID);
    public void removePrivateHandler(String privId) {
        if (privateHandlers.remove(privId)) {
            scriptedPrivateModuleHandlerFactory.removeHandler(privId);
        Set<String> types = new HashSet<>(modules);
        for (String moduleType : types) {
            removeModuleType(moduleType);
        Set<String> moduleHandlers = new HashSet<>(this.moduleHandlers);
        for (String uid : moduleHandlers) {
        Set<String> privateHandlers = new HashSet<>(this.privateHandlers);
        for (String privId : privateHandlers) {
            removePrivateHandler(privId);
        ruleRegistryDelegate.removeAllAddedByScript();
    public Rule addRule(Rule element) {
        Rule rule = addUnmanagedRule(element);
        ruleRegistryDelegate.add(rule);
        return rule;
    public Rule addUnmanagedRule(Rule element) {
        RuleBuilder builder = RuleBuilder.create(element.getUID());
        String name = element.getName();
        if (name == null || name.isEmpty()) {
            name = element.getClass().getSimpleName();
            if (name.contains("$")) {
                name = name.substring(0, name.indexOf('$'));
        builder.withName(name).withDescription(element.getDescription()).withTags(element.getTags());
        // used for numbering the modules of the rule
        int moduleIndex = 1;
            List<Condition> conditions = new ArrayList<>();
            for (Condition cond : element.getConditions()) {
                Condition toAdd = cond;
                if (cond.getId().isEmpty()) {
                    toAdd = ModuleBuilder.createCondition().withId(Integer.toString(moduleIndex++))
                            .withTypeUID(cond.getTypeUID()).withConfiguration(cond.getConfiguration())
                            .withInputs(cond.getInputs()).build();
                conditions.add(toAdd);
            builder.withConditions(conditions);
            // conditions are optional
            List<Trigger> triggers = new ArrayList<>();
            for (Trigger trigger : element.getTriggers()) {
                Trigger toAdd = trigger;
                if (trigger.getId().isEmpty()) {
                    toAdd = ModuleBuilder.createTrigger().withId(Integer.toString(moduleIndex++))
                            .withTypeUID(trigger.getTypeUID()).withConfiguration(trigger.getConfiguration()).build();
                triggers.add(toAdd);
            builder.withTriggers(triggers);
            // triggers are optional
        List<Action> actions = new ArrayList<>(element.getActions());
        if (element instanceof SimpleRuleActionHandler handler) {
            String privId = addPrivateActionHandler(new SimpleRuleActionHandlerDelegate(handler));
            Action scriptedAction = ActionBuilder.create().withId(Integer.toString(moduleIndex++))
                    .withTypeUID("jsr223.ScriptedAction").withConfiguration(new Configuration()).build();
            scriptedAction.getConfiguration().put("privId", privId);
            actions.add(scriptedAction);
        builder.withConfigurationDescriptions(element.getConfigurationDescriptions());
        builder.withConfiguration(element.getConfiguration());
        builder.withActions(actions);
    public void addConditionType(ConditionType conditionType) {
        modules.add(conditionType.getUID());
        scriptedCustomModuleTypeProvider.addModuleType(conditionType);
    public void addConditionHandler(String uid, ScriptedHandler conditionHandler) {
        moduleHandlers.add(uid);
        scriptedCustomModuleHandlerFactory.addModuleHandler(uid, conditionHandler);
        scriptedCustomModuleTypeProvider.updateModuleHandler(uid);
    public String addPrivateConditionHandler(SimpleConditionHandler conditionHandler) {
        String uid = scriptedPrivateModuleHandlerFactory.addHandler(conditionHandler);
        privateHandlers.add(uid);
    public void addActionType(ActionType actionType) {
        modules.add(actionType.getUID());
        scriptedCustomModuleTypeProvider.addModuleType(actionType);
    public void addActionHandler(String uid, ScriptedHandler actionHandler) {
        scriptedCustomModuleHandlerFactory.addModuleHandler(uid, actionHandler);
    public String addPrivateActionHandler(SimpleActionHandler actionHandler) {
        String uid = scriptedPrivateModuleHandlerFactory.addHandler(actionHandler);
    public void addTriggerType(TriggerType triggerType) {
        modules.add(triggerType.getUID());
        scriptedCustomModuleTypeProvider.addModuleType(triggerType);
    public void addTriggerHandler(String uid, ScriptedHandler triggerHandler) {
        scriptedCustomModuleHandlerFactory.addModuleHandler(uid, triggerHandler);
    public String addPrivateTriggerHandler(SimpleTriggerHandler triggerHandler) {
        String uid = scriptedPrivateModuleHandlerFactory.addHandler(triggerHandler);
