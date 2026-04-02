 * This is an {@link EventFactory} for creating web audio events.
 * The only currently supported event type is {@link PlayURLEvent}.
public class WebAudioEventFactory extends AbstractEventFactory {
    private static final String PLAY_URL_TOPIC = "openhab/webaudio/playurl";
     * Constructs a new WebAudioEventFactory.
    public WebAudioEventFactory() {
        super(Set.of(PlayURLEvent.TYPE));
        if (PlayURLEvent.TYPE.equals(eventType)) {
            String url = deserializePayload(payload, String.class);
            return createPlayURLEvent(url);
        throw new IllegalArgumentException("The event type '" + eventType + "' is not supported by this factory.");
     * Creates a PlayURLEvent event.
    public static PlayURLEvent createPlayURLEvent(String url) {
        String payload = serializePayload(url);
        return new PlayURLEvent(PLAY_URL_TOPIC, payload, url);
