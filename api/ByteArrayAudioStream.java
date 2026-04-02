 * This is an implementation of an {@link AudioStream} with known length and a clone method, which is based on a simple
 * byte array.
public class ByteArrayAudioStream extends FixedLengthAudioStream {
    private final byte[] bytes;
    private final AudioFormat format;
    private final ByteArrayInputStream stream;
    public ByteArrayAudioStream(byte[] bytes, AudioFormat format) {
        this.bytes = bytes;
        this.format = format;
        this.stream = new ByteArrayInputStream(bytes);
    public AudioFormat getFormat() {
    public int read() throws IOException {
        return stream.read();
    public void close() throws IOException {
        stream.close();
    public long length() {
        return bytes.length;
    public InputStream getClonedStream() {
        return new ByteArrayAudioStream(bytes, format);
    public synchronized void mark(int readlimit) {
        stream.mark(readlimit);
    public synchronized void reset() throws IOException {
        stream.reset();
    public boolean markSupported() {
