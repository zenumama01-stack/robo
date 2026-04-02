import org.openhab.core.automation.internal.TriggerImpl;
 * This class allows the easy construction of a {@link Trigger} instance using the builder pattern.
public class TriggerBuilder extends ModuleBuilder<TriggerBuilder, Trigger> {
    protected TriggerBuilder() {
    protected TriggerBuilder(final Trigger condition) {
    public static TriggerBuilder create() {
        return new TriggerBuilder();
    public static TriggerBuilder create(final Trigger trigger) {
        return new TriggerBuilder(trigger);
    public Trigger build() {
        return new TriggerImpl(getId(), getTypeUID(), configuration, label, description);
