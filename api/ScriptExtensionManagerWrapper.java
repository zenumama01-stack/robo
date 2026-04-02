import org.openhab.core.automation.module.script.internal.ScriptExtensionManager;
public interface ScriptExtensionManagerWrapper {
    List<String> getTypes();
    List<String> getPresets();
    Object get(String type);
    String getScriptIdentifier();
    List<String> getDefaultPresets();
     * Imports a collection of named host objects/classes into a script engine instance. Sets of objects are provided
     * under their object name, and categorized by preset name. This method will import all named objects for a specific
     * preset name.
     * @param preset the name of the preset to import
     * @return a map of host object names to objects
     * @see ScriptExtensionManager
    Map<String, Object> importPreset(String preset);
