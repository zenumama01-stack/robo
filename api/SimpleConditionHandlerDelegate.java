import org.openhab.core.automation.handler.BaseConditionModuleHandler;
 * The SimpleConditionHandlerDelegate allows the registration of {@link SimpleConditionHandler}s to the RuleManager.
public class SimpleConditionHandlerDelegate extends BaseConditionModuleHandler {
    private SimpleConditionHandler conditionHandler;
    public SimpleConditionHandlerDelegate(Condition condition, SimpleConditionHandler scriptedHandler) {
        super(condition);
        this.conditionHandler = scriptedHandler;
        scriptedHandler.init(condition);
    public boolean isSatisfied(Map<String, Object> inputs) {
        return conditionHandler.isSatisfied(module, inputs);
