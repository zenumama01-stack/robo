 * An exception when the token endpoint encounters an error and must return an error response, according to RFC 6749
 * Section 5.2.
 * @see <a href="https://datatracker.ietf.org/doc/html/rfc6749#section-5.2">RFC 6749: Issuing an Access Token - Error
 *      Response</a>
public class TokenEndpointException extends AuthenticationException {
    private static final long serialVersionUID = 610324537843397832L;
     * Represents the error types which are supported in token issuing error responses.
    public enum ErrorType {
        INVALID_REQUEST("invalid_request"),
        INVALID_GRANT("invalid_grant"),
        INVALID_CLIENT("invalid_client"),
        INVALID_SCOPE("invalid_scope"),
        UNAUTHORIZED_CLIENT("unauthorized_client"),
        UNSUPPORTED_GRANT_TYPE("unsupported_grant_type");
        private String error;
        ErrorType(String error) {
        public String getError() {
     * Constructs a {@link TokenEndpointException} for the specified error type.
     * @param errorType the error type
    public TokenEndpointException(ErrorType errorType) {
        super(errorType.getError());
     * Gets a {@link TokenResponseErrorDTO} representing the exception
     * @return the error response object
    public TokenResponseErrorDTO getErrorDTO() {
        return new TokenResponseErrorDTO(getMessage());
