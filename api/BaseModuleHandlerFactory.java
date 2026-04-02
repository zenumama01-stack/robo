 * This class provides a {@link ModuleHandlerFactory} base implementation. It is used by its subclasses for base
 * implementation of creating and disposing {@link ModuleHandler} instances. They only have to implement
 * {@link #internalCreate(Module, String)} method for creating concrete instances needed for the operation of the
 * {@link Module}s.
 * @author Benedikt Niehues - change behavior for unregistering ModuleHandler
public abstract class BaseModuleHandlerFactory implements ModuleHandlerFactory {
    private final Map<String, ModuleHandler> handlers = new HashMap<>();
     * Should be overridden by the implementations that extend this base class. Called from DS to deactivate the
     * {@link ModuleHandlerFactory}.
        for (ModuleHandler handler : handlers.values()) {
            handler.dispose();
        handlers.clear();
     * Provides all available {@link ModuleHandler}s created by concrete factory implementation.
     * @return a map with keys calculated by concatenated rule UID and module Id and values representing
     *         {@link ModuleHandler} created for concrete module corresponding to the module Id and belongs to rule with
     *         such UID.
    protected Map<String, ModuleHandler> getHandlers() {
        return Collections.unmodifiableMap(handlers);
    public @Nullable ModuleHandler getHandler(Module module, String ruleUID) {
        String id = getModuleIdentifier(ruleUID, module.getId());
        ModuleHandler handler = handlers.get(id);
        handler = handler == null ? internalCreate(module, ruleUID) : handler;
            handlers.put(id, handler);
     * Creates a new {@link ModuleHandler} for a given {@code module} and {@code ruleUID}.
     * @param module the {@link Module} for which a handler should be created.
     * @param ruleUID the identifier of the {@link Rule} that the given module belongs to.
     * @return a {@link ModuleHandler} instance or {@code null} if this module type is not supported.
    protected abstract @Nullable ModuleHandler internalCreate(Module module, String ruleUID);
    public void ungetHandler(Module module, String ruleUID, ModuleHandler handler) {
        if (handlers.remove(getModuleIdentifier(ruleUID, module.getId()), handler)) {
    protected String getModuleIdentifier(String ruleUid, String moduleId) {
        return ruleUid + "$" + moduleId;
