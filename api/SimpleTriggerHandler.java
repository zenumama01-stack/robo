public abstract class SimpleTriggerHandler implements ScriptedHandler {
    private @Nullable SimpleTriggerHandlerCallback ruleCallback;
    public void init(Trigger module) {
    public void setRuleEngineCallback(Trigger module, SimpleTriggerHandlerCallback ruleCallback) {
        this.ruleCallback = ruleCallback;
    protected void trigger(Map<String, ?> context) {
        SimpleTriggerHandlerCallback callback = this.ruleCallback;
        if (callback != null) {
            callback.triggered(context);
