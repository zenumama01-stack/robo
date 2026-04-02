 * This {@link SecurityContext} contains information about a user, roles and authorizations granted to a client as
 * parsed from the contents of a JSON Web Token
public class JwtSecurityContext implements AuthenticationSecurityContext {
    Authentication authentication;
    public JwtSecurityContext(Authentication authentication) {
        this.authentication = authentication;
    public Principal getUserPrincipal() {
        return new GenericUser(authentication.getUsername());
        return authentication.getRoles().contains(role);
    public String getAuthenticationScheme() {
        return "JWT";
    public Authentication getAuthentication() {
