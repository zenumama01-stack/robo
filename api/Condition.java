 * This interface represents automation {@code Condition} modules which are working as a filter for {@link Rule}'s
 * executions. After being triggered, a Rule's execution will continue only if all its conditions are satisfied.
 * Conditions can be used to check the output from the trigger or other data available in the system. To receive an
 * output data from triggers the Conditions have {@link Input}s.
 * Conditions can be configured.
 * Conditions don't have {@link Output}s 'cause they don't provide information to the other modules of the Rule.
 * Building elements of conditions as {@link ConfigDescriptionParameter}s and {@link Input}s. They are defined by the
 * corresponding {@link ConditionType}.
 * Condition modules are placed in <b>conditions</b> section of the {@link Rule} definition.
 * @see Module
public interface Condition extends Module {
     * Gets the input references of the Condition. The references define how the {@link Input}s of this {@link Module}
     * are connected to {@link Output}s of other {@link Module}s.
     * @return a map that contains the input references of this condition.
 * A representation of a sitemap rule condition.
public interface Condition {
     * Get the item for which the state will be used in the condition evaluation. If no item is set (null returned), the
     * item of
     * the widget will be used.
    String getItem();
     * Set the item for which the state will be used in the condition evaluation.
    void setItem(@Nullable String item);
     * Get the condition comparator. Valid values are: "==", "&gt;", "&lt;", "&gt;=", "&lt;=", "!=". The item in the
     * condition will be compared against the value using this comparator. If no condition comparator is set, "==" is
     * assumed.
     * @return condition comparator
    String getCondition();
     * Set the condition comparator, see {@link #getCondition()}.
     * @param condition
    void setCondition(@Nullable String condition);
     * Get the condition comparison value.
     * @return value
    String getValue();
     * Set the condition comparison value.
    void setValue(String value);
