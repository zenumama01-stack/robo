 * This is a base class that can be used by any ModuleHandler implementation
public class BaseModuleHandler<T extends Module> implements ModuleHandler {
    protected T module;
    protected @Nullable ModuleHandlerCallback callback;
    public BaseModuleHandler(T module) {
        this.module = module;
        this.callback = null;
     * Returns the configuration of the module.
     * @return configuration of the module
    protected Configuration getConfig() {
        return module.getConfiguration();
     * Returns the configuration of the module and transforms it to the given class.
     * @param configurationClass configuration class
     * @return configuration of module in form of the given class
    protected <C> C getConfigAs(Class<C> configurationClass) {
        return getConfig().as(configurationClass);
