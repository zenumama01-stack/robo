import static org.osgi.service.component.annotations.ServiceScope.PROTOTYPE;
import javax.ws.rs.InternalServerErrorException;
import org.osgi.service.jaxrs.whiteboard.propertytypes.JaxrsMediaType;
 * A media type extension for all supported media types.
@Component(scope = PROTOTYPE)
@JaxrsMediaType({ MediaType.APPLICATION_JSON, MediaType.TEXT_PLAIN })
public class MediaTypeExtension<T> implements MessageBodyReader<T>, MessageBodyWriter<T> {
    private static String mediaTypeWithoutParams(final MediaType mediaType) {
        return mediaType.getType() + "/" + mediaType.getSubtype();
    private final Map<String, MessageBodyReader<T>> readers = new HashMap<>();
    private final Map<String, MessageBodyWriter<T>> writers = new HashMap<>();
    public MediaTypeExtension() {
        final Gson gson = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_JSON_COMPAT).create();
        readers.put(mediaTypeWithoutParams(MediaType.APPLICATION_JSON_TYPE), new GsonMessageBodyReader<>(gson));
        readers.put(mediaTypeWithoutParams(MediaType.TEXT_PLAIN_TYPE), new PlainMessageBodyReader<>());
        writers.put(mediaTypeWithoutParams(MediaType.APPLICATION_JSON_TYPE), new GsonMessageBodyWriter<>(gson));
        writers.put(mediaTypeWithoutParams(MediaType.TEXT_PLAIN_TYPE), new PlainMessageBodyWriter<>());
        final MessageBodyWriter<T> writer = writers.get(mediaTypeWithoutParams(mediaType));
        return writer != null && writer.isWriteable(type, genericType, annotations, mediaType);
        if (writer != null) {
            writer.writeTo(object, type, genericType, annotations, mediaType, httpHeaders, entityStream);
            throw new InternalServerErrorException("unsupported media type");
        final MessageBodyReader<T> reader = readers.get(mediaTypeWithoutParams(mediaType));
        return reader != null && reader.isReadable(type, genericType, annotations, mediaType);
            return reader.readFrom(type, genericType, annotations, mediaType, httpHeaders, entityStream);
