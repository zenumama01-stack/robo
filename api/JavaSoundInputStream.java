 * This is an AudioStream from a Java sound API input
public class JavaSoundInputStream extends AudioStream {
     * InputStream for the input
    private final InputStream input;
     * Constructs a JavaSoundInputStream with the passed input and a close handler.
     * @param input The mic which data is pulled from
    public JavaSoundInputStream(InputStream input, AudioFormat format) {
        this.input = input;
        byte[] b = new byte[1];
        int bytesRead = read(b);
        if (-1 == bytesRead) {
            return bytesRead;
        Byte bb = Byte.valueOf(b[0]);
        return bb.intValue();
        return input.read(b, 0, b.length);
        return input.read(b, off, len);
        input.close();
