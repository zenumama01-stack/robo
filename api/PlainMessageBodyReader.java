 * A message body reader for plain text.
public class PlainMessageBodyReader<T> implements MessageBodyReader<T> {
    private final Logger logger = LoggerFactory.getLogger(PlainMessageBodyReader.class);
        final byte[] data = data(entityStream);
        logger.debug("Received: type={}, genericType={}, annotations={}, mediaType={}, httpHeaders={}", type,
                genericType, annotations, mediaType, httpHeaders);
        if (type.equals(String.class) || genericType.equals(String.class)) {
            return (T) new String(data, StandardCharsets.UTF_8);
        } else if (type.equals(byte[].class) || genericType.equals(byte[].class)) {
            return (T) data;
        } else if (type.equals(Byte[].class) || genericType.equals(Byte[].class)) {
            final Byte[] dataB = new Byte[data.length];
            for (int i = 0; i < data.length; ++i) {
                dataB[i] = Byte.valueOf(data[i]);
            return (T) dataB;
            throw new InternalServerErrorException(
                    String.format("Cannot assign text plain to type \"%s\", generic type: \"%s\".", type, genericType));
    private static byte[] data(final InputStream is) throws IOException {
        final ByteArrayOutputStream outputBytes = new ByteArrayOutputStream();
        int read;
        final byte[] buffer = new byte[1024];
        while ((read = is.read(buffer)) != -1) {
            outputBytes.write(buffer, 0, read);
        return outputBytes.toByteArray();
