import org.openhab.core.automation.Action;
import org.openhab.core.automation.handler.BaseModuleHandlerFactory;
import org.openhab.core.automation.handler.ModuleHandler;
import org.openhab.core.automation.handler.ModuleHandlerFactory;
import org.openhab.core.voice.VoiceManager;
@Component(service = ModuleHandlerFactory.class)
public class MediaModuleHandlerFactory extends BaseModuleHandlerFactory {
    private static final Collection<String> TYPES = List.of(SayActionHandler.TYPE_ID, PlayActionHandler.TYPE_ID);
    private final VoiceManager voiceManager;
    public MediaModuleHandlerFactory(final @Reference AudioManager audioManager,
            final @Reference VoiceManager voiceManager) {
        this.voiceManager = voiceManager;
        super.deactivate();
    public Collection<String> getTypes() {
        return TYPES;
    protected @Nullable ModuleHandler internalCreate(Module module, String ruleUID) {
        if (module instanceof Action action) {
            switch (module.getTypeUID()) {
                case SayActionHandler.TYPE_ID:
                    return new SayActionHandler(action, voiceManager);
                case PlayActionHandler.TYPE_ID:
                    return new PlayActionHandler(action, audioManager);
                case SynthesizeActionHandler.TYPE_ID:
                    return new SynthesizeActionHandler(action, audioManager);
