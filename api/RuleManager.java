public interface RuleManager {
     * The <b>disabled</b> rule status is {@link RuleStatus#UNINITIALIZED} with {@link RuleStatusDetail#DISABLED}.
     * @return {@code true} when the {@link RuleStatus} is one of the {@link RuleStatus#UNINITIALIZED} with any other
     *         {@link RuleStatusDetail} than {@link RuleStatusDetail#DISABLED},
     *         {@link RuleStatus#UNINITIALIZED} with {@link RuleStatusDetail#DISABLED} and {@code null} when it is not
     *         available.
     * @return a copy of the rule context, including possible return values
    Map<String, Object> runNow(String uid);
    Map<String, Object> runNow(String uid, boolean considerConditions, @Nullable Map<String, Object> context);
     * Simulates the execution of all rules with tag 'Schedule' for the given time interval.
     * The result is sorted ascending by execution time.
     * @param from {@link ZonedDateTime} earliest time to be contained in the rule simulation.
     * @param until {@link ZonedDateTime} latest time to be contained in the rule simulation.
     * @return A {@link Stream} with all expected {@link RuleExecution}.
    Stream<RuleExecution> simulateRuleExecutions(ZonedDateTime from, ZonedDateTime until);
