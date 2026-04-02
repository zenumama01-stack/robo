 * Base type for exceptions thrown by authentication layer.
 * @author Kai Kreuzer - Added JavaDoc and serial id
public class AuthenticationException extends SecurityException {
    private static final long serialVersionUID = 8063538216812770858L;
     * Creates a new exception instance.
     * @param message exception message
    public AuthenticationException(String message) {
     * @param cause exception cause
    public AuthenticationException(Throwable cause) {
    public AuthenticationException(String message, Throwable cause) {
