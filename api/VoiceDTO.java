import org.openhab.core.voice.Voice;
 * A DTO that is used on the REST API to provide infos about {@link Voice} to UIs.
@Schema(name = "Voice")
public class VoiceDTO {
    public String locale;
