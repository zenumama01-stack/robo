 * This class is a handler implementation for {@link CompositeTriggerType}. The trigger which has
 * {@link CompositeTriggerType} has to be notified by the handlers of child triggers and it will be triggered when some
 * of them is triggered. The handler has to put outputs of the trigger, base on the outputs of the child triggers, into
 * rule context. The outputs of the child triggers are not visible out of context of the trigger.
public class CompositeTriggerHandler
        extends AbstractCompositeModuleHandler<Trigger, CompositeTriggerType, TriggerHandler>
        implements TriggerHandler, TriggerHandlerCallback {
    private @NonNullByDefault({}) TriggerHandlerCallback callback;
     * Constructor of this system handler.
     * @param trigger trigger of composite type (parent trigger).
     * @param mt module type of parent trigger
     * @param mapModuleToHandler map of pairs child triggers to their handlers
     * @param ruleUID UID of rule where the parent trigger is part of
    public CompositeTriggerHandler(Trigger trigger, CompositeTriggerType mt,
            LinkedHashMap<Trigger, @Nullable TriggerHandler> mapModuleToHandler, String ruleUID) {
        super(trigger, mt, mapModuleToHandler);
     * This method is called by the child triggers defined by the {@link CompositeTriggerType} of parent trigger.
     * The method goes through the outputs of the parent trigger and fill them base on the ouput's reference value.
     * The ouput's reference value can contain more then one references to the child outputs separated by comma. In this
     * case the method will try to fill the output value in sequence defined in the reference value. The letter
     * reference can be overwritten by the previous ones.
     * @see org.openhab.core.automation.handler.TriggerHandlerCallback#triggered(org.openhab.core.automation.Trigger,
     *      java.util.Map)
            List<Output> outputs = moduleType.getOutputs();
            Map<String, Object> result = new HashMap<>(11);
                            String childModuleId = ref.substring(0, i);
                            if (trigger.getId().equals(childModuleId)) {
                                ref = ref.substring(i + 1);
                        Object value;
                        int idx = ReferenceResolver.getNextRefToken(ref, 1);
                        if (idx < ref.length()) {
                            String outputId = ref.substring(0, idx);
                            value = ReferenceResolver.resolveComplexDataReference(context.get(outputId),
                                    ref.substring(idx + 1));
                            value = context.get(ref);
                            result.put(output.getName(), value);
            callback.triggered(module, result);
     * The {@link CompositeTriggerHandler} sets itself as callback to the child triggers and store the callback to the
     * rule engine. In this way the trigger of composite type will always be notified when some of the child triggers
     * are triggered and has an opportunity to set the outputs of parent trigger to the rule context.
     * @see org.openhab.core.automation.handler.TriggerHandler#setCallback(ModuleHandlerCallback)
    public void setCallback(@Nullable ModuleHandlerCallback callback) {
        this.callback = (TriggerHandlerCallback) callback;
        if (callback instanceof TriggerHandlerCallback) {// could be called with 'null' from dispose and might not be a
                                                         // trigger callback
            List<Trigger> children = getChildren();
            for (Trigger child : children) {
                TriggerHandler handler = moduleHandlerMap.get(child);
                    handler.setCallback(this);
        setCallback(null);
    protected List<Trigger> getChildren() {
