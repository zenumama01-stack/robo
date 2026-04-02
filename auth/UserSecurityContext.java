 * This {@link SecurityContext} contains information about a user, roles and authorizations granted to a client
 * from a {@link User} instance.
public class UserSecurityContext implements AuthenticationSecurityContext {
    private User user;
    private Authentication authentication;
    private String authenticationScheme;
     * Constructs a security context from an instance of {@link User}
     * @param user the user
     * @param authentication the related {@link Authentication}
     * @param authenticationScheme the scheme that was used to authenticate the user, e.g. "Basic"
    public UserSecurityContext(User user, Authentication authentication, String authenticationScheme) {
        this.user = user;
        this.authenticationScheme = authenticationScheme;
        return user.getRoles().contains(role);
        return authenticationScheme;
