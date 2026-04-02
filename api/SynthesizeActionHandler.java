 * This is an ModuleHandler implementation for Actions that synthesize a tone melody.
public class SynthesizeActionHandler extends BaseActionModuleHandler {
    public static final String TYPE_ID = "media.SynthesizeAction";
    public static final String PARAM_MELODY = "melody";
    private final String melody;
    public SynthesizeActionHandler(Action module, AudioManager audioManager) {
        this.melody = module.getConfiguration().get(PARAM_MELODY).toString();
        audioManager.playMelody(melody, sink, volume);
