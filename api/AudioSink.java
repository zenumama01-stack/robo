import java.util.concurrent.CompletableFuture;
 * Definition of an audio output like headphones, a speaker or for writing to
 * a file / clip.
 * @author Christoph Weitkamp - Added getSupportedStreams() and UnsupportedAudioStreamException
public interface AudioSink {
     * Returns a simple string that uniquely identifies this service
     * @return an id that identifies this service
     * Returns a localized human readable label that can be used within UIs.
     * @param locale the locale to provide the label for
     * @return a localized string to be used in UIs
    String getLabel(@Nullable Locale locale);
     * Processes the passed {@link AudioStream}
     * If the passed {@link AudioStream} is not supported by this instance, an {@link UnsupportedAudioStreamException}
     * is thrown.
     * If the passed {@link AudioStream} has an {@link AudioFormat} not supported by this instance,
     * an {@link UnsupportedAudioFormatException} is thrown.
     * In case the audioStream is null, this should be interpreted as a request to end any currently playing stream.
     * When the stream is not needed anymore, if the stream implements the {@link org.openhab.core.common.Disposable}
     * interface, the sink should hereafter get rid of it by calling the dispose method.
     * @param audioStream the audio stream to play or null to keep quiet
     * @throws UnsupportedAudioFormatException If audioStream format is not supported
     * @throws UnsupportedAudioStreamException If audioStream is not supported
     * @deprecated Use {@link AudioSink#processAndComplete(AudioStream)}
    void process(@Nullable AudioStream audioStream)
            throws UnsupportedAudioFormatException, UnsupportedAudioStreamException;
     * Processes the passed {@link AudioStream}, and returns a CompletableFuture that should complete when the sound is
     * fully played. It is the sink responsibility to complete this future.
     * @return A future completed when the sound is fully played. The method can instead complete with
     *         UnsupportedAudioFormatException if the audioStream format is not supported, or
     *         UnsupportedAudioStreamException If audioStream is not supported
    default CompletableFuture<@Nullable Void> processAndComplete(@Nullable AudioStream audioStream) {
            process(audioStream);
        } catch (UnsupportedAudioFormatException | UnsupportedAudioStreamException e) {
            return CompletableFuture.failedFuture(e);
        return CompletableFuture.completedFuture(null);
     * Gets a set containing all supported audio formats
     * @return A Set containing all supported audio formats
    Set<AudioFormat> getSupportedFormats();
     * Gets a set containing all supported audio stream formats
     * @return A Set containing all supported audio stream formats
    Set<Class<? extends AudioStream>> getSupportedStreams();
     * Gets the volume
     * @return a PercentType value between 0 and 100 representing the actual volume
     * @throws IOException if the volume can not be determined
    PercentType getVolume() throws IOException;
     * Sets the volume
     * @param volume a PercentType value between 0 and 100 representing the desired volume
     * @throws IOException if the volume can not be set
    void setVolume(PercentType volume) throws IOException;
