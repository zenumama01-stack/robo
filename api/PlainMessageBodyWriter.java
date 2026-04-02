 * A message body writer for plain text.
public class PlainMessageBodyWriter<T> implements MessageBodyWriter<T> {
        entityStream.write(object.toString().getBytes(StandardCharsets.UTF_8));
