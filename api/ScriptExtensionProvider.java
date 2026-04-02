 * A {@link ScriptExtensionProvider} can provide variable and types on ScriptEngine instance basis.
public interface ScriptExtensionProvider {
     * These presets will always get injected into the ScriptEngine on instance creation.
     * @return collection of presets
    Collection<String> getDefaultPresets();
     * Returns the provided Presets which are supported by this ScriptExtensionProvider.
     * Presets define imports which will be injected into the ScriptEngine if called by "importPreset".
     * @return provided presets
    Collection<String> getPresets();
     * Returns the supported types which can be received by the given ScriptExtensionProvider
     * @return provided types
    Collection<String> getTypes();
     * This method should return an Object of the given type. Note: get can be called multiple times in the scripts use
     * caching where appropriate.
     * @param scriptIdentifier the identifier of the script that requests the given type
     * @param type the type that is requested (must be part of the collection returned by the {@code #getTypes()} method
     * @return the requested type or null
     * @throws IllegalArgumentException if the given type does not match to one returned by the {@code #getTypes()}
     *             method
    Object get(String scriptIdentifier, String type) throws IllegalArgumentException;
     * This method should return variables and types of the concrete type which will be injected into the ScriptEngines
     * scope.
     * @param scriptIdentifier the identifier of the script that receives the preset
     * @return the presets, must be non-null (use an empty map instead)
    Map<String, Object> importPreset(String scriptIdentifier, String preset);
     * This will be called when the ScriptEngine will be unloaded (e.g. if the Script is deleted or updated).
     * Every Context information stored in the ScriptExtensionProvider should be removed.
     * @param scriptIdentifier the identifier of the script that is unloaded
    void unload(String scriptIdentifier);
