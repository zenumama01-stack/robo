import org.openhab.core.automation.handler.BaseActionModuleHandler;
 * This is a ModuleHandler implementation for Actions that play a sound file from the file system.
public class PlayActionHandler extends BaseActionModuleHandler {
    public static final String TYPE_ID = "media.PlayAction";
    public static final String PARAM_SOUND = "sound";
    public static final String PARAM_SINK = "sink";
    public static final String PARAM_VOLUME = "volume";
    private final Logger logger = LoggerFactory.getLogger(PlayActionHandler.class);
    private final String sound;
    private final @Nullable String sink;
    private final @Nullable PercentType volume;
    public PlayActionHandler(Action module, AudioManager audioManager) {
        super(module);
        this.sound = module.getConfiguration().get(PARAM_SOUND).toString();
        Object sinkParam = module.getConfiguration().get(PARAM_SINK);
        this.sink = sinkParam != null ? sinkParam.toString() : null;
        Object volumeParam = module.getConfiguration().get(PARAM_VOLUME);
        this.volume = volumeParam instanceof BigDecimal bd ? new PercentType(bd) : null;
    public @Nullable Map<String, @Nullable Object> execute(Map<String, Object> context) {
            audioManager.playFile(sound, sink, volume);
            logger.error("Error playing sound '{}': {}", sound, e.getMessage());
