import org.mockito.InjectMocks;
import org.mockito.MockitoAnnotations;
 * The {@link AuthFilterTest} is a
public class AuthFilterTest {
    @InjectMocks
    private @NonNullByDefault({}) AuthFilter authFilter;
    // These mocks are inject into authFilter during setup
    public @Mock @NonNullByDefault({}) JwtHelper jwtHelperMock;
    public @Mock @NonNullByDefault({}) UserRegistry userRegistryMock;
    private @Mock @NonNullByDefault({}) ContainerRequestContext containerRequestContext;
    private @Mock @NonNullByDefault({}) HttpServletRequest servletRequest;
        MockitoAnnotations.openMocks(this);
        when(servletRequest.getRemoteAddr()).thenReturn("192.168.0.100");
    public void implicitUserRoleAllowsAccess() throws IOException {
        authFilter.activate(Map.of()); // implicit user role is true by default
        authFilter.filter(containerRequestContext);
        verify(containerRequestContext).setSecurityContext(any());
    public void noImplicitUserRoleDeniesAccess() throws IOException {
        authFilter.activate(Map.of(AuthFilter.CONFIG_IMPLICIT_USER_ROLE, false));
        verify(containerRequestContext, never()).setSecurityContext(any());
    public void trustedNetworkAllowsAccessIfForwardedHeaderMatches() throws IOException {
        authFilter.activate(Map.of(AuthFilter.CONFIG_IMPLICIT_USER_ROLE, false, AuthFilter.CONFIG_TRUSTED_NETWORKS,
                "192.168.1.0/24"));
        when(servletRequest.getHeader("x-forwarded-for")).thenReturn("192.168.1.100");
    public void trustedNetworkDeniesAccessIfForwardedHeaderDoesNotMatch() throws IOException {
        when(servletRequest.getHeader("x-forwarded-for")).thenReturn("192.168.2.100");
    public void trustedNetworkAllowsAccessIfRemoteAddressMatches() throws IOException {
                "192.168.0.0/24"));
    public void trustedNetworkDeniesAccessIfRemoteAddressDoesNotMatch() throws IOException {
