public abstract class SimpleConditionHandler implements ScriptedHandler {
    public void init(Condition condition) {
    public abstract boolean isSatisfied(Condition condition, Map<String, ?> inputs);
