import org.eclipse.jetty.http2.client.HTTP2Client;
import org.eclipse.jetty.util.ssl.SslContextFactory;
 * Factory class to create Jetty http clients
 * @author Martin van Wingerden - add createHttpClient without endpoint
 * @author Andrew Fiddian-Green - Added support for HTTP2 client creation
public interface HttpClientFactory {
     * Creates a new Jetty http client.
     * The returned client is not started yet. You have to start it yourself before using.
     * Don't forget to stop a started client again after its usage.
     * The client lifecycle should be the same as for your service.
     * DO NOT CREATE NEW CLIENTS FOR EACH REQUEST!
     * @param consumerName the for identifying the consumer in the Jetty thread pool.
     *            Must be between 4 and 20 characters long and must contain only the following characters [a-zA-Z0-9-_]
     * @return the Jetty client
     * @throws IllegalArgumentException if {@code consumerName} is invalid
    HttpClient createHttpClient(String consumerName);
     * @param sslContextFactory the SSL factory managing TLS encryption
    HttpClient createHttpClient(String consumerName, @Nullable SslContextFactory sslContextFactory);
     * Returns the shared Jetty http client. You must not call any setter methods or {@code stop()} on it.
     * The returned client is already started.
     * @return the shared Jetty http client
    HttpClient getCommonHttpClient();
     * Creates a new Jetty HTTP/2 client.
     * @param consumerName for identifying the consumer in the Jetty thread pool.
     * @return the Jetty HTTP/2 client
    HTTP2Client createHttp2Client(String consumerName);
    HTTP2Client createHttp2Client(String consumerName, @Nullable SslContextFactory sslContextFactory);
