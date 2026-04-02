 * The ScriptEngineManager provides the ability to load and unload scripts.
 * @author Scott Rushworth - changed parameter names when implementing {@link ScriptModuleTypeProvider}
public interface ScriptEngineManager {
     * Creates a new ScriptEngine used to execute scripts, ScriptActions or ScriptConditions
     * @param engineIdentifier the unique identifier for the ScriptEngine (script file path or UUID)
     * @return ScriptEngineContainer or null
    ScriptEngineContainer createScriptEngine(String scriptType, String engineIdentifier);
     * Loads a script and initializes its scope variables
     * @param scriptData the content of the script
     * @return <code>true</code> if the script was successfully loaded, <code>false</code> otherwise
    boolean loadScript(String engineIdentifier, InputStreamReader scriptData);
     * Unloads the ScriptEngine loaded with the engineIdentifier
    void removeEngine(String engineIdentifier);
     * Checks if the supplied file extension or MimeType is supported by the existing ScriptEngineFactories
     * @return true, if supported, else false
    boolean isSupported(String scriptType);
     * Add a listener that is notified when a ScriptEngineFactory is added or removed
     * @param listener an object that implements {@link FactoryChangeListener}
    void addFactoryChangeListener(FactoryChangeListener listener);
     * Remove a listener that is notified when a ScriptEngineFactory is added or removed
    void removeFactoryChangeListener(FactoryChangeListener listener);
    interface FactoryChangeListener {
         * Called by the {@link ScriptEngineManager} when a ScriptEngineFactory is added
         * @param scriptType the script type supported by the newly added factory
        void factoryAdded(String scriptType);
         * Called by the {@link ScriptEngineManager} when a ScriptEngineFactory is removed
         * @param scriptType the script type supported by the removed factory
        void factoryRemoved(String scriptType);
