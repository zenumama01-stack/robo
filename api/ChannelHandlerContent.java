 * The {@link ChannelHandlerContent} defines the pre-processed response
public class ChannelHandlerContent {
    private final byte[] rawContent;
    private final Charset encoding;
    private final @Nullable String mediaType;
    public ChannelHandlerContent(byte[] rawContent, String encoding, @Nullable String mediaType) {
        this.rawContent = rawContent;
        this.mediaType = mediaType;
        Charset finalEncoding = StandardCharsets.UTF_8;
            finalEncoding = Charset.forName(encoding);
        this.encoding = finalEncoding;
    public byte[] getRawContent() {
        return new String(rawContent, encoding);
    public @Nullable String getMediaType() {
        return mediaType;
