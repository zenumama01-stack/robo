 * Provides an exception class for openHAB to be used in case of connection issues with a device
public class ConnectionException extends CommunicationException {
    public ConnectionException(String message, @Nullable Object @Nullable... msgParams) {
    public ConnectionException(String message, @Nullable Throwable cause, @Nullable Object @Nullable... msgParams) {
    public ConnectionException(Throwable cause) {
