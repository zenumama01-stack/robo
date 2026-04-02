import org.openhab.core.cache.lru.InputStreamCacheWrapper;
 * Implements AudioStream methods, with an inner stream extracted from cache
public class AudioStreamFromCache extends FixedLengthAudioStream {
    private InputStreamCacheWrapper inputStream;
    private AudioFormat audioFormat;
    private String key;
    public AudioStreamFromCache(InputStreamCacheWrapper inputStream, AudioFormatInfo audioFormat, String key) {
        this.audioFormat = audioFormat.toAudioFormat();
        return inputStream.read(b, off, len);
        return inputStream.length();
            return inputStream.getClonedStream();
            throw new AudioException("Cannot get cloned AudioStream", e);
    public long skip(long n) throws IOException {
        return inputStream.skip(n);
    public void skipNBytes(long n) throws IOException {
        inputStream.skipNBytes(n);
    public int available() throws IOException {
        return inputStream.available();
        inputStream.mark(readlimit);
        return inputStream.markSupported();
