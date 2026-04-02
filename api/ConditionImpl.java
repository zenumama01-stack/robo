 * This class is implementation of {@link Condition} modules used in the {@link RuleEngineImpl}s.
public class ConditionImpl extends ModuleImpl implements Condition {
     * Constructor of {@link Condition} module object.
     * @param id id of the module.
     * @param typeUID unique module type id.
     * @param configuration configuration values of the {@link Condition} module.
     * @param inputs set of {@link Input}s used by this module.
    public ConditionImpl(String id, String typeUID, @Nullable Configuration configuration, @Nullable String label,
        super(id, typeUID, configuration, label, description);
     * This method is used to get input connections of the Condition. The connections
     * are links between {@link Input}s of the current {@link Module} and {@link Output}s of other
     * @return map that contains the inputs of this condition.
public class ConditionImpl implements Condition {
    private @Nullable String item;
    private @Nullable String condition;
    private String value = "";
    public ConditionImpl() {
    public ConditionImpl(Condition condition) {
        this.item = condition.getItem();
        this.condition = condition.getCondition();
        this.value = condition.getValue();
    public @Nullable String getItem() {
    public void setItem(@Nullable String item) {
    public @Nullable String getCondition() {
    public void setCondition(@Nullable String condition) {
        this.condition = condition;
    public void setValue(String value) {
