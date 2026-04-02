import org.openhab.core.automation.internal.ModuleImpl;
import org.openhab.core.automation.internal.RuleEngineImpl;
 * This class is a factory for system module handler for modules of composite module types: {@link CompositeTriggerType}
 * , {@link CompositeConditionType} and {@link CompositeActionType}. The composite module type is a type which contains
 * one or more internal (child) modules and these modules have access to configuration properties and inputs of
 * composite module. The outputs of module of composite type (if they exists) are set these handlers and they are base
 * on the values of child module outputs.
 * The {@link CompositeModuleHandlerFactory} is a system handler factory and it is not registered as service in OSGi
 * framework, but it will be used by the rule engine to serve composite module types without any action of the user.
public class CompositeModuleHandlerFactory extends BaseModuleHandlerFactory implements ModuleHandlerFactory {
    private final Logger logger = LoggerFactory.getLogger(CompositeModuleHandlerFactory.class);
     * The constructor of system handler factory for composite module types.
     * @param mtRegistry is a module type registry
     * @param re is a rule engine
    public CompositeModuleHandlerFactory(ModuleTypeRegistry mtRegistry, RuleEngineImpl re) {
        this.mtRegistry = mtRegistry;
        this.ruleEngine = re;
     * It is system factory and must not be registered as service. This method is not used.
     * @see org.openhab.core.automation.handler.ModuleHandlerFactory#getTypes()
        return new ArrayList<>();
    @SuppressWarnings({ "unchecked" })
    public void ungetHandler(Module module, String childModulePrefix, ModuleHandler handler) {
        ModuleHandler handlerOfModule = getHandlers().get(getModuleIdentifier(childModulePrefix, module.getId()));
        if (handlerOfModule instanceof AbstractCompositeModuleHandler) {
            AbstractCompositeModuleHandler<ModuleImpl, ?, ?> h = (AbstractCompositeModuleHandler<ModuleImpl, ?, ?>) handlerOfModule;
            for (Entry<ModuleImpl, @Nullable ? extends ModuleHandler> entry : h.moduleHandlerMap.entrySet()) {
                ModuleImpl child = entry.getKey();
                ModuleHandler childHandler = entry.getValue();
                if (childHandler == null) {
                ModuleHandlerFactory mhf = ruleEngine.getModuleHandlerFactory(child.getTypeUID());
                mhf.ungetHandler(child, childModulePrefix + ":" + module.getId(), childHandler);
        String ruleId = getRuleId(childModulePrefix);
        super.ungetHandler(module, ruleId, handler);
    private String getRuleId(String childModulePrefix) {
        int i = childModulePrefix.indexOf(':');
        return i != -1 ? childModulePrefix.substring(0, i) : childModulePrefix;
    public @Nullable ModuleHandler internalCreate(Module module, String ruleUID) {
        ModuleHandler handler = null;
        String moduleType = module.getTypeUID();
        ModuleType mt = mtRegistry.get(moduleType);
        if (mt instanceof CompositeTriggerType type) {
            List<Trigger> childModules = type.getChildren();
            LinkedHashMap<Trigger, @Nullable TriggerHandler> mapModuleToHandler = getChildHandlers(module.getId(),
                    module.getConfiguration(), childModules, ruleUID);
            if (mapModuleToHandler != null) {
                handler = new CompositeTriggerHandler((Trigger) module, type, mapModuleToHandler, ruleUID);
        } else if (mt instanceof CompositeConditionType type) {
            List<Condition> childModules = type.getChildren();
            LinkedHashMap<Condition, @Nullable ConditionHandler> mapModuleToHandler = getChildHandlers(module.getId(),
                handler = new CompositeConditionHandler((Condition) module, type, mapModuleToHandler, ruleUID);
        } else if (mt instanceof CompositeActionType type) {
            List<Action> childModules = type.getChildren();
            LinkedHashMap<Action, @Nullable ActionHandler> mapModuleToHandler = getChildHandlers(module.getId(),
                handler = new CompositeActionHandler((Action) module, type, mapModuleToHandler, ruleUID);
            logger.debug("Set module handler: {}  -> {} of rule {}.", module.getId(),
                    handler.getClass().getSimpleName() + "(" + moduleType + ")", ruleUID);
            logger.debug("Not found module handler {} for moduleType {} of rule {}.", module.getId(), moduleType,
     * This method associates module handlers to the child modules of composite module types. It links module types of
     * child modules to the rule which contains this composite module. It also resolve links between child configuration
     * properties and configuration of composite module see:
     * {@link ReferenceResolver#updateConfiguration(Configuration, Map, Logger)}.
     * @param compositeConfig configuration values of composite module.
     * @param childModules list of child modules
     * @param childModulePrefix defines UID of child module. The rule id is not enough for prefix when a composite type
     *            is used more then one time in one and the same rule. For example the prefix can be:
     *            ruleId:compositeModuleId:compositeModileId2.
     * @return map of pairs of module and its handler. Return null when some of the child modules can not find its
     *         handler.
    private <T extends Module, @Nullable MT extends ModuleHandler> @Nullable LinkedHashMap<T, MT> getChildHandlers(
            String compositeModuleId, Configuration compositeConfig, List<T> childModules, String childModulePrefix) {
        LinkedHashMap<T, MT> mapModuleToHandler = new LinkedHashMap<>();
        for (T child : childModules) {
            ruleEngine.updateMapModuleTypeToRule(ruleId, child.getTypeUID());
            ModuleHandlerFactory childMhf = ruleEngine.getModuleHandlerFactory(child.getTypeUID());
            if (childMhf == null) {
                mapModuleToHandler.clear();
                mapModuleToHandler = null;
            ReferenceResolver.updateConfiguration(child.getConfiguration(), compositeConfig.getProperties(), logger);
            MT childHandler = (MT) childMhf.getHandler(child, childModulePrefix + ":" + compositeModuleId);
            mapModuleToHandler.put(child, childHandler);
        return mapModuleToHandler;
