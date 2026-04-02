 * A DTO that is used on the REST API to provide infos about {@link AudioSink} to UIs.
@Schema(name = "AudioSink")
public class AudioSinkDTO {
    public AudioSinkDTO(String id, @Nullable String label) {
