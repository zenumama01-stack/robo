package org.openhab.core.model.rule.runtime.internal;
import org.eclipse.xtext.nodemodel.util.NodeModelUtils;
import org.eclipse.xtext.xbase.XExpression;
import org.eclipse.xtext.xbase.interpreter.IEvaluationContext;
import org.openhab.core.automation.internal.module.handler.DateTimeTriggerHandler;
import org.openhab.core.automation.internal.module.handler.GenericCronTriggerHandler;
import org.openhab.core.automation.internal.module.handler.TimeOfDayTriggerHandler;
import org.openhab.core.model.rule.jvmmodel.RulesRefresher;
import org.openhab.core.model.rule.rules.ChangedEventTrigger;
import org.openhab.core.model.rule.rules.CommandEventTrigger;
import org.openhab.core.model.rule.rules.DateTimeTrigger;
import org.openhab.core.model.rule.rules.EventEmittedTrigger;
import org.openhab.core.model.rule.rules.EventTrigger;
import org.openhab.core.model.rule.rules.GroupMemberChangedEventTrigger;
import org.openhab.core.model.rule.rules.GroupMemberCommandEventTrigger;
import org.openhab.core.model.rule.rules.GroupMemberUpdateEventTrigger;
import org.openhab.core.model.rule.rules.RuleModel;
import org.openhab.core.model.rule.rules.SystemOnShutdownTrigger;
import org.openhab.core.model.rule.rules.SystemOnStartupTrigger;
import org.openhab.core.model.rule.rules.SystemStartlevelTrigger;
import org.openhab.core.model.rule.rules.ThingStateChangedEventTrigger;
import org.openhab.core.model.rule.rules.ThingStateUpdateEventTrigger;
import org.openhab.core.model.rule.rules.TimerTrigger;
import org.openhab.core.model.rule.rules.UpdateEventTrigger;
import org.openhab.core.model.script.runtime.DSLScriptContextProvider;
import org.openhab.core.model.script.script.Script;
 * This RuleProvider provides rules that are defined in DSL rule files.
 * All rules consist out of a list of triggers and a single script action.
 * No rule conditions are used as this concept does not exist for DSL rules.
@Component(immediate = true, service = { DSLRuleProvider.class, RuleProvider.class, DSLScriptContextProvider.class })
public class DSLRuleProvider
        implements RuleProvider, ModelRepositoryChangeListener, DSLScriptContextProvider, ReadyTracker {
    static final String MIMETYPE_OPENHAB_DSL_RULE = "application/vnd.openhab.dsl.rule";
    private final Logger logger = LoggerFactory.getLogger(DSLRuleProvider.class);
    private final Collection<ProviderChangeListener<Rule>> listeners = new ArrayList<>();
    private final Map<String, Rule> rules = new ConcurrentHashMap<>();
    private final Map<String, IEvaluationContext> contexts = new ConcurrentHashMap<>();
    private final Map<String, XExpression> xExpressions = new ConcurrentHashMap<>();
    private final ReadyMarker marker = new ReadyMarker("rules", "dslprovider");
    private int triggerId = 0;
    public DSLRuleProvider(@Reference ModelRepository modelRepository, @Reference ReadyService readyService) {
        readyService.registerTracker(this, new ReadyMarkerFilter().withType(RulesRefresher.RULES_REFRESH_MARKER_TYPE)
                .withIdentifier(RulesRefresher.RULES_REFRESH));
        contexts.clear();
        xExpressions.clear();
    public void addProviderChangeListener(ProviderChangeListener<Rule> listener) {
    public void removeProviderChangeListener(ProviderChangeListener<Rule> listener) {
    public void modelChanged(String modelFileName, EventType type) {
        String ruleModelType = modelFileName.substring(modelFileName.lastIndexOf(".") + 1);
        if ("rules".equalsIgnoreCase(ruleModelType)) {
            String ruleModelName = modelFileName.substring(0, modelFileName.lastIndexOf("."));
            List<ModelRulePair> modelRules = new ArrayList<>();
                    EObject model = modelRepository.getModel(modelFileName);
                    if (model instanceof RuleModel ruleModel) {
                        for (org.openhab.core.model.rule.rules.Rule rule : ruleModel.getRules()) {
                            Rule newRule = toRule(ruleModelName, rule, index);
                            rules.put(newRule.getUID(), newRule);
                            xExpressions.put(ruleModelName + "-" + index, rule.getScript());
                            modelRules.add(new ModelRulePair(newRule, null));
                        handleVarDeclarations(ruleModelName, ruleModel);
                    removeRuleModel(ruleModelName);
                    EObject modifiedModel = modelRepository.getModel(modelFileName);
                    if (modifiedModel instanceof RuleModel ruleModel) {
                            Rule oldRule = rules.remove(ruleModelName);
                            modelRules.add(new ModelRulePair(newRule, oldRule));
                    logger.debug("Unknown event type.");
            notifyProviderChangeListeners(modelRules);
        } else if ("script".equals(ruleModelType)) {
                    if (model instanceof Script script) {
                        Rule oldRule = rules.remove(modelFileName);
                        Rule newRule = toRule(modelFileName, script);
                        listeners.forEach(listener -> listener.removed(this, oldRule));
    public @Nullable IEvaluationContext getContext(String contextName) {
        return contexts.get(contextName);
    public @Nullable XExpression getParsedScript(String modelName, String index) {
        return xExpressions.get(modelName + "-" + index);
    private void handleVarDeclarations(String modelName, RuleModel ruleModel) {
        IEvaluationContext context = RuleContextHelper.getContext(ruleModel);
        contexts.put(modelName, context);
    private void removeRuleModel(String modelName) {
        Iterator<Entry<String, Rule>> it = rules.entrySet().iterator();
            Entry<String, Rule> entry = it.next();
            if (belongsToModel(entry.getKey(), modelName)) {
                listeners.forEach(listener -> listener.removed(this, entry.getValue()));
        Iterator<Entry<String, XExpression>> it2 = xExpressions.entrySet().iterator();
        while (it2.hasNext()) {
            Entry<String, XExpression> entry = it2.next();
                it2.remove();
        contexts.remove(modelName);
    private boolean belongsToModel(String id, String modelName) {
        int idx = id.lastIndexOf("-");
            String prefix = id.substring(0, idx);
            return prefix.equals(modelName);
    private Rule toRule(String modelName, Script script) {
        String scriptText = NodeModelUtils.findActualNodeFor(script).getText();
        Configuration cfg = new Configuration();
        cfg.put(AbstractScriptModuleHandler.CONFIG_SCRIPT, removeIndentation(scriptText));
        cfg.put(AbstractScriptModuleHandler.CONFIG_SCRIPT_TYPE, MIMETYPE_OPENHAB_DSL_RULE);
        List<Action> actions = List.of(ActionBuilder.create().withId("script").withTypeUID(ScriptActionHandler.TYPE_ID)
                .withConfiguration(cfg).build());
        return RuleBuilder.create(modelName).withTags("Script").withName(modelName).withActions(actions).build();
    private Rule toRule(String modelName, org.openhab.core.model.rule.rules.Rule rule, int index) {
        String name = rule.getName();
        String uid = modelName + "-" + index;
        // Create Triggers
        triggerId = 0;
        for (EventTrigger t : rule.getEventtrigger()) {
            Trigger trigger = mapTrigger(t);
            if (trigger != null) {
                triggers.add(trigger);
        // Create Action
        String context = DSLScriptContextProvider.CONTEXT_IDENTIFIER + modelName + "-" + index + "\n";
        XExpression expression = rule.getScript();
        String script = NodeModelUtils.findActualNodeFor(expression).getText();
        cfg.put(AbstractScriptModuleHandler.CONFIG_SCRIPT, context + removeIndentation(script));
        List<String> ruleTags = rule.getTags();
        Set<String> tags = ruleTags == null ? Set.of() : Set.copyOf(ruleTags);
        return RuleBuilder.create(uid).withTags(tags).withName(name).withTriggers(triggers).withActions(actions)
    private String removeIndentation(String script) {
        String s = script;
        // first let's remove empty lines at the beginning and add an empty line at the end to beautify the yaml style.
        if (s.startsWith("\n")) {
            s = s.substring(1);
        if (s.startsWith("\r\n")) {
            s = s.substring(2);
        if (!(s.endsWith("\n\n") || s.endsWith("\r\n\r\n"))) {
            s += "\n\n";
        String firstLine = s.lines().findFirst().orElse("");
        String indentation = firstLine == null ? ""
                : firstLine.substring(0, firstLine.length() - firstLine.stripLeading().length());
        return s.lines().map(line -> (line.startsWith(indentation) ? line.substring(indentation.length()) : line))
                .collect(Collectors.joining("\n"));
    private @Nullable Trigger mapTrigger(EventTrigger t) {
        if (t instanceof SystemOnStartupTrigger) {
            cfg.put(SystemTriggerHandler.CFG_STARTLEVEL, 40);
            return TriggerBuilder.create().withId(Integer.toString(triggerId++))
                    .withTypeUID(SystemTriggerHandler.STARTLEVEL_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof SystemStartlevelTrigger slTrigger) {
            cfg.put(SystemTriggerHandler.CFG_STARTLEVEL, slTrigger.getLevel());
        } else if (t instanceof SystemOnShutdownTrigger) {
            logger.warn("System shutdown rule triggers are no longer supported!");
        } else if (t instanceof CommandEventTrigger ceTrigger) {
            cfg.put(ItemCommandTriggerHandler.CFG_ITEMNAME, ceTrigger.getItem());
            if (ceTrigger.getCommand() != null) {
                cfg.put(ItemCommandTriggerHandler.CFG_COMMAND, ceTrigger.getCommand().getValue());
                    .withTypeUID(ItemCommandTriggerHandler.MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof GroupMemberCommandEventTrigger ceTrigger) {
            cfg.put(GroupCommandTriggerHandler.CFG_GROUPNAME, ceTrigger.getGroup());
                cfg.put(GroupCommandTriggerHandler.CFG_COMMAND, ceTrigger.getCommand().getValue());
                    .withTypeUID(GroupCommandTriggerHandler.MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof UpdateEventTrigger ueTrigger) {
            cfg.put(ItemStateTriggerHandler.CFG_ITEMNAME, ueTrigger.getItem());
            if (ueTrigger.getState() != null) {
                cfg.put(ItemStateTriggerHandler.CFG_STATE, ueTrigger.getState().getValue());
                    .withTypeUID(ItemStateTriggerHandler.UPDATE_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof GroupMemberUpdateEventTrigger ueTrigger) {
            cfg.put(GroupStateTriggerHandler.CFG_GROUPNAME, ueTrigger.getGroup());
                cfg.put(GroupStateTriggerHandler.CFG_STATE, ueTrigger.getState().getValue());
                    .withTypeUID(GroupStateTriggerHandler.UPDATE_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof ChangedEventTrigger ceTrigger) {
            cfg.put(ItemStateTriggerHandler.CFG_ITEMNAME, ceTrigger.getItem());
            if (ceTrigger.getNewState() != null) {
                cfg.put(ItemStateTriggerHandler.CFG_STATE, ceTrigger.getNewState().getValue());
            if (ceTrigger.getOldState() != null) {
                cfg.put(ItemStateTriggerHandler.CFG_PREVIOUS_STATE, ceTrigger.getOldState().getValue());
                    .withTypeUID(ItemStateTriggerHandler.CHANGE_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof GroupMemberChangedEventTrigger ceTrigger) {
            cfg.put(GroupStateTriggerHandler.CFG_GROUPNAME, ceTrigger.getGroup());
                cfg.put(GroupStateTriggerHandler.CFG_STATE, ceTrigger.getNewState().getValue());
                cfg.put(GroupStateTriggerHandler.CFG_PREVIOUS_STATE, ceTrigger.getOldState().getValue());
                    .withTypeUID(GroupStateTriggerHandler.CHANGE_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof TimerTrigger tt) {
            String triggerType;
            if (tt.getCron() != null) {
                triggerType = GenericCronTriggerHandler.MODULE_TYPE_ID;
                cfg.put(GenericCronTriggerHandler.CFG_CRON_EXPRESSION, tt.getCron());
                triggerType = TimeOfDayTriggerHandler.MODULE_TYPE_ID;
                String id = tt.getTime();
                if ("noon".equals(id)) {
                    cfg.put(TimeOfDayTriggerHandler.CFG_TIME, "12:00");
                } else if ("midnight".equals(id)) {
                    cfg.put(TimeOfDayTriggerHandler.CFG_TIME, "00:00");
                    cfg.put(TimeOfDayTriggerHandler.CFG_TIME, id);
            return TriggerBuilder.create().withId(Integer.toString(triggerId++)).withTypeUID(triggerType)
                    .withConfiguration(cfg).build();
        } else if (t instanceof DateTimeTrigger tt) {
            cfg.put(DateTimeTriggerHandler.CONFIG_ITEM_NAME, tt.getItem());
            cfg.put(DateTimeTriggerHandler.CONFIG_TIME_ONLY, tt.isTimeOnly());
            cfg.put(DateTimeTriggerHandler.CONFIG_OFFSET, tt.getOffset());
            return TriggerBuilder.create().withId(Integer.toString((triggerId++)))
                    .withTypeUID(DateTimeTriggerHandler.MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof EventEmittedTrigger eeTrigger) {
            cfg.put(ChannelEventTriggerHandler.CFG_CHANNEL, eeTrigger.getChannel());
            if (eeTrigger.getTrigger() != null) {
                cfg.put(ChannelEventTriggerHandler.CFG_CHANNEL_EVENT, eeTrigger.getTrigger().getValue());
                    .withTypeUID(ChannelEventTriggerHandler.MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof ThingStateUpdateEventTrigger tsuTrigger) {
            cfg.put(ThingStatusTriggerHandler.CFG_THING_UID, tsuTrigger.getThing());
            cfg.put(ThingStatusTriggerHandler.CFG_STATUS, tsuTrigger.getState());
                    .withTypeUID(ThingStatusTriggerHandler.UPDATE_MODULE_TYPE_ID).withConfiguration(cfg).build();
        } else if (t instanceof ThingStateChangedEventTrigger tscTrigger) {
            cfg.put(ThingStatusTriggerHandler.CFG_THING_UID, tscTrigger.getThing());
            cfg.put(ThingStatusTriggerHandler.CFG_STATUS, tscTrigger.getNewState());
            cfg.put(ThingStatusTriggerHandler.CFG_PREVIOUS_STATUS, tscTrigger.getOldState());
                    .withTypeUID(ThingStatusTriggerHandler.CHANGE_MODULE_TYPE_ID).withConfiguration(cfg).build();
            logger.warn("Unknown trigger type '{}' - ignoring it.", t.getClass().getSimpleName());
        for (String ruleFileName : modelRepository.getAllModelNamesOfType("rules")) {
            EObject model = modelRepository.getModel(ruleFileName);
            String ruleModelName = ruleFileName.substring(0, ruleFileName.indexOf("."));
        readyService.markReady(marker);
    private void notifyProviderChangeListeners(List<ModelRulePair> modelRules) {
        modelRules.forEach(rulePair -> {
            Rule oldRule = rulePair.oldRule();
                rules.remove(oldRule.getUID());
                rules.put(rulePair.newRule().getUID(), rulePair.newRule());
                listeners.forEach(listener -> listener.updated(this, oldRule, rulePair.newRule()));
                listeners.forEach(listener -> listener.added(this, rulePair.newRule()));
        readyService.unmarkReady(marker);
    private record ModelRulePair(Rule newRule, @Nullable Rule oldRule) {
