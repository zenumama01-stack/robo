package org.openhab.core.automation.util;
import org.openhab.core.automation.internal.ActionImpl;
 * This class allows the easy construction of an {@link Action} instance using the builder pattern.
public class ActionBuilder extends ModuleBuilder<ActionBuilder, Action> {
    private @Nullable Map<String, String> inputs;
    protected ActionBuilder() {
    protected ActionBuilder(final Action action) {
        this.inputs = action.getInputs();
    public static ActionBuilder create() {
        return new ActionBuilder();
    public static ActionBuilder create(final Action action) {
        return new ActionBuilder(action);
    public ActionBuilder withInputs(@Nullable Map<String, String> inputs) {
        this.inputs = inputs != null ? Map.copyOf(inputs) : null;
    public Action build() {
        return new ActionImpl(getId(), getTypeUID(), configuration, label, description, inputs);
