package org.openhab.core.io.rest.core.internal;
import javax.ws.rs.WebApplicationException;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.ext.MessageBodyReader;
 * A message body reader for JSON using GSON.
public class GsonMessageBodyReader<T> implements MessageBodyReader<T> {
     * @param gson the GSON object to use
    public GsonMessageBodyReader(final Gson gson) {
        this.gson = gson;
    public boolean isReadable(final Class<?> type, final Type genericType, final Annotation[] annotations,
            final MediaType mediaType) {
    public T readFrom(final Class<T> type, final Type genericType, final Annotation[] annotations,
            final MediaType mediaType, final MultivaluedMap<String, String> httpHeaders, final InputStream entityStream)
            throws IOException, WebApplicationException {
        try (InputStreamReader reader = new InputStreamReader(entityStream, StandardCharsets.UTF_8)) {
            return gson.fromJson(reader, type);
