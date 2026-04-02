 * This interface represents a factory for {@link ModuleHandler} instances. It is used for creating
 * and disposing the {@link TriggerHandler}s, {@link ConditionHandler}s and {@link ActionHandler}s
 * needed for the operation of the {@link Module}s included in {@link Rule}s.
 * {@link ModuleHandlerFactory} implementations must be registered as services in the OSGi framework.
public interface ModuleHandlerFactory {
     * Returns the UIDs of the module types currently supported by this factory.
     * A {@link ModuleHandlerFactory} instance can add new types to this list, but should not remove. If a
     * module type is no longer supported, the {@link ModuleHandlerFactory} service must be unregistered, and
     * then registered again with the new list.
     * If two or more {@link ModuleHandlerFactory}s support the same module type, the Rule Engine will choose
     * one of them randomly. Once a factory is chosen, it will be used to create instances of this module
     * type until its service is unregistered.
     * @return collection of module type UIDs supported by this factory.
     * Creates a {@link ModuleHandler} instance needed for the operation of the {@link Module}s
     * included in {@link Rule}s.
     * @param module the {@link Module} for which a {@link ModuleHandler} instance must be created.
     * @return a new {@link ModuleHandler} instance, or {@code null} if the type of the
     *         {@code module} parameter is not supported by this factory.
    ModuleHandler getHandler(Module module, String ruleUID);
     * Releases the {@link ModuleHandler} instance when it is not needed anymore
     * for handling the specified {@code module} in the {@link Rule} with the specified {@code ruleUID}.
     * If no other {@link Rule}s and {@link Module}s use this {@code handler} instance, it should be disposed.
     * @param module the {@link Module} for which the {@code handler} was created.
     * @param handler the {@link ModuleHandler} instance that is no longer needed.
    void ungetHandler(Module module, String ruleUID, ModuleHandler handler);
