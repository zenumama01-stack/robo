 * This interface provides common functionality for processing {@link Condition} modules.
public interface ConditionHandler extends ModuleHandler {
     * Called to compile the {@link Condition} when the {@link Rule} is initialized.
     * Checks if the Condition is satisfied in the given {@code context}.
     * @param context an unmodifiable map containing the outputs of the {@link Trigger} that triggered the {@link Rule}
     *            and the inputs of the {@link Condition}.
     * @return {@code true} if {@link Condition} is satisfied, {@code false} otherwise.
    boolean isSatisfied(Map<String, Object> context);
