package org.openhab.core.audio.internal.utils;
import java.io.Closeable;
import org.openhab.core.audio.internal.AudioManagerTest;
 * Helper class to handle bundled resources.
 * @author Markus Rathgeb - Initial contribution
public class BundledSoundFileHandler implements Closeable {
    private static final String MP3_FILE_NAME = "mp3AudioFile.mp3";
    private static final String WAV_FILE_NAME = "wavAudioFile.wav";
    private final Logger logger = LoggerFactory.getLogger(BundledSoundFileHandler.class);
    private static void copy(final String resourcePath, final String filePath) throws IOException {
        try (InputStream is = AudioManagerTest.class.getResourceAsStream(resourcePath)) {
            byte[] buffer = new byte[is.available()];
            is.read(buffer);
            new File(filePath).getParentFile().mkdirs();
            try (OutputStream outStream = new FileOutputStream(filePath)) {
                outStream.write(buffer);
    private final Path tmpdir;
    private final String mp3FilePath;
    private final String wavFilePath;
    public BundledSoundFileHandler() throws IOException {
        tmpdir = Files.createTempDirectory(null);
        final Path configdir = tmpdir.resolve("configdir");
        System.setProperty(OpenHAB.CONFIG_DIR_PROG_ARGUMENT, configdir.toString());
        mp3FilePath = configdir.resolve("sounds/" + MP3_FILE_NAME).toString();
        copy("/configuration/sounds/mp3AudioFile.mp3", mp3FilePath);
        wavFilePath = configdir.resolve("sounds/" + WAV_FILE_NAME).toString();
        copy("/configuration/sounds/wavAudioFile.wav", wavFilePath);
        System.setProperty(OpenHAB.CONFIG_DIR_PROG_ARGUMENT, OpenHAB.DEFAULT_CONFIG_FOLDER);
        try (Stream<Path> files = Files.walk(tmpdir)) {
            files.sorted(Comparator.reverseOrder()).map(Path::toFile).forEach(File::delete);
        } catch (IOException ex) {
            logger.error("Exception while deleting files", ex);
    public String mp3FileName() {
        return MP3_FILE_NAME;
    public String mp3FilePath() {
        return mp3FilePath;
    public String wavFileName() {
        return WAV_FILE_NAME;
    public String wavFilePath() {
        return wavFilePath;
