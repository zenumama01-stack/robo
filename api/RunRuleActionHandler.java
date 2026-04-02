 * This class is a handler for RunRuleAction module type. It runs the rules
 * which's UIDs are passed by the 'ruleUIDs' property. If a rule's status is not
 * IDLE that rule can not run!
 *"type": "core.RunRuleAction",
public class RunRuleActionHandler extends BaseActionModuleHandler {
     * The UID for this handler for identification in the factory.
    public static final String UID = "core.RunRuleAction";
     * the key for the 'rulesUIDs' property of the {@link Action}.
    private static final String CONSIDER_CONDITIONS_KEY = "considerConditions";
     * The logger
    private final Logger logger = LoggerFactory.getLogger(RunRuleActionHandler.class);
     * the UIDs of the rules to be executed.
    private final List<String> ruleUIDs;
     * boolean to express if the conditions should be considered, defaults to
     * true;
    private boolean considerConditions = true;
    public RunRuleActionHandler(final Action module) {
        if (config.getProperties().isEmpty()) {
            throw new IllegalArgumentException("'Configuration' can not be empty.");
        ruleUIDs = (List<String>) config.get(RULE_UIDS_KEY);
        if (ruleUIDs == null) {
            throw new IllegalArgumentException("'ruleUIDs' property must not be null.");
        if (config.get(CONSIDER_CONDITIONS_KEY) != null && config.get(CONSIDER_CONDITIONS_KEY) instanceof Boolean) {
            this.considerConditions = (Boolean) config.get(CONSIDER_CONDITIONS_KEY);
        this.moduleId = module.getId();
        // execute each rule after the other; at the moment synchronously
        Object previousEvent = context.get("event");
        Event event = AutomationEventFactory.createExecutionEvent(moduleId,
                previousEvent instanceof Event ? Map.of("previous", previousEvent) : null, "runRuleAction");
        Map<String, Object> newContext = new HashMap<>(context);
        newContext.put("event", event);
        for (String uid : ruleUIDs) {
                callback.runNow(uid, considerConditions, newContext);
        // no outputs from this module
