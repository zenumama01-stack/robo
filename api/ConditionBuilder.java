import org.openhab.core.automation.internal.ConditionImpl;
 * This class allows the easy construction of a {@link Condition} instance using the builder pattern.
public class ConditionBuilder extends ModuleBuilder<ConditionBuilder, Condition> {
    protected ConditionBuilder() {
    protected ConditionBuilder(final Condition condition) {
        this.inputs = condition.getInputs();
    public static ConditionBuilder create() {
        return new ConditionBuilder();
    public static ConditionBuilder create(final Condition condition) {
        return new ConditionBuilder(condition);
    public ConditionBuilder withInputs(@Nullable Map<String, String> inputs) {
    public Condition build() {
        return new ConditionImpl(getId(), getTypeUID(), configuration, label, description, inputs);
