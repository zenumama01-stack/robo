 * This is a callback interface to RuleManager which is used by the {@link TriggerHandler} to notify the RuleManager
 * about firing of the {@link Trigger}. These calls from {@link Trigger}s must be stored in a queue
 * and applied to the RuleAngine in order of their appearance. Each {@link Rule} has to create its own instance of
 * {@link TriggerHandlerCallback}.
 * @author Kai Kreuzer - made it a sub-interface of ModuleHandlerCallback
 * @author Fabian Wolter - Add method for retrieving the handler's scheduler
public interface TriggerHandlerCallback extends ModuleHandlerCallback {
     * This method is used by the {@link TriggerHandler} to notify the RuleManager when
     * the linked {@link Trigger} instance was fired.
     * @param trigger instance of trigger which was fired. When one TriggerHandler
     *            serve more then one {@link Trigger} instances, this parameter
     *            defines which trigger was fired.
    default void triggered(Trigger trigger) {
        triggered(trigger, Map.of());
     * @param context is a {@link Map} of output values of the triggered {@link Trigger}. Each entry of the map
     *            contains:
     *            <ul>
     *            <li><code>key</code> - the id of the {@link Output} ,
     *            <li><code>value</code> - represents output value of the {@link Trigger}'s {@link Output}
     *            </ul>
    void triggered(Trigger trigger, Map<String, ?> context);
     * @return the scheduler of this rule
    ScheduledExecutorService getScheduler();
