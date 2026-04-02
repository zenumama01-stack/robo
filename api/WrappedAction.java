package org.openhab.core.automation.internal.ruleengine;
import org.openhab.core.automation.internal.Connection;
 * This class holds the information that is necessary for the rule engine.
public class WrappedAction extends WrappedModule<Action, ActionHandler> {
    private Set<Connection> connections = Set.of();
    public WrappedAction(final Action action) {
        super(action);
     * This method sets the connections for this module.
     * @param connections the set of connections for this action
    public void setConnections(@Nullable Set<Connection> connections) {
        this.connections = connections == null ? Set.of() : connections;
    public Set<Connection> getConnections() {
     * This method is used to connect {@link Input}s of the action to {@link Output}s of other {@link Module}s.
     * @param inputs map that contains the inputs for this action.
    public void setInputs(@Nullable Map<String, String> inputs) {
        this.inputs = inputs == null ? Map.of() : Collections.unmodifiableMap(inputs);
