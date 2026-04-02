 * This is a ModuleHandler implementation for Actions that trigger a TTS output through "say".
public class SayActionHandler extends BaseActionModuleHandler {
    public static final String TYPE_ID = "media.SayAction";
    public static final String PARAM_TEXT = "text";
    private final String text;
    public SayActionHandler(Action module, VoiceManager voiceManager) {
        this.text = module.getConfiguration().get(PARAM_TEXT).toString();
        voiceManager.say(text, null, sink, volume);
