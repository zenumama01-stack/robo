import java.io.DataInputStream;
import javax.sound.sampled.AudioFormat.Encoding;
 * Some utility methods for parsing and cleaning wav files
public class AudioWaveUtils {
     * This "magic" packet marks the beginning of the read data
    private static final int DATA_MAGIC = 0x64617461;
    private static final AudioFormat DEFAULT_WAVE_AUDIO_FORMAT = new AudioFormat(AudioFormat.CONTAINER_WAVE,
            AudioFormat.CODEC_PCM_SIGNED, false, 16, 705600, 44100L, 1);
     * @param inputStream an InputStream of a wav file to analyze. The InputStream must have a fmt header
     *            and support the mark/reset method
     * @return The audio format, or the default audio format if an error occured
     * @throws IOException If i/o exception occurs, or if the InputStream doesn't support the mark/reset
    public static AudioFormat parseWavFormat(InputStream inputStream) throws IOException {
            inputStream.mark(200); // arbitrary amount, also used by the underlying parsing package from Sun
            javax.sound.sampled.AudioFormat format = AudioSystem.getAudioInputStream(inputStream).getFormat();
            Encoding javaSoundencoding = format.getEncoding();
            String codecPCMSignedOrUnsigned;
            if (Encoding.PCM_SIGNED.equals(javaSoundencoding)) {
                codecPCMSignedOrUnsigned = AudioFormat.CODEC_PCM_SIGNED;
            } else if (Encoding.PCM_UNSIGNED.equals(javaSoundencoding)) {
                codecPCMSignedOrUnsigned = AudioFormat.CODEC_PCM_UNSIGNED;
            } else if (Encoding.ULAW.equals(javaSoundencoding)) {
                codecPCMSignedOrUnsigned = AudioFormat.CODEC_PCM_ULAW;
            } else if (Encoding.ALAW.equals(javaSoundencoding)) {
                codecPCMSignedOrUnsigned = AudioFormat.CODEC_PCM_ALAW;
                codecPCMSignedOrUnsigned = null;
            Integer bitRate = Math.round(format.getSampleRate() * format.getSampleSizeInBits() * format.getChannels());
            Long frequency = Float.valueOf(format.getSampleRate()).longValue();
            return new AudioFormat(AudioFormat.CONTAINER_WAVE, codecPCMSignedOrUnsigned, format.isBigEndian(),
                    format.getSampleSizeInBits(), bitRate, frequency, format.getChannels());
        } catch (UnsupportedAudioFileException e) {
            // do not throw exception and assume default format to let a chance for the sink to play the stream.
            return DEFAULT_WAVE_AUDIO_FORMAT;
            inputStream.reset();
     * Remove FMT block (WAV header) from a stream by consuming it until
     * the magic packet signaling data. Limit to 200 readInt() (arbitrary value
     * used in sun audio package).
     * If you don't remove/consume the FMT and pass the data to a player
     * as if it is a pure PCM stream, it could try to play it and will
     * do a "click" noise at the beginning.
     * @param data A wav container in an InputStream
     * @throws IOException
    public static void removeFMT(InputStream data) throws IOException {
        DataInputStream dataInputStream = new DataInputStream(data);
        int nextInt = dataInputStream.readInt();
        while (nextInt != DATA_MAGIC && i < 200) {
            nextInt = dataInputStream.readInt();
        dataInputStream.readInt();
