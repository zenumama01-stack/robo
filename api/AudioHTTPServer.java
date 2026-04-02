 * This is an interface that is implemented by {@link org.openhab.core.audio.internal.AudioServlet} and which allows
 * exposing audio streams through HTTP.
 * Streams are only served a single time and then discarded.
public interface AudioHTTPServer {
     * Creates a relative url for a given {@link AudioStream} where it can be requested a single time.
     * Note that the HTTP header only contains "Content-length", if the passed stream is an instance of
     * {@link SizeableAudioStream}.
     * If the client that requests the url expects this header field to be present, make sure to pass such an instance.
     * Streams are closed after having been served.
     * @param stream the stream to serve on HTTP
     * @return the relative URL to access the stream starting with a '/'
     * @deprecated Use {@link AudioHTTPServer#serve(AudioStream, int, boolean)}
    @Deprecated
    String serve(AudioStream stream);
     * Creates a relative url for a given {@link AudioStream} where it can be requested multiple times within the given
     * time frame.
     * This method accepts all {@link AudioStream}s, but it is better to use {@link ClonableAudioStream}s. If generic
     * {@link AudioStream} is used, the method tries to add the Clonable capability by storing it in a small memory
     * buffer, e.g {@link ByteArrayAudioStream}, or in a cached file if the stream reached the buffer capacity,
     * or fails if the stream is too long.
     * Streams are closed, once they expire.
     * @param seconds number of seconds for which the stream is available through HTTP
    String serve(AudioStream stream, int seconds);
     * Creates a relative url for a given {@link AudioStream} where it can be requested one or multiple times within the
     * given time frame.
     * This method accepts all {@link AudioStream}s, but if multiTimeStream is set to true it is better to use
     * {@link ClonableAudioStream}s. Otherwise, if a generic {@link AudioStream} is used, the method will then try
     * to add the Clonable capability by storing it in a small memory buffer, e.g {@link ByteArrayAudioStream}, or in a
     * cached file if the stream reached the buffer capacity, or fails to render the sound completely if the stream is
     * too long.
     * A {@link java.util.concurrent.CompletableFuture} is used to inform the caller that the playback ends in order to
     * clean
     * resources and run delayed task, such as restoring volume.
     * @param seconds number of seconds for which the stream is available through HTTP. The stream will be deleted only
     *            if not started, so you can set a duration shorter than the track's duration.
     * @param multiTimeStream set to true if this stream should be played multiple time, and thus needs to be made
     *            Cloneable if it is not already.
     * @return information about the {@link StreamServed}, including the relative URL to access the stream starting with
     *         a '/', and a CompletableFuture to know when the playback ends.
     * @throws IOException when the stream is not a {@link ClonableAudioStream} and we cannot get or store it on disk.
    StreamServed serve(AudioStream stream, int seconds, boolean multiTimeStream) throws IOException;
