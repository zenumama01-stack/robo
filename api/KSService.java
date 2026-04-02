 * This is the interface that a keyword spotting service has to implement.
 * @author Kai Kreuzer - Refactored to use AudioStream
public interface KSService extends DTService {
     * Obtain the Locales available from this KSService
     * @return The Locales available from this service
    Set<Locale> getSupportedLocales();
     * Obtain the audio formats supported by this KSService
     * This method starts the process of keyword spotting
     * The audio data of the passed {@link AudioStream} is passed to the keyword
     * spotting engine. The keyword spotting attempts to spot {@code keyword} as
     * being spoken in the passed {@code Locale}. Spotted keyword is indicated by
     * fired {@link KSEvent} events targeting the passed {@link KSListener}.
     * The passed {@link AudioStream} must be of a supported {@link AudioFormat}.
     * In other words an {@link AudioFormat} compatible with one returned from
     * the {@code getSupportedFormats()} method.
     * The passed {@code Locale} must be supported. That is to say it must be
     * a {@code Locale} returned from the {@code getSupportedLocales()} method.
     * The passed {@code keyword} is the keyword which should be spotted.
     * The method is supposed to return fast, i.e. it should only start the spotting as a background process.
     * @param ksListener Non-null {@link KSListener} that {@link KSEvent} events target
     * @param audioStream The {@link AudioStream} from which keywords are spotted
     * @param locale The {@code Locale} in which the target keywords are spoken
     * @param keyword The keyword which to spot
     * @return A {@link KSServiceHandle} used to abort keyword spotting
     * @throws KSException if any parameter is invalid or a problem occurs
    KSServiceHandle spot(KSListener ksListener, AudioStream audioStream, Locale locale, String keyword)
            throws KSException;
