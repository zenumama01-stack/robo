 * A DTO that is used on the REST API to provide infos about {@link AudioSource} to UIs.
@Schema(name = "AudioSource")
public class AudioSourceDTO {
    public AudioSourceDTO(String id, @Nullable String label) {
