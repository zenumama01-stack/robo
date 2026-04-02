 * This class is a handler implementation for {@link CompositeConditionType}. The condition which has
 * {@link CompositeConditionType} module type will be satisfied only when all child conditions (defined
 * by its {@link CompositeConditionType}) are satisfied.
public class CompositeConditionHandler
        extends AbstractCompositeModuleHandler<Condition, CompositeConditionType, ConditionHandler>
        implements ConditionHandler {
    public CompositeConditionHandler(Condition condition, CompositeConditionType mt,
            LinkedHashMap<Condition, @Nullable ConditionHandler> mapModuleToHandler, String ruleUID) {
        super(condition, mt, mapModuleToHandler);
     * The method calls handlers of child modules and return true only when they all are satisfied.
     * @see org.openhab.core.automation.handler.ConditionHandler#isSatisfied(java.util.Map)
    public boolean isSatisfied(Map<String, Object> context) {
        List<Condition> children = getChildren();
        Map<String, Object> compositeContext = getCompositeContext(context);
        for (Condition child : children) {
            ConditionHandler childHandler = moduleHandlerMap.get(child);
            boolean isSatisfied = childHandler.isSatisfied(childContext);
            if (!isSatisfied) {
    protected List<Condition> getChildren() {
