 * This class allows the easy construction of a {@link Module} instance using the builder pattern.
 * @author Markus Rathgeb - Split implementation for different module types in sub classes
public abstract class ModuleBuilder<B extends ModuleBuilder<B, T>, T extends Module> {
    public static ActionBuilder createAction() {
        return ActionBuilder.create();
    public static ActionBuilder createAction(final Action action) {
        return ActionBuilder.create(action);
    public static ConditionBuilder createCondition() {
        return ConditionBuilder.create();
    public static ConditionBuilder createCondition(final Condition condition) {
        return ConditionBuilder.create(condition);
    public static TriggerBuilder createTrigger() {
        return TriggerBuilder.create();
    public static TriggerBuilder createTrigger(final Trigger trigger) {
        return TriggerBuilder.create(trigger);
    public static <B extends ModuleBuilder<B, T>, T extends Module> ModuleBuilder<B, T> create(Module module) {
            return (ModuleBuilder<B, T>) createAction(action);
            return (ModuleBuilder<B, T>) createCondition(condition);
        } else if (module instanceof Trigger trigger) {
            return (ModuleBuilder<B, T>) createTrigger(trigger);
            throw new IllegalArgumentException("Parameter must be an instance of Action, Condition or Trigger.");
    private @Nullable String typeUID;
    protected @Nullable Configuration configuration;
    protected @Nullable String label;
    protected ModuleBuilder() {
    protected ModuleBuilder(T module) {
        this.id = module.getId();
        this.typeUID = module.getTypeUID();
        this.configuration = new Configuration(module.getConfiguration());
        this.label = module.getLabel();
        this.description = module.getDescription();
    public B withId(String id) {
        return myself();
    public B withTypeUID(String typeUID) {
    public B withConfiguration(Configuration configuration) {
    public B withLabel(@Nullable String label) {
    public B withDescription(@Nullable String description) {
    private B myself() {
        return (B) this;
    protected String getId() {
        final String id = this.id;
            throw new IllegalStateException("The ID must not be null.");
    protected String getTypeUID() {
        final String typeUID = this.typeUID;
        if (typeUID == null) {
            throw new IllegalStateException("The type UID must not be null.");
    public abstract T build();
