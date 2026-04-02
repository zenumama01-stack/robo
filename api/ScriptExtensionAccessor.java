 * Accessor allowing script engines to lookup presets.
public interface ScriptExtensionAccessor {
     * Access the default presets for a script engine
     * @param scriptIdentifier the identifier for the script engine
     * @return map of preset objects
    Map<String, Object> findDefaultPresets(String scriptIdentifier);
     * Access specific presets for a script engine
     * @param preset the name of the preset
    Map<String, Object> findPreset(String preset, String scriptIdentifier);
