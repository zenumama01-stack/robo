 * This is the interface that a text-to-speech service has to implement.
 * @author Kai Kreuzer - Refactored to use AudioStreams
public interface TTSService {
     * Obtain the voices available from this TTSService
     * @return The voices available from this service
    Set<Voice> getAvailableVoices();
     * Obtain the audio formats supported by this TTSService
    AudioStream synthesize(String text, Voice voice, AudioFormat requestedFormat) throws TTSException;
