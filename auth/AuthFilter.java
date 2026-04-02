import javax.annotation.Priority;
import javax.ws.rs.Priorities;
import javax.ws.rs.container.ContainerRequestContext;
import javax.ws.rs.container.ContainerRequestFilter;
import javax.ws.rs.container.PreMatching;
import javax.ws.rs.ext.Provider;
import org.openhab.core.auth.UserApiTokenCredentials;
import org.openhab.core.io.rest.auth.internal.ExpiringUserSecurityContextCache;
import org.openhab.core.io.rest.auth.internal.JwtHelper;
import org.openhab.core.io.rest.auth.internal.JwtSecurityContext;
import org.openhab.core.io.rest.auth.internal.UserSecurityContext;
import org.osgi.service.jaxrs.whiteboard.propertytypes.JaxrsExtension;
 * This filter is responsible for parsing credentials provided with a request, and hydrating a {@link SecurityContext}
 * from these credentials if they are valid.
 * @author Yannick Schaus - Allow basic authentication
 * @author Yannick Schaus - Add support for API tokens
 * @author Sebastian Gerber - Add basic auth caching
 * @author Kai Kreuzer - Add null annotations, constructor initialization
 * @author Miguel Álvarez - Add trusted networks for implicit user role
@PreMatching
@Component(configurationPid = "org.openhab.restauth", property = Constants.SERVICE_PID
        + "=org.openhab.restauth", service = { ContainerRequestFilter.class, AuthFilter.class })
@ConfigurableService(category = "system", label = "API Security", description_uri = AuthFilter.CONFIG_URI)
@JaxrsExtension
@Priority(Priorities.AUTHENTICATION)
@Provider
public class AuthFilter implements ContainerRequestFilter {
    private final Logger logger = LoggerFactory.getLogger(AuthFilter.class);
    static final String ALT_AUTH_HEADER = "X-OPENHAB-TOKEN";
    static final String API_TOKEN_PREFIX = "oh.";
    protected static final String CONFIG_URI = "system:restauth";
    static final String CONFIG_ALLOW_BASIC_AUTH = "allowBasicAuth";
    static final String CONFIG_IMPLICIT_USER_ROLE = "implicitUserRole";
    static final String CONFIG_TRUSTED_NETWORKS = "trustedNetworks";
    static final String CONFIG_CACHE_EXPIRATION = "cacheExpiration";
    private boolean allowBasicAuth = false;
    private boolean implicitUserRole = true;
    private List<CIDR> trustedNetworks = List.of();
    private Long cacheExpiration = 6L;
    private ExpiringUserSecurityContextCache authCache = new ExpiringUserSecurityContextCache(
            Duration.ofHours(cacheExpiration).toMillis());
    private static final byte[] RANDOM_BYTES = new byte[32];
    private final JwtHelper jwtHelper;
    @Context
    private @NonNullByDefault({}) HttpServletRequest servletRequest;
    private RegistryChangeListener<User> userRegistryListener = new RegistryChangeListener<>() {
        public void added(User element) {
        public void removed(User element) {
            authCache.clear();
        public void updated(User oldElement, User element) {
    public AuthFilter(@Reference JwtHelper jwtHelper, @Reference UserRegistry userRegistry) {
        this.jwtHelper = jwtHelper;
        new Random().nextBytes(RANDOM_BYTES);
        userRegistry.addRegistryChangeListener(userRegistryListener);
            allowBasicAuth = ConfigParser.valueAsOrElse(properties.get(CONFIG_ALLOW_BASIC_AUTH), Boolean.class, false);
            implicitUserRole = ConfigParser.valueAsOrElse(properties.get(CONFIG_IMPLICIT_USER_ROLE), Boolean.class,
            trustedNetworks = parseTrustedNetworks(
                    ConfigParser.valueAsOrElse(properties.get(CONFIG_TRUSTED_NETWORKS), String.class, ""));
                cacheExpiration = ConfigParser.valueAsOrElse(properties.get(CONFIG_CACHE_EXPIRATION), Long.class, 6L);
                logger.warn("Ignoring invalid configuration value '{}' for cacheExpiration parameter.",
                        properties.get(CONFIG_CACHE_EXPIRATION));
        userRegistry.removeRegistryChangeListener(userRegistryListener);
    private @Nullable String getCacheKey(String credentials) {
        if (cacheExpiration == 0) {
            // caching is disabled
            final MessageDigest md = MessageDigest.getInstance("SHA-256");
            md.update(RANDOM_BYTES);
            return new String(md.digest(credentials.getBytes()));
            // SHA-256 is available for all java distributions so this code will actually never run
            // If it does we'll just flood the cache with random values
            logger.warn("SHA-256 is not available. Cache for basic auth disabled!");
    private SecurityContext authenticateBearerToken(String token) throws AuthenticationException {
        if (token.startsWith(API_TOKEN_PREFIX)) {
            UserApiTokenCredentials credentials = new UserApiTokenCredentials(token);
            Authentication auth = userRegistry.authenticate(credentials);
                throw new AuthenticationException("User not found in registry");
            return new UserSecurityContext(user, auth, "ApiToken");
            Authentication auth = jwtHelper.verifyAndParseJwtAccessToken(token);
            return new JwtSecurityContext(auth);
    private SecurityContext authenticateBasicAuth(String credentialString) throws AuthenticationException {
        final String cacheKey = getCacheKey(credentialString);
        if (cacheKey != null) {
            final UserSecurityContext cachedValue = authCache.get(cacheKey);
            if (cachedValue != null) {
                return cachedValue;
        String[] decodedCredentials = new String(Base64.getDecoder().decode(credentialString), StandardCharsets.UTF_8)
                .split(":");
        if (decodedCredentials.length != 2) {
            throw new AuthenticationException("Invalid Basic authentication credential format");
        UsernamePasswordCredentials credentials = new UsernamePasswordCredentials(decodedCredentials[0],
                decodedCredentials[1]);
        UserSecurityContext context = new UserSecurityContext(user, auth, "Basic");
            authCache.put(cacheKey, context);
    public void filter(@Nullable ContainerRequestContext requestContext) throws IOException {
        if (requestContext != null) {
                SecurityContext sc = getSecurityContext(servletRequest, false);
                if (sc != null) {
                    requestContext.setSecurityContext(sc);
                logger.warn("Unauthorized API request from {}: {}", getClientIp(servletRequest), e.getMessage());
                requestContext.abortWith(JSONResponse.createErrorResponse(Status.UNAUTHORIZED, "Invalid credentials"));
    public @Nullable SecurityContext getSecurityContext(@Nullable String bearerToken) throws AuthenticationException {
        if (bearerToken == null) {
        return authenticateBearerToken(bearerToken);
    public @Nullable SecurityContext getSecurityContext(HttpServletRequest request, boolean allowQueryToken)
            throws AuthenticationException, IOException {
        String altTokenHeader = request.getHeader(ALT_AUTH_HEADER);
        if (altTokenHeader != null) {
            return authenticateBearerToken(altTokenHeader);
        String authHeader = request.getHeader(HttpHeaders.AUTHORIZATION);
        String authType = null;
        String authValue = null;
        boolean authFromQuery = false;
        if (authHeader != null) {
            String[] authParts = authHeader.split(" ");
            if (authParts.length == 2) {
                authType = authParts[0];
                authValue = authParts[1];
        } else if (allowQueryToken) {
            Map<String, String[]> parameterMap = request.getParameterMap();
            String[] accessToken = parameterMap.get("accessToken");
            if (accessToken != null && accessToken.length > 0) {
                authValue = accessToken[0];
                authFromQuery = true;
        if (authValue != null) {
            if (authFromQuery) {
                    return authenticateBearerToken(authValue);
                    if (allowBasicAuth) {
                        return authenticateBasicAuth(authValue);
            } else if ("Bearer".equalsIgnoreCase(authType)) {
            } else if ("Basic".equalsIgnoreCase(authType)) {
                String[] decodedCredentials = new String(Base64.getDecoder().decode(authValue), "UTF-8").split(":");
                if (decodedCredentials.length > 2) {
                switch (decodedCredentials.length) {
                        return authenticateBearerToken(decodedCredentials[0]);
                        if (!allowBasicAuth) {
                            throw new AuthenticationException(
                                    "Basic authentication with username/password is not allowed");
        } else if (isImplicitUserRole(request)) {
            return new AnonymousUserSecurityContext();
    private boolean isImplicitUserRole(HttpServletRequest request) {
        if (implicitUserRole) {
            byte[] clientAddress = InetAddress.getByName(getClientIp(request)).getAddress();
            return trustedNetworks.stream().anyMatch(networkCIDR -> networkCIDR.isInRange(clientAddress));
            logger.debug("Error validating trusted networks: {}", e.getMessage());
    private List<CIDR> parseTrustedNetworks(String value) {
        var cidrList = new ArrayList<CIDR>();
        for (var cidrString : value.split(",")) {
                if (!cidrString.isBlank()) {
                    cidrList.add(new CIDR(cidrString.trim()));
                logger.warn("Error parsing trusted network cidr: {}", cidrString);
        return cidrList;
    private String getClientIp(HttpServletRequest request) throws UnknownHostException {
        String ipForwarded = Objects.requireNonNullElse(request.getHeader("x-forwarded-for"), "");
        String clientIp = ipForwarded.split(",")[0];
        return clientIp.isBlank() ? request.getRemoteAddr() : clientIp;
    private static class CIDR {
        private static final Pattern CIDR_PATTERN = Pattern.compile("(?<networkAddress>.*?)/(?<prefixLength>\\d+)");
        private final byte[] networkBytes;
        private final int prefix;
        public CIDR(String cidr) throws UnknownHostException {
            Matcher m = CIDR_PATTERN.matcher(cidr);
            if (!m.matches()) {
                throw new UnknownHostException();
            this.prefix = Integer.parseInt(m.group("prefixLength"));
            this.networkBytes = InetAddress.getByName(m.group("networkAddress")).getAddress();
        public boolean isInRange(byte[] address) {
            if (networkBytes.length != address.length) {
            int p = this.prefix;
            while (p > 8) {
                if (networkBytes[i] != address[i]) {
                p -= 8;
            final int m = (65280 >> p) & 255;
            return (networkBytes[i] & m) == (address[i] & m);
