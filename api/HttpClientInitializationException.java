 * This exception is thrown, if an unexpected error occurs during initialization of the Jetty client
public class HttpClientInitializationException extends RuntimeException {
    private static final long serialVersionUID = -3187938868560212413L;
    public HttpClientInitializationException(String message, @Nullable Throwable cause) {
