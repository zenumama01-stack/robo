 * An interface for a generic {@link Registry} of {@link User} entities. User registries can also be used as
 * {@link AuthenticationProvider}.
public interface UserRegistry extends Registry<User, String>, AuthenticationProvider {
     * Adds a new {@link User} in this registry. The implementation receives the clear text credentials and is
     * responsible for their secure storage (for instance by hashing the password), then return the newly created
     * {@link User} instance.
     * @param username the username of the new user
     * @param password the user password
     * @param roles the roles attributed to the new user
     * @return the new registered {@link User} instance
    User register(String username, String password, Set<String> roles);
     * Change the password for a {@link User} in this registry. The implementation receives the new password and is
     * responsible for their secure storage (for instance by hashing the password).
     * @param user the username of the existing user
     * @param newPassword the new password
    void changePassword(User user, String newPassword);
     * Adds a new session to the user profile
     * @param session the session to add
    void addUserSession(User user, UserSession session);
     * Removes the specified session from the user profile
     * @param session the session to remove
    void removeUserSession(User user, UserSession session);
     * Clears all sessions from the user profile
    void clearSessions(User user);
     * Adds a new API token to the user profile. The implementation is responsible for storing the token in a secure way
     * (for instance by hashing it).
     * @param name the name of the API token to create
     * @param scope the scope this API token will be valid for
     * @return the string that can be used as a Bearer token to match the new API token
    String addUserApiToken(User user, String name, String scope);
     * Removes the specified API token from the user profile
     * @param apiToken the API token
    void removeUserApiToken(User user, UserApiToken apiToken);
