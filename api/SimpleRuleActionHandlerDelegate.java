public class SimpleRuleActionHandlerDelegate extends SimpleActionHandler {
    private SimpleRuleActionHandler handler;
    public SimpleRuleActionHandlerDelegate(SimpleRuleActionHandler handler) {
        this.handler = handler;
    public Object execute(Action module, Map<String, ?> inputs) {
        return handler.execute(module, inputs);
