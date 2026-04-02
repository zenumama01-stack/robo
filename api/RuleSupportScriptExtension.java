import org.openhab.core.automation.Rule;
import org.openhab.core.automation.RuleRegistry;
import org.openhab.core.automation.module.script.rulesupport.shared.RuleSupportRuleRegistryDelegate;
import org.openhab.core.automation.module.script.rulesupport.shared.ScriptedAutomationManager;
import org.openhab.core.automation.module.script.rulesupport.shared.ScriptedRuleProvider;
import org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleRule;
import org.openhab.core.automation.type.TriggerType;
import org.openhab.core.automation.util.ActionBuilder;
import org.openhab.core.automation.util.ConditionBuilder;
import org.openhab.core.automation.util.ModuleBuilder;
import org.openhab.core.automation.util.TriggerBuilder;
 * This Script-Addon provides types and presets to support writing Rules using a ScriptEngine.
 * One can write and register Rules and Modules by adding them through the HandlerRegistry and/or RuleRegistry
public class RuleSupportScriptExtension implements ScriptExtensionProvider {
    private static final String RULE_SUPPORT = "RuleSupport";
    private static final String RULE_REGISTRY = "ruleRegistry";
    private static final String AUTOMATION_MANAGER = "automationManager";
    private static final Map<String, Collection<String>> PRESETS = new HashMap<>();
    private static final Map<String, Object> STATIC_TYPES = new HashMap<>();
    private static final Set<String> TYPES = new HashSet<>();
    private final Map<String, Map<String, Object>> objectCache = new ConcurrentHashMap<>();
    private final RuleRegistry ruleRegistry;
    private final ScriptedRuleProvider ruleProvider;
    private final ScriptedCustomModuleHandlerFactory scriptedCustomModuleHandlerFactory;
    private final ScriptedCustomModuleTypeProvider scriptedCustomModuleTypeProvider;
    private final ScriptedPrivateModuleHandlerFactory scriptedPrivateModuleHandlerFactory;
    static {
        STATIC_TYPES.put("SimpleActionHandler", SimpleActionHandler.class);
        STATIC_TYPES.put("SimpleConditionHandler", SimpleConditionHandler.class);
        STATIC_TYPES.put("SimpleTriggerHandler", SimpleTriggerHandler.class);
        STATIC_TYPES.put("SimpleRule", SimpleRule.class);
        STATIC_TYPES.put("ActionHandlerFactory", ScriptedActionHandlerFactory.class);
        STATIC_TYPES.put("ConditionHandlerFactory", ScriptedConditionHandlerFactory.class);
        STATIC_TYPES.put("TriggerHandlerFactory", ScriptedTriggerHandlerFactory.class);
        STATIC_TYPES.put("ModuleBuilder", ModuleBuilder.class);
        STATIC_TYPES.put("ActionBuilder", ActionBuilder.class);
        STATIC_TYPES.put("ConditionBuilder", ConditionBuilder.class);
        STATIC_TYPES.put("TriggerBuilder", TriggerBuilder.class);
        STATIC_TYPES.put("Configuration", Configuration.class);
        STATIC_TYPES.put("Action", Action.class);
        STATIC_TYPES.put("Condition", Condition.class);
        STATIC_TYPES.put("Trigger", Trigger.class);
        STATIC_TYPES.put("Rule", Rule.class);
        STATIC_TYPES.put("ModuleType", ModuleType.class);
        STATIC_TYPES.put("ActionType", ActionType.class);
        STATIC_TYPES.put("TriggerType", TriggerType.class);
        STATIC_TYPES.put("Visibility", Visibility.class);
        STATIC_TYPES.put("ConfigDescriptionParameter", ConfigDescriptionParameter.class);
        TYPES.addAll(STATIC_TYPES.keySet());
        TYPES.add(AUTOMATION_MANAGER);
        TYPES.add(RULE_REGISTRY);
        PRESETS.put(RULE_SUPPORT, Arrays.asList("Configuration", "Action", "Condition", "Trigger", "Rule",
                "ModuleBuilder", "ActionBuilder", "ConditionBuilder", "TriggerBuilder"));
        PRESETS.put("RuleSimple", Arrays.asList("SimpleActionHandler", "SimpleConditionHandler", "SimpleTriggerHandler",
                "SimpleRule", "TriggerType", "ConfigDescriptionParameter", "ModuleType", "ActionType", "Visibility"));
        PRESETS.put("RuleFactories",
                Arrays.asList("ActionHandlerFactory", "ConditionHandlerFactory", "TriggerHandlerFactory", "TriggerType",
                        "ConfigDescriptionParameter", "ModuleType", "ActionType", "Visibility"));
    public RuleSupportScriptExtension(final @Reference RuleRegistry ruleRegistry,
            final @Reference ScriptedRuleProvider ruleProvider,
            final @Reference ScriptedCustomModuleHandlerFactory scriptedCustomModuleHandlerFactory,
            final @Reference ScriptedCustomModuleTypeProvider scriptedCustomModuleTypeProvider,
            final @Reference ScriptedPrivateModuleHandlerFactory scriptedPrivateModuleHandlerFactory) {
        this.ruleRegistry = ruleRegistry;
        this.ruleProvider = ruleProvider;
        this.scriptedCustomModuleHandlerFactory = scriptedCustomModuleHandlerFactory;
        this.scriptedCustomModuleTypeProvider = scriptedCustomModuleTypeProvider;
        this.scriptedPrivateModuleHandlerFactory = scriptedPrivateModuleHandlerFactory;
        return PRESETS.keySet();
        Object obj = STATIC_TYPES.get(type);
        if (obj != null) {
        Map<String, Object> objects = Objects
                .requireNonNull(objectCache.computeIfAbsent(scriptIdentifier, k -> new HashMap<>()));
        obj = objects.get(type);
        if (AUTOMATION_MANAGER.equals(type) || RULE_REGISTRY.equals(type)) {
            RuleSupportRuleRegistryDelegate ruleRegistryDelegate = new RuleSupportRuleRegistryDelegate(ruleRegistry,
                    ruleProvider);
            ScriptedAutomationManager automationManager = new ScriptedAutomationManager(ruleRegistryDelegate,
                    scriptedCustomModuleHandlerFactory, scriptedCustomModuleTypeProvider,
                    scriptedPrivateModuleHandlerFactory);
            objects.put(AUTOMATION_MANAGER, automationManager);
            objects.put(RULE_REGISTRY, ruleRegistryDelegate);
        Map<String, Object> scopeValues = new HashMap<>();
        Collection<String> values = PRESETS.get(preset);
        if (values != null) {
            for (String value : values) {
                Object staticType = STATIC_TYPES.get(value);
                if (staticType != null) {
                    scopeValues.put(value, staticType);
        if (RULE_SUPPORT.equals(preset)) {
            Object automationManager = get(scriptIdentifier, AUTOMATION_MANAGER);
            if (automationManager != null) {
                scopeValues.put(AUTOMATION_MANAGER, automationManager);
            Object ruleRegistry = get(scriptIdentifier, RULE_REGISTRY);
            if (ruleRegistry != null) {
                scopeValues.put(RULE_REGISTRY, ruleRegistry);
        return scopeValues;
        Map<String, Object> objects = objectCache.remove(scriptIdentifier);
        if (objects != null) {
            Object automationManager = objects.get(AUTOMATION_MANAGER);
                ScriptedAutomationManager scriptedAutomationManager = (ScriptedAutomationManager) automationManager;
                scriptedAutomationManager.removeAll();
