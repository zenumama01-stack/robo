import org.openhab.core.automation.module.script.ScriptExtensionProvider;
 * This is a scope provider for features that are related to audio and voice support.
public class MediaScriptScopeProvider implements ScriptExtensionProvider {
    private static final String MEDIA_PRESET_NAME = "media";
    private static final String AUDIO_MANAGER_NAME = "audio";
    private static final String VOICE_MANAGER_NAME = "voice";
    private final Map<String, Object> elements = new HashMap<>();
    @Reference
    protected void setAudioManager(AudioManager audioManager) {
        elements.put(AUDIO_MANAGER_NAME, audioManager);
    protected void unsetAudioManager(AudioManager audioManager) {
        elements.remove(AUDIO_MANAGER_NAME);
    protected void setVoiceManager(VoiceManager voiceManager) {
        elements.put(VOICE_MANAGER_NAME, voiceManager);
    protected void unsetVoiceManager(VoiceManager voiceManager) {
        elements.remove(VOICE_MANAGER_NAME);
    public Collection<String> getDefaultPresets() {
        return Set.of(MEDIA_PRESET_NAME);
    public Collection<String> getPresets() {
        return elements.keySet();
    public @Nullable Object get(String scriptIdentifier, String type) {
        return elements.get(type);
    public Map<String, Object> importPreset(String scriptIdentifier, String preset) {
    public void unload(String scriptIdentifier) {
