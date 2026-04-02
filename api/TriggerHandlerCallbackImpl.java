 * This class is implementation of {@link TriggerHandlerCallback} used by the {@link Trigger}s to notify rule engine
 * about appearing of new triggered data. There is one and only one {@link TriggerHandlerCallback} per Rule and
 * it is used by all rule's {@link Trigger}s.
 * @author Kai Kreuzer - improved stability
 * @author Fabian Wolter - Change executor to ScheduledExecutorService and expose it
public class TriggerHandlerCallbackImpl implements TriggerHandlerCallback {
    private final RuleEngineImpl re;
    private final String ruleUID;
    private ScheduledExecutorService executor;
    private @Nullable Future<?> future;
    protected TriggerHandlerCallbackImpl(RuleEngineImpl re, String ruleUID) {
        this.re = re;
        this.executor = ThreadPoolManager.getPoolBasedSequentialScheduledExecutorService("rules", "rule-" + ruleUID);
            future = executor.submit(new TriggerData(trigger, context));
        re.logger.debug("The trigger '{}' of rule '{}' is triggered.", trigger.getId(), ruleUID);
    public boolean isRunning() {
        Future<?> future = this.future;
        return future == null || !future.isDone();
    class TriggerData implements Runnable {
        private @Nullable final Map<String, ?> outputs;
        public Trigger getTrigger() {
            return trigger;
        public @Nullable Map<String, ?> getOutputs() {
        public TriggerData(Trigger t, @Nullable Map<String, ?> outputs) {
            this.trigger = t;
            this.outputs = outputs;
            re.runRule(ruleUID, this);
            executor.shutdownNow();
        return re.isEnabled(ruleUID);
        re.setEnabled(uid, isEnabled);
        return re.getStatusInfo(ruleUID);
        return re.getStatus(ruleUID);
        re.runNow(uid);
        re.runNow(uid, considerConditions, context);
        return executor;
