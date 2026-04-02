import javazoom.jl.decoder.Bitstream;
import javazoom.jl.decoder.BitstreamException;
import javazoom.jl.decoder.Header;
import javax.sound.sampled.UnsupportedAudioFileException;
public class AudioSinkUtilsImpl implements AudioSinkUtils {
    private final Logger logger = LoggerFactory.getLogger(AudioSinkUtilsImpl.class);
    public @Nullable Long transferAndAnalyzeLength(InputStream in, OutputStream out, AudioFormat audioFormat)
            throws IOException {
        // take some data from the stream beginning
        byte[] dataBytes = in.readNBytes(8192);
        // beginning sound timestamp :
        long startTime = System.nanoTime();
        // copy already read data to the output stream :
        out.write(dataBytes);
        // transfer everything else
        Long dataTransferedLength = dataBytes.length + in.transferTo(out);
        if (dataTransferedLength > 0) {
            if (AudioFormat.CODEC_PCM_SIGNED.equals(audioFormat.getCodec())) {
                try (AudioInputStream audioInputStream = AudioSystem
                        .getAudioInputStream(new ByteArrayInputStream(dataBytes))) {
                    int frameSize = audioInputStream.getFormat().getFrameSize();
                    float frameRate = audioInputStream.getFormat().getFrameRate();
                    long computedDuration = Float.valueOf((dataTransferedLength / (frameSize * frameRate)) * 1000000000)
                            .longValue();
                    return startTime + computedDuration;
                } catch (IOException | UnsupportedAudioFileException e) {
                    logger.debug("Cannot compute the duration of input stream with method java stream sound analysis",
                    Integer bitRate = audioFormat.getBitRate();
                    if (bitRate != null && bitRate != 0) {
                        long computedDuration = Float.valueOf((8f * dataTransferedLength / bitRate) * 1000000000)
                        logger.debug("Cannot compute the duration of input stream by using audio format information");
            } else if (AudioFormat.CODEC_MP3.equals(audioFormat.getCodec())) {
                // not accurate, no VBR support, but better than nothing
                Bitstream bitstream = new Bitstream(new ByteArrayInputStream(dataBytes));
                    Header h = bitstream.readFrame();
                    if (h != null) {
                        long computedDuration = Float.valueOf(h.total_ms(dataTransferedLength.intValue()) * 1000000)
                } catch (BitstreamException ex) {
                    logger.debug("Cannot compute the duration of input stream", ex);
