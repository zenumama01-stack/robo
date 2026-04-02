 * This type can be used for all binary data such as images, documents, sounds etc.
 * Note that it is NOT adequate for any kind of streams, but only for fixed-size data.
 * @author Laurent Garnier - add MIME type
public class RawType implements PrimitiveType, State {
    public static final String DEFAULT_MIME_TYPE = "application/octet-stream";
    protected byte[] bytes;
    protected String mimeType;
    public RawType(byte[] bytes, String mimeType) {
        if (mimeType.isEmpty()) {
            throw new IllegalArgumentException("mimeType argument must not be blank");
        this.mimeType = mimeType;
    public String getMimeType() {
        return mimeType;
    public static RawType valueOf(String value) {
        int idx, idx2;
        if (value.isEmpty()) {
            throw new IllegalArgumentException("Argument must not be blank");
        } else if (!value.startsWith("data:") || ((idx = value.indexOf(",")) < 0)) {
            throw new IllegalArgumentException("Invalid data URI syntax for argument " + value);
        } else if ((idx2 = value.indexOf(";")) <= 5) {
            throw new IllegalArgumentException("Missing MIME type in argument " + value);
        return new RawType(Base64.getDecoder().decode(value.substring(idx + 1)), value.substring(5, idx2));
        return String.format("raw type (%s): %d bytes", mimeType, bytes.length);
        return String.format("data:%s;base64,%s", mimeType, Base64.getEncoder().encodeToString(bytes));
        RawType other = (RawType) obj;
        if (!mimeType.equals(other.mimeType)) {
