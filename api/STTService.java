 * This is the interface that a speech-to-text service has to implement.
public interface STTService {
     * Obtain the Locales available from this STTService
     * Obtain the audio formats supported by this STTService
     * This method starts the process of speech recognition.
     * The audio data of the passed {@link AudioStream} is passed to the speech
     * recognition engine. The recognition engine attempts to recognize speech
     * as being spoken in the passed {@code Locale} and containing statements
     * specified in the passed {@code grammars}. Recognition is indicated by
     * fired {@link STTEvent} events targeting the passed {@link STTListener}.
     * The passed {@code grammars} must consist of a syntactically valid grammar
     * as specified by the JSpeech Grammar Format. If {@code grammars} is null
     * or empty, large vocabulary continuous speech recognition is attempted.
     * @see <a href="https://www.w3.org/TR/jsgf/">JSpeech Grammar Format.</a>
     * @param sttListener Non-null {@link STTListener} that {@link STTEvent} events target
     * @param audioStream The {@link AudioStream} from which speech is recognized
     * @param locale The {@code Locale} in which the target speech is spoken
     * @param grammars The JSpeech Grammar Format grammar specifying allowed statements
     * @return A {@link STTServiceHandle} used to abort recognition
     * @throws STTException if any parameter is invalid or a STT problem occurs
    STTServiceHandle recognize(STTListener sttListener, AudioStream audioStream, Locale locale, Set<String> grammars)
            throws STTException;
