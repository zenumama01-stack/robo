 * Credentials which represent a user API token.
public class UserApiTokenCredentials implements Credentials {
    private final String userApiToken;
     * @param userApiToken the user API token
    public UserApiTokenCredentials(String userApiToken) {
        this.userApiToken = userApiToken;
     * Retrieves the user API token
     * @return the token
        return userApiToken;
