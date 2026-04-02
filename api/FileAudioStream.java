import java.io.BufferedInputStream;
import java.io.FileNotFoundException;
import org.openhab.core.audio.utils.AudioStreamUtils;
import org.openhab.core.audio.utils.AudioWaveUtils;
 * This is an AudioStream from an audio file
 * @author Kai Kreuzer - Refactored to take a file as input
 * @author Christoph Weitkamp - Refactored use of filename extension
public class FileAudioStream extends FixedLengthAudioStream implements Disposable {
    public static final String WAV_EXTENSION = "wav";
    public static final String MP3_EXTENSION = "mp3";
    public static final String OGG_EXTENSION = "ogg";
    public static final String AAC_EXTENSION = "aac";
    private final File file;
    private final AudioFormat audioFormat;
    private FileInputStream inputStream;
    private final long length;
    private final boolean isTemporaryFile;
    private int markedOffset = 0;
    private int alreadyRead = 0;
    public FileAudioStream(File file) throws AudioException {
        this(file, getAudioFormat(file));
    public FileAudioStream(File file, AudioFormat format) throws AudioException {
        this(file, format, false);
    public FileAudioStream(File file, AudioFormat format, boolean isTemporaryFile) throws AudioException {
        this.file = file;
        this.inputStream = getInputStream(file);
        this.audioFormat = format;
        this.length = file.length();
        this.isTemporaryFile = isTemporaryFile;
    private static AudioFormat getAudioFormat(File file) throws AudioException {
        final String filename = file.getName().toLowerCase();
        final String extension = AudioStreamUtils.getExtension(filename);
        switch (extension) {
            case WAV_EXTENSION:
                return parseWavFormat(file);
            case MP3_EXTENSION:
                return AudioFormat.MP3;
            case OGG_EXTENSION:
                return AudioFormat.OGG;
            case AAC_EXTENSION:
                return AudioFormat.AAC;
                throw new AudioException("Unsupported file extension!");
    private static AudioFormat parseWavFormat(File file) throws AudioException {
        try (BufferedInputStream inputStream = new BufferedInputStream(getInputStream(file))) {
            return AudioWaveUtils.parseWavFormat(inputStream);
            throw new AudioException("Cannot parse wav stream", e);
    private static FileInputStream getInputStream(File file) throws AudioException {
            return new FileInputStream(file);
        } catch (FileNotFoundException e) {
            throw new AudioException("File '" + file.getAbsolutePath() + "' not found!");
        return audioFormat;
        int read = inputStream.read();
        alreadyRead++;
        inputStream.close();
        super.close();
        return this.length;
            inputStream = getInputStream(file);
            inputStream.skipNBytes(markedOffset);
            alreadyRead = markedOffset;
        } catch (AudioException e) {
            throw new IOException("Cannot reset file input stream: " + e.getMessage(), e);
        markedOffset = alreadyRead;
    public InputStream getClonedStream() throws AudioException {
        return getInputStream(file);
    public void dispose() throws IOException {
        if (isTemporaryFile) {
            Files.delete(file.toPath());
