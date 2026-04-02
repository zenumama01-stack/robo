public class ScriptExtensionManagerWrapperImpl implements ScriptExtensionManagerWrapper {
    private final ScriptEngineContainer container;
    private final ScriptExtensionManager manager;
    public ScriptExtensionManagerWrapperImpl(ScriptExtensionManager manager, ScriptEngineContainer container) {
        manager.addExtension(provider);
        manager.removeExtension(provider);
        return manager.getTypes();
        return container.getIdentifier();
        return manager.getPresets();
    public @Nullable Object get(String type) {
        return manager.get(type, container.getIdentifier());
        return manager.getDefaultPresets();
    public Map<String, Object> importPreset(String preset) {
        return manager.importPreset(preset, container.getFactory(), container.getScriptEngine(),
                container.getIdentifier());
