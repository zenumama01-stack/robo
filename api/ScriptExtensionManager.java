import java.util.concurrent.CopyOnWriteArraySet;
import org.openhab.core.automation.module.script.ScriptExtensionAccessor;
 * This manager allows a script import extension providers
@Component(service = { ScriptExtensionManager.class, ScriptExtensionAccessor.class })
public class ScriptExtensionManager implements ScriptExtensionAccessor {
    private final Set<ScriptExtensionProvider> scriptExtensionProviders = new CopyOnWriteArraySet<>();
    public void addScriptExtensionProvider(ScriptExtensionProvider provider) {
        scriptExtensionProviders.add(provider);
    public void removeScriptExtensionProvider(ScriptExtensionProvider provider) {
        scriptExtensionProviders.remove(provider);
    public void addExtension(ScriptExtensionProvider provider) {
    public void removeExtension(ScriptExtensionProvider provider) {
    public List<String> getTypes() {
        List<String> types = new ArrayList<>();
        for (ScriptExtensionProvider provider : scriptExtensionProviders) {
            types.addAll(provider.getTypes());
    public List<String> getPresets() {
        List<String> presets = new ArrayList<>();
            presets.addAll(provider.getPresets());
        return presets;
    public @Nullable Object get(String type, String scriptIdentifier) {
            if (provider.getTypes().contains(type)) {
                return provider.get(scriptIdentifier, type);
    public List<String> getDefaultPresets() {
        List<String> defaultPresets = new ArrayList<>();
            defaultPresets.addAll(provider.getDefaultPresets());
        return defaultPresets;
    public void importDefaultPresets(ScriptEngineFactory engineProvider, ScriptEngine scriptEngine,
            String scriptIdentifier) {
        engineProvider.scopeValues(scriptEngine, findDefaultPresets(scriptIdentifier));
    public Map<String, Object> importPreset(String preset, ScriptEngineFactory engineProvider,
            ScriptEngine scriptEngine, String scriptIdentifier) {
        Map<String, Object> rv = findPreset(preset, scriptIdentifier);
        engineProvider.scopeValues(scriptEngine, rv);
    public Map<String, Object> findDefaultPresets(String scriptIdentifier) {
        Map<String, Object> allValues = new HashMap<>();
        for (String preset : getDefaultPresets()) {
            allValues.putAll(findPreset(preset, scriptIdentifier));
        return allValues;
    public Map<String, Object> findPreset(String preset, String scriptIdentifier) {
            if (provider.getPresets().contains(preset)) {
                Map<String, Object> scopeValues = provider.importPreset(scriptIdentifier, preset);
                allValues.putAll(scopeValues);
    public void dispose(String scriptIdentifier) {
            provider.unload(scriptIdentifier);
