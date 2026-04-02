import org.openhab.core.automation.RulePredicates;
import org.openhab.core.automation.handler.TimeBasedConditionHandler;
import org.openhab.core.automation.handler.TimeBasedTriggerHandler;
 * Simulates {@link Rule rules} to show up their next execution times within the schedule.
final class RuleExecutionSimulator {
     * Tag that all rules must have, do be included within the simulation.
    private static final String TAG_SCHEDULE = "Schedule";
    private final Logger logger = LoggerFactory.getLogger(RuleExecutionSimulator.class);
    private final RuleEngineImpl ruleEngine;
     * Constructs a new {@link RuleExecutionSimulator}.
    public RuleExecutionSimulator(RuleRegistry ruleRegistry, RuleEngineImpl ruleEngine) {
        this.ruleEngine = ruleEngine;
        logger.debug("Simulating rules from {} until {}.", from, until);
        return ruleRegistry.stream() //
                .filter(RulePredicates.hasAllTags(TAG_SCHEDULE)) //
                .filter((Rule r) -> ruleEngine.isEnabled(r.getUID())) //
                .map((Rule r) -> simulateExecutionsForRule(r, from, until)) //
                .flatMap(List::stream).sorted();
     * Simulates the next executions for the given {@link Rule} until the given {@link Date}.
     * @param rule {@link Rule} to be simulated.
     * @return List of expected {@link RuleExecution}.
    private List<RuleExecution> simulateExecutionsForRule(Rule rule, ZonedDateTime from, ZonedDateTime until) {
        final List<RuleExecution> executions = new ArrayList<>();
        for (Trigger trigger : rule.getTriggers()) {
            TriggerHandler triggerHandler = (TriggerHandler) this.ruleEngine.getModuleHandler(trigger, rule.getUID());
            // Only triggers that are time-based will be considered within the simulation
            if (triggerHandler instanceof TimeBasedTriggerHandler handler) {
                SchedulerTemporalAdjuster temporalAdjuster = handler.getTemporalAdjuster();
                executions.addAll(simulateExecutionsForCronBasedRule(rule, from, until, temporalAdjuster));
        logger.debug("Created {} rule simulations for rule {}.", executions.size(), rule.getName());
        return executions;
     * Simulates all {@link RuleExecution} for the given cron expression of the given rule.
     * @param temporalAdjuster {@link SchedulerTemporalAdjuster} to be evaluated for determining the execution times.
     * @return a list of expected executions.
    private List<RuleExecution> simulateExecutionsForCronBasedRule(Rule rule, ZonedDateTime from, ZonedDateTime until,
            SchedulerTemporalAdjuster temporalAdjuster) {
        final List<RuleExecution> result = new ArrayList<>();
        ZonedDateTime currentTime = ZonedDateTime.from(temporalAdjuster.adjustInto(from));
        while (!temporalAdjuster.isDone(currentTime) && currentTime.isBefore(until)) {
            // if the current time satisfies all conditions add the instance to the result
            if (checkConditions(rule, currentTime)) {
                result.add(new RuleExecution(Date.from(currentTime.toInstant()), rule));
            currentTime = ZonedDateTime.from(temporalAdjuster.adjustInto(currentTime));
     * Checks if all defined conditions for the given rule are satisfied.
     * @param rule The rule to check
     * @param current the time to check
     * @return <code>true</code> if and only if all conditions are satisfied for the current time.
    private boolean checkConditions(Rule rule, ZonedDateTime current) {
        for (Condition condition : rule.getConditions()) {
            ConditionHandler conditionHandler = (ConditionHandler) this.ruleEngine.getModuleHandler(condition,
                    rule.getUID());
            // Only conditions that are time based are checked
            if (conditionHandler instanceof TimeBasedConditionHandler handler && !handler.isSatisfiedAt(current)) {
