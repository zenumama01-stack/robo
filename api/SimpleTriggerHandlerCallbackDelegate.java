import org.openhab.core.automation.RuleStatus;
import org.openhab.core.automation.RuleStatusInfo;
import org.openhab.core.automation.handler.TriggerHandlerCallback;
import org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleTriggerHandlerCallback;
 * The {@link SimpleTriggerHandlerCallbackDelegate} allows a script to define callbacks for triggers in different ways.
public class SimpleTriggerHandlerCallbackDelegate implements SimpleTriggerHandlerCallback {
    private final Trigger trigger;
    private final TriggerHandlerCallback callback;
    public SimpleTriggerHandlerCallbackDelegate(Trigger trigger, TriggerHandlerCallback callback) {
        this.trigger = trigger;
        this.callback = callback;
    public void triggered(Trigger trigger, Map<String, ?> context) {
        callback.triggered(trigger, context);
    public void triggered(Map<String, ?> context) {
        callback.triggered(this.trigger, context);
    public ScheduledExecutorService getScheduler() {
        return callback.getScheduler();
    public @Nullable Boolean isEnabled(String ruleUID) {
        return callback.isEnabled(ruleUID);
    public void setEnabled(String uid, boolean isEnabled) {
        callback.setEnabled(uid, isEnabled);
    public @Nullable RuleStatusInfo getStatusInfo(String ruleUID) {
        return callback.getStatusInfo(ruleUID);
    public @Nullable RuleStatus getStatus(String ruleUID) {
        return callback.getStatus(ruleUID);
    public void runNow(String uid) {
        callback.runNow(uid);
    public void runNow(String uid, boolean considerConditions, @Nullable Map<String, Object> context) {
        callback.runNow(uid, considerConditions, context);
