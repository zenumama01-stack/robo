 * A DTO object for an unsuccessful token endpoint response, as per RFC 6749, Section 5.2.
public class TokenResponseErrorDTO {
    public String error;
    public String error_description;
    public String error_uri;
     * Builds a token endpoint response for a specific error
     * @param the error
    public TokenResponseErrorDTO(String error) {
