 * A dedicated exception thrown when extracted credentials can not be matched with any authentication provider.
 * This can usually happen when configuration is somewhat wrong. In order to make debugging easier a separate exception
 * is created.
public class UnsupportedCredentialsException extends AuthenticationException {
    public UnsupportedCredentialsException(String message) {
    public UnsupportedCredentialsException(Throwable cause) {
    public UnsupportedCredentialsException(String message, Throwable cause) {
