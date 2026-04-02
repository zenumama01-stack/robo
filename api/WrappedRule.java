public class WrappedRule {
    private static <T extends WrappedModule, U extends Module> List<T> map(final List<U> in, Function<U, T> factory,
            final Collection<WrappedModule<Module, ModuleHandler>> coll) {
        return in.stream().map(module -> {
            final T impl = factory.apply(module);
            coll.add(impl);
            return impl;
        }).toList();
    private RuleStatusInfo statusInfo = new RuleStatusInfo(RuleStatus.UNINITIALIZED, RuleStatusDetail.NONE);
    private final List<WrappedModule<Module, ModuleHandler>> modules;
    private final List<WrappedAction> actions;
    private final List<WrappedCondition> conditions;
    private final List<WrappedTrigger> triggers;
    public WrappedRule(final Rule rule) {
        final LinkedList<WrappedModule<Module, ModuleHandler>> modules = new LinkedList<>();
        this.actions = map(rule.getActions(), WrappedAction::new, modules);
        this.conditions = map(rule.getConditions(), WrappedCondition::new, modules);
        this.triggers = map(rule.getTriggers(), WrappedTrigger::new, modules);
        this.modules = Collections.unmodifiableList(modules);
    public final String getUID() {
        return rule.getUID();
    public final Rule unwrap() {
    public void setStatusInfo(final RuleStatusInfo statusInfo) {
    public List<WrappedAction> getActions() {
    public List<WrappedCondition> getConditions() {
    public List<WrappedTrigger> getTriggers() {
    public List<WrappedModule<Module, ModuleHandler>> getModules() {
