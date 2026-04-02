import java.net.Socket;
import java.util.NoSuchElementException;
import java.util.Scanner;
 * This is an AudioStream from a URL. Note that some sinks, like Sonos, can directly handle URL
 * based streams, and therefore can/should call getURL() to get a direct reference to the URL.
 * @author Kai Kreuzer - Refactored to not require a source
public class URLAudioStream extends AudioStream implements ClonableAudioStream {
    private static final Pattern PLS_STREAM_PATTERN = Pattern.compile("^File[0-9]=(.+)$");
    public static final String M3U_EXTENSION = "m3u";
    public static final String PLS_EXTENSION = "pls";
    private final Logger logger = LoggerFactory.getLogger(URLAudioStream.class);
    private final InputStream inputStream;
    private String url;
    private @Nullable Socket shoutCastSocket;
    public URLAudioStream(String url) throws AudioException {
        this.url = url;
        this.audioFormat = new AudioFormat(AudioFormat.CONTAINER_NONE, AudioFormat.CODEC_MP3, false, 16, null, null);
        this.inputStream = createInputStream();
    private InputStream createInputStream() throws AudioException {
        final String filename = url.toLowerCase();
            URL streamUrl = new URI(url).toURL();
                case M3U_EXTENSION:
                    try (Scanner scanner = new Scanner(streamUrl.openStream(), StandardCharsets.UTF_8.name())) {
                            String line = scanner.nextLine();
                            if (!line.isEmpty() && !line.startsWith("#")) {
                                url = line;
                    } catch (NoSuchElementException e) {
                        // we reached the end of the file, this exception is thus expected
                case PLS_EXTENSION:
                            if (!line.isEmpty() && line.startsWith("File")) {
                                final Matcher matcher = PLS_STREAM_PATTERN.matcher(line);
                                if (matcher.find()) {
                                    url = matcher.group(1);
            streamUrl = new URI(url).toURL();
            URLConnection connection = streamUrl.openConnection();
            if ("unknown/unknown".equals(connection.getContentType())) {
                // Java does not parse non-standard headers used by SHOUTCast
                int port = streamUrl.getPort() > 0 ? streamUrl.getPort() : 80;
                // Manipulate User-Agent to receive a stream
                Socket socket = new Socket(streamUrl.getHost(), port);
                shoutCastSocket = socket;
                OutputStream os = socket.getOutputStream();
                String userAgent = "WinampMPEG/5.09";
                String req = "GET / HTTP/1.0\r\nuser-agent: " + userAgent
                        + "\r\nIcy-MetaData: 1\r\nConnection: keep-alive\r\n\r\n";
                os.write(req.getBytes());
                return socket.getInputStream();
                // getInputStream() method is more error-proof than openStream(),
                // because openStream() does openConnection().getInputStream(),
                // which opens a new connection and does not reuse the old one.
                return connection.getInputStream();
        } catch (MalformedURLException | URISyntaxException e) {
            logger.error("URL '{}' is not a valid url: {}", url, e.getMessage(), e);
            throw new AudioException("URL not valid");
            logger.error("Cannot set up stream '{}': {}", url, e.getMessage(), e);
            throw new AudioException("IO Error");
        return inputStream.read();
    public String getURL() {
        if (shoutCastSocket instanceof Socket socket) {
            socket.close();
        return new URLAudioStream(url);
