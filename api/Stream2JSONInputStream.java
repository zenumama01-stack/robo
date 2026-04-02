import java.io.SequenceInputStream;
 * This {@link InputStream} will stream {@link Stream}s as JSON one item at a time. This will reduce memory usage when
 * streaming large collections through the REST interface. The input stream creates one JSON representation at a time
 * from the top level elements of the stream. For best performance a flattened stream should be provided. Otherwise a
 * nested collections JSON representation will be fully transformed into memory.
 * @author Jörg Sautter - Use as SequenceInputStream to simplify the logic
public class Stream2JSONInputStream extends InputStream implements JSONInputStream {
    private static final Gson GSON = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS).create();
    private final InputStream stream;
     * Creates a new {@link Stream2JSONInputStream} backed by the given {@link Stream} source.
     * @param source the {@link Stream} backing this input stream. Must not be null.
    public Stream2JSONInputStream(Stream<?> source) {
        Iterator<String> iterator = source.map(e -> GSON.toJson(e)).iterator();
        Enumeration<InputStream> enumeration = new Enumeration<>() {
            private boolean consumed = false;
            private @Nullable InputStream next = toStream("[");
            public boolean hasMoreElements() {
                return next != null || iterator.hasNext();
            public InputStream nextElement() {
                InputStream is;
                if (next != null) {
                    is = next;
                    if (!consumed && !iterator.hasNext()) {
                        next = toStream("]");
                        consumed = true;
                        next = null;
                    return is;
                is = toStream(iterator.next());
                if (iterator.hasNext()) {
                    next = toStream(",");
            private static InputStream toStream(String data) {
                return new ByteArrayInputStream(data.getBytes(StandardCharsets.UTF_8));
        stream = new SequenceInputStream(enumeration);
        return stream.read(b, off, len);
    public long transferTo(OutputStream target) throws IOException {
        return stream.transferTo(target);
