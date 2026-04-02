 * Realizations of this type are responsible for checking validity of various credentials and giving back authentication
 * which defines access scope for authenticated user or system.
public interface AuthenticationProvider {
     * Verify given credentials and give back authentication if they are valid.
     * @param credentials User credentials.
     * @return null if credentials were not valid for this provider
     * @throws AuthenticationException if authentication failed due to credentials mismatch.
     * Additional method to verify if given authentication provider can handle given type of credentials.
     * @param type Type of credentials.
     * @return True if credentials of given type can be used for authentication attempt with provider.
    boolean supports(Class<? extends Credentials> type);
