 * Provides an exception class for openHAB to be used in case of communication issues with a device
public class CommunicationException extends AbstractI18nException {
    public CommunicationException(String message, @Nullable Object @Nullable... msgParams) {
        super(message, msgParams);
    public CommunicationException(String message, @Nullable Throwable cause, @Nullable Object @Nullable... msgParams) {
        super(message, cause, msgParams);
    public CommunicationException(Throwable cause) {
