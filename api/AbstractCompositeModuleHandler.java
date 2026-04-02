package org.openhab.core.automation.internal.composite;
 * This class is base implementation of all system composite module handlers: {@link CompositeTriggerHandler},
 * {@link CompositeConditionHandler} and {@link CompositeActionHandler}. The instances of these handlers are created by
 * {@link CompositeModuleHandlerFactory}.
 * The composite module handlers have to serve modules of composite module types. These handlers are responsible to
 * propagate configuration properties and input values of composite module to the child modules defined by the composite
 * module type and to call the handlers which are responsible for the child modules.
 * @param <M> type of module. It can be {@link Trigger}, {@link Condition} or {@link Action}
 * @param <MT> type of module type. It can be {@link TriggerType}, {@link ConditionType} or {@link ActionType}
 * @param <H> type of module handler. It can be {@link TriggerHandler}, {@link ConditionHandler} or
 *            {@link ActionHandler}
public abstract class AbstractCompositeModuleHandler<M extends Module, MT extends ModuleType, H extends ModuleHandler>
        implements ModuleHandler {
    protected LinkedHashMap<M, @Nullable H> moduleHandlerMap;
    protected M module;
    protected MT moduleType;
     * This constructor creates composite module handler base on composite module, module type of the module and map of
     * pairs of child module instances and corresponding handlers.
     * @param module module of composite type.
     * @param moduleType composite module type. This is the type of module.
     * @param mapModuleToHandler map containing pairs of child modules instances (defined by module type) and their
     *            handlers
    protected AbstractCompositeModuleHandler(M module, MT moduleType,
            LinkedHashMap<M, @Nullable H> mapModuleToHandler) {
        this.moduleType = moduleType;
        this.moduleHandlerMap = mapModuleToHandler;
     * Creates internal composite context which will be used for resolving child module's context.
     * @param context contains composite inputs and composite configuration.
     * @return context that will be passed to the child module
    protected Map<String, Object> getCompositeContext(Map<String, ?> context) {
        Map<String, Object> result = new HashMap<>(context);
        result.putAll(module.getConfiguration().getProperties());
     * Creates child context that will be passed to the child handler.
     * @param child Composite ModuleImpl's child
     * @param compositeContext context with which child context will be resolved.
     * @return child context ready to be passed to the child for execution.
    protected Map<String, Object> getChildContext(Module child, Map<String, ?> compositeContext) {
        return ReferenceResolver.getCompositeChildContext(child, compositeContext);
        List<M> children = getChildren();
        for (M child : children) {
            ModuleHandler childHandler = moduleHandlerMap.remove(child);
            if (childHandler != null) {
                childHandler.dispose();
        moduleHandlerMap.clear();
            H handler = moduleHandlerMap.get(child);
                handler.setCallback(callback);
    protected abstract List<M> getChildren();
