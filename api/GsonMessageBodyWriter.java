import javax.ws.rs.core.StreamingOutput;
import javax.ws.rs.ext.MessageBodyWriter;
import org.openhab.core.io.rest.JSONInputStream;
 * A message body writer for JSON using GSON.
public class GsonMessageBodyWriter<T> implements MessageBodyWriter<T> {
    private final Logger logger = LoggerFactory.getLogger(GsonMessageBodyWriter.class);
    public GsonMessageBodyWriter(final Gson gson) {
    public long getSize(final T object, final Class<?> type, final Type genericType, final Annotation[] annotations,
    public boolean isWriteable(final Class<?> type, final Type genericType, final Annotation[] annotations,
    public void writeTo(final T object, final Class<?> type, final Type genericType, final Annotation[] annotations,
            final MediaType mediaType, final MultivaluedMap<String, Object> httpHeaders,
            final OutputStream entityStream) throws IOException, WebApplicationException {
        // Log a message if a response builder is received.
        if (object instanceof ResponseBuilder) {
                    "A REST endpoint returns a ResponseBuilder object. This is mostly wrong and the call to \"build()\" is missing. Please report or fix it. Got: {}",
                    object);
            if (object instanceof InputStream stream && object instanceof JSONInputStream) {
                stream.transferTo(entityStream);
            } else if (object instanceof StreamingOutput streaming) {
                streaming.write(entityStream);
                entityStream.write(gson.toJson(object).getBytes(StandardCharsets.UTF_8));
            // Flush the stream.
            // Keep this code as it has been present before,
            // but I don't think this needs to be done in the message body writer itself.
            entityStream.flush();
            // we catch this exception to avoid confusion errors in the log file, since this is not any error situation
            // see https://github.com/openhab/openhab-distro/issues/1188
            logger.debug("Failed writing HTTP response, since other side closed the connection", e);
