 * A DTO object for a successful token endpoint response, as per RFC 6749, Section 5.1.
 * @see <a href="https://datatracker.ietf.org/doc/html/rfc6749#section-5.1">RFC 6749: Issuing an Access Token -
 *      Successful Response</a>
@Schema(name = "TokenResponse")
public class TokenResponseDTO {
    public String access_token;
    public String token_type;
    public Integer expires_in;
    public String refresh_token;
    public String scope;
    public UserDTO user;
     * Builds a successful response containing token information.
     * @param access_token the access token
     * @param token_type the type of the token, normally "bearer"
     * @param expires_in the expiration time of the access token in seconds
     * @param refresh_token the refresh token which can be used to get additional tokens
     * @param scope the request scope
     * @param user the user object, an additional parameter not part of the specification
    public TokenResponseDTO(String access_token, String token_type, Integer expires_in, String refresh_token,
            String scope, User user) {
        this.access_token = access_token;
        this.token_type = token_type;
        this.expires_in = expires_in;
        this.refresh_token = refresh_token;
        this.user = new UserDTO(user);
