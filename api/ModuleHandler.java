 * A common interface for all module Handler interfaces. The Handler interfaces are
 * bridge between RuleManager and external modules used by the RuleManager.
 * @see ModuleHandlerFactory
public interface ModuleHandler {
     * The method is called by RuleManager to free resources when {@link ModuleHandler} is released.
     * The callback is injected to the handler through this method.
     * @param callback a {@link ModuleHandlerCallback} instance
    void setCallback(ModuleHandlerCallback callback);
