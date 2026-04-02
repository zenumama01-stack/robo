import org.openhab.core.model.script.actions.Voice;
 * This class registers an OSGi service for the voice action.
public class VoiceActionService implements ActionService {
    public static @Nullable VoiceManager voiceManager;
    public VoiceActionService(final @Reference VoiceManager voiceManager, final @Reference AudioManager audioManager) {
        VoiceActionService.voiceManager = voiceManager;
        VoiceActionService.audioManager = audioManager;
        return Voice.class;
