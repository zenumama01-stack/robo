public class WrappedCondition extends WrappedModule<Condition, ConditionHandler> {
    public WrappedCondition(final Condition condition) {
     * @param connections the set of connections for this condition
     * This method is used to connect {@link Input}s of the Condition to {@link Output}s of other {@link Module}s.
     * @param inputs map that contains the inputs for this condition.
