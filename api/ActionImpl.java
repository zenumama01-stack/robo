package org.openhab.core.automation.internal;
 * This class is implementation of {@link Action} modules used in the {@link RuleEngineImpl}s.
public class ActionImpl extends ModuleImpl implements Action {
    private Map<String, String> inputs = Map.of();
     * Constructor of Action object.
     * @param uid action unique id.
     * @param typeUID module type unique id.
     * @param configuration map of configuration values.
     * @param label the label
     * @param description description
     * @param inputs set of connections to other modules (triggers and other actions).
    public ActionImpl(String uid, String typeUID, @Nullable Configuration configuration, @Nullable String label,
            @Nullable String description, @Nullable Map<String, String> inputs) {
        super(uid, typeUID, configuration, label, description);
        this.inputs = inputs == null ? Map.of() : Map.copyOf(inputs);
     * This method is used to get input connections of the Action. The connections
     * are links between {@link Input}s of the this {@link Module} and {@link Output}s
     * of other {@link Module}s.
     * @return map that contains the inputs of this action.
    public Map<String, String> getInputs() {
        return inputs;
