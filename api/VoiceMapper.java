 * Mapper class that maps {@link Voice} instanced to their respective DTOs.
public class VoiceMapper {
     * Maps a {@link Voice} to a {@link VoiceDTO}.
     * @param voice the voice
    public static VoiceDTO map(Voice voice) {
        VoiceDTO dto = new VoiceDTO();
        dto.id = voice.getUID();
        dto.label = voice.getLabel();
        dto.locale = voice.getLocale().toString();
