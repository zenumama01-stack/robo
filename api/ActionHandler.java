package org.openhab.core.automation.handler;
 * This interface should be implemented by external modules which provide functionality for processing {@link Action}
 * modules. This functionality is called to execute the {@link Action}s of the {@link Rule} when it is needed.
 * @see ModuleHandler
public interface ActionHandler extends ModuleHandler {
     * Called to compile an {@link Action} of the {@link Rule} when the rule is initialized.
     * @throws Exception if the compilation fails
    default void compile() throws Exception {
        // Do nothing by default
     * Called to execute an {@link Action} of the {@link Rule} when it is needed.
     * @param context an unmodifiable map containing the outputs of the {@link Trigger} that triggered the {@link Rule},
     *            the outputs of all preceding {@link Action}s, and the inputs for this {@link Action}.
     * @return a map with the {@code outputs} which are the result of the {@link Action}'s execution (may be null).
    Map<String, @Nullable Object> execute(Map<String, Object> context);
