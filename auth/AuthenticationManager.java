 * Authentication manager is main entry point for all places which are interested in securing requests and verifying
 * their originator.
public interface AuthenticationManager {
     * Authentication attempt with specified credentials.
     * @param credentials Credentials to authenticate with.
     * @return Null value should never be returned. Any failed authentication (for whatever reason), should cause
     *         AuthenticationException.
     * @throws AuthenticationException when none of available authentication methods succeeded.
    Authentication authenticate(Credentials credentials) throws AuthenticationException;
