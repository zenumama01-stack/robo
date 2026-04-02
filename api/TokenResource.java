import javax.ws.rs.CookieParam;
import javax.ws.rs.FormParam;
import javax.ws.rs.core.Cookie;
import javax.ws.rs.core.Response.ResponseBuilder;
import org.jose4j.base64url.Base64Url;
import org.openhab.core.auth.UserSession;
import org.openhab.core.io.rest.auth.internal.TokenEndpointException.ErrorType;
 * This class is used to issue JWT tokens to clients.
 * @author Wouter Born - Migrated to JAX-RS Whiteboard Specification
 * @author Yannick Schaus - Add API token operations
@Component(service = { RESTResource.class, TokenResource.class })
@JaxrsName(TokenResource.PATH_AUTH)
@Path(TokenResource.PATH_AUTH)
@Tag(name = TokenResource.PATH_AUTH)
public class TokenResource implements RESTResource {
    private final Logger logger = LoggerFactory.getLogger(TokenResource.class);
    public static final String PATH_AUTH = "auth";
    /** The name of the HTTP-only cookie holding the session ID */
    public static final String SESSIONID_COOKIE_NAME = "X-OPENHAB-SESSIONID";
    private static final String SESSIONID_COOKIE_FORMAT = SESSIONID_COOKIE_NAME
            + "=%s; Domain=%s; Path=/; Max-Age=2147483647; HttpOnly; SameSite=Strict";
    /** The default lifetime of tokens in minutes before they expire */
    public static final int TOKEN_LIFETIME = 60;
    public TokenResource(final @Reference UserRegistry userRegistry, final @Reference JwtHelper jwtHelper) {
    @Path("/token")
    @Produces({ MediaType.APPLICATION_JSON })
    @Consumes({ MediaType.APPLICATION_FORM_URLENCODED })
    @Operation(operationId = "getOAuthToken", summary = "Get access and refresh tokens.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = TokenResponseDTO.class))),
            @ApiResponse(responseCode = "400", description = "Invalid request parameters") })
    public Response getToken(@FormParam("grant_type") String grantType, @FormParam("code") String code,
            @FormParam("redirect_uri") String redirectUri, @FormParam("client_id") String clientId,
            @FormParam("refresh_token") String refreshToken, @FormParam("code_verifier") String codeVerifier,
            @QueryParam("useCookie") boolean useCookie,
            @Nullable @CookieParam(SESSIONID_COOKIE_NAME) Cookie sessionCookie) {
            switch (grantType) {
                case "authorization_code":
                    return processAuthorizationCodeGrant(code, redirectUri, clientId, codeVerifier, useCookie);
                case "refresh_token":
                    return processRefreshTokenGrant(clientId, refreshToken, sessionCookie);
                    throw new TokenEndpointException(ErrorType.UNSUPPORTED_GRANT_TYPE);
        } catch (TokenEndpointException e) {
            logger.warn("Token issuing failed: {}", e.getMessage());
            return Response.status(Status.BAD_REQUEST).entity(e.getErrorDTO()).build();
            logger.error("Error while authenticating", e);
            return Response.status(Status.BAD_REQUEST).build();
    @Path("/sessions")
    @Operation(operationId = "getSessionsForCurrentUser", summary = "List the sessions associated to the authenticated user.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = UserSessionDTO.class)))),
            @ApiResponse(responseCode = "400", description = "User authentication is not managed by openHAB"),
            @ApiResponse(responseCode = "401", description = "User is not authenticated"),
            @ApiResponse(responseCode = "404", description = "User not found") })
    public Response getSessions(@Context SecurityContext securityContext) {
        if (securityContext.getUserPrincipal() == null) {
            return JSONResponse.createErrorResponse(Status.UNAUTHORIZED, "User is not authenticated");
        User user = userRegistry.get(securityContext.getUserPrincipal().getName());
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "User not found");
        if (!(user instanceof AuthenticatedUser authenticatedUser)) {
                    "User authentication is not managed by openHAB");
        Stream<UserSessionDTO> sessions = authenticatedUser.getSessions().stream().map(this::toUserSessionDTO);
        return Response.ok(new Stream2JSONInputStream(sessions)).build();
    @Path("/apitokens")
    @Operation(operationId = "getApiTokens", summary = "List the API tokens associated to the authenticated user.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = UserApiTokenDTO.class)))),
    public Response getApiTokens(@Context SecurityContext securityContext) {
        Stream<UserApiTokenDTO> sessions = authenticatedUser.getApiTokens().stream().map(this::toUserApiTokenDTO);
    @Path("/apitokens/{name}")
    @Operation(operationId = "removeApiToken", summary = "Revoke a specified API token associated to the authenticated user.", responses = {
            @ApiResponse(responseCode = "404", description = "User or API token not found") })
    public Response removeApiToken(@Context SecurityContext securityContext, @PathParam("name") String name) {
        Optional<UserApiToken> userApiToken = authenticatedUser.getApiTokens().stream()
                .filter(apiToken -> apiToken.getName().equals(name)).findAny();
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "No API token found with that name");
    @Path("/logout")
    @Operation(operationId = "deleteSession", summary = "Delete the session associated with a refresh token.", responses = {
            @ApiResponse(responseCode = "404", description = "User or refresh token not found") })
    public Response deleteSession(@Nullable @FormParam("refresh_token") String refreshToken,
            @Nullable @FormParam("id") String id, @Nullable @CookieParam(SESSIONID_COOKIE_NAME) Cookie sessionCookie,
            @Context SecurityContext securityContext) {
        Optional<UserSession> session;
        if (refreshToken != null) {
            session = authenticatedUser.getSessions().stream().filter(s -> s.getRefreshToken().equals(refreshToken))
                    .findAny();
        } else if (id != null) {
            session = authenticatedUser.getSessions().stream().filter(s -> s.getSessionId().startsWith(id + "-"))
            throw new IllegalArgumentException("no refresh_token or id specified");
        if (session.isEmpty()) {
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Session not found");
        ResponseBuilder response = Response.ok();
        if (sessionCookie != null && sessionCookie.getValue().equals(session.get().getSessionId())) {
                URI domainUri = new URI(session.get().getRedirectUri());
                // workaround to set the SameSite cookie attribute until we upgrade to
                // jakarta.ws.rs/jakarta.ws.rs-api/3.1.0 or newer
                response.header("Set-Cookie",
                        SESSIONID_COOKIE_FORMAT.formatted(UUID.randomUUID(), domainUri.getHost()));
                logger.error("Unexpected error deleting session", e);
        userRegistry.removeUserSession(user, session.get());
        return response.build();
    private UserSessionDTO toUserSessionDTO(UserSession session) {
        // we only divulge the prefix of the session ID to the client (otherwise an XSS attacker may find the
        // session ID for a stolen refresh token easily by using the sessions endpoint).
        return new UserSessionDTO(session.getSessionId().split("-")[0], session.getCreatedTime(),
                session.getLastRefreshTime(), session.getClientId(), session.getScope());
    private UserApiTokenDTO toUserApiTokenDTO(UserApiToken apiToken) {
        return new UserApiTokenDTO(apiToken.getName(), apiToken.getCreatedTime(), apiToken.getScope());
    private Response processAuthorizationCodeGrant(String code, String redirectUri, String clientId,
            @Nullable String codeVerifier, boolean useCookie) throws TokenEndpointException, NoSuchAlgorithmException {
        // find a user with the authorization code pending
        Optional<User> optionalUser = userRegistry.getAll().stream().filter(u -> {
            if (!(u instanceof AuthenticatedUser authenticatedUser)) {
            PendingToken pendingToken = authenticatedUser.getPendingToken();
            return (pendingToken != null && pendingToken.getAuthorizationCode().equals(code));
        }).findAny();
        if (optionalUser.isEmpty()) {
            logger.warn("Couldn't find a user with the provided authentication code pending");
            throw new TokenEndpointException(ErrorType.INVALID_GRANT);
        User user = optionalUser.get();
            logger.warn("User '{}' authentication is not managed by openHAB", user.getName());
            throw new TokenEndpointException(ErrorType.INVALID_CLIENT);
        if (pendingToken == null) {
        if (!pendingToken.getClientId().equals(clientId)) {
            logger.warn("client_id '{}' doesn't match pending token information '{}'", clientId,
                    pendingToken.getClientId());
        if (!pendingToken.getRedirectUri().equals(redirectUri)) {
            logger.warn("redirect_uri '{}' doesn't match pending token information '{}'", redirectUri,
                    pendingToken.getRedirectUri());
        // create a new session ID and refresh token
        String sessionId = UUID.randomUUID().toString();
        String newRefreshToken = UUID.randomUUID().toString().replace("-", "");
        String scope = pendingToken.getScope();
        // if there is PKCE information in the pending token, check that first
        String codeChallengeMethod = pendingToken.getCodeChallengeMethod();
        if (codeChallengeMethod != null) {
            String codeChallenge = pendingToken.getCodeChallenge();
            if (codeChallenge == null || codeVerifier == null) {
                logger.warn("the PKCE code challenge or code verifier information is missing");
            switch (codeChallengeMethod) {
                case "plain":
                    if (!codeVerifier.equals(codeChallenge)) {
                        logger.warn("PKCE verification failed");
                case "S256":
                    MessageDigest sha256Digest = MessageDigest.getInstance("SHA-256");
                    String computedCodeChallenge = Base64Url.encode(sha256Digest.digest(codeVerifier.getBytes()));
                    if (!computedCodeChallenge.equals(codeChallenge)) {
                    logger.warn("PKCE transformation algorithm '{}' not supported", codeChallengeMethod);
                    throw new TokenEndpointException(ErrorType.INVALID_REQUEST);
        // create an access token
        String accessToken = jwtHelper.getJwtAccessToken(authenticatedUser, clientId, scope, TOKEN_LIFETIME);
        UserSession newSession = new UserSession(sessionId, newRefreshToken, clientId, redirectUri, scope);
        ResponseBuilder response = Response.ok(new TokenResponseDTO(accessToken, "bearer", TOKEN_LIFETIME * 60,
                newRefreshToken, scope, authenticatedUser));
        // if the client has requested an http-only cookie for the session, set it
        if (useCookie) {
                // this feature is only available for root redirect URIs: the targeted client is the main
                // UI; even though the cookie will be set for the entire domain (i.e. no path) so that
                // other servlets can make use of it
                URI domainUri = new URI(redirectUri);
                if (!("".equals(domainUri.getPath()) || "/".equals(domainUri.getPath()))) {
                            "Will not honor the request to set a session cookie for this client, because it's only allowed for root redirect URIs");
                response.header("Set-Cookie", SESSIONID_COOKIE_FORMAT.formatted(sessionId, domainUri.getHost()));
                // also mark the session as supported by a cookie
                newSession.setSessionCookie(true);
                logger.warn("Error while setting a session cookie: {}", e.getMessage());
                throw new TokenEndpointException(ErrorType.UNAUTHORIZED_CLIENT);
        // add the new session to the user profile and clear the pending token information
        authenticatedUser.getSessions().add(newSession);
        authenticatedUser.setPendingToken(null);
    private Response processRefreshTokenGrant(String clientId, @Nullable String refreshToken,
            @Nullable Cookie sessionCookie) throws TokenEndpointException {
        if (refreshToken == null) {
        // find a user associated with the provided refresh token
        Optional<User> optionalRefreshTokenUser = userRegistry.getAll().stream()
                .filter(u -> u instanceof AuthenticatedUser).filter(u -> ((AuthenticatedUser) u).getSessions().stream()
                        .anyMatch(s -> refreshToken.equals(s.getRefreshToken())))
        if (optionalRefreshTokenUser.isEmpty()) {
            logger.warn("Couldn't find a user with a session matching the provided refresh_token");
        // get the session from the refresh token
        User refreshTokenUser = optionalRefreshTokenUser.get();
        if (!(refreshTokenUser instanceof AuthenticatedUser refreshTokenAuthenticatedUser)) {
            logger.warn("User '{}' authentication is not managed by openHAB", refreshTokenUser.getName());
        Optional<UserSession> optSession = refreshTokenAuthenticatedUser.getSessions().stream()
                .filter(s -> s.getRefreshToken().equals(refreshToken)).findAny();
        if (optSession.isEmpty()) {
            logger.warn("Not refreshing token for user {}, missing session", refreshTokenAuthenticatedUser.getName());
        UserSession session = optSession.get();
        // if the cookie flag is present on the session, verify that the cookie is present and corresponds
        // to this session
        if (session.hasSessionCookie()
                && (sessionCookie == null || !sessionCookie.getValue().equals(session.getSessionId()))) {
            logger.warn("Not refreshing token for session {} of user {}, missing or invalid session cookie",
                    session.getSessionId(), refreshTokenAuthenticatedUser.getName());
        // issue a new access token
        String refreshedAccessToken = jwtHelper.getJwtAccessToken(refreshTokenAuthenticatedUser, clientId,
                session.getScope(), TOKEN_LIFETIME);
        logger.debug("Refreshing session {} of user {}", session.getSessionId(),
                refreshTokenAuthenticatedUser.getName());
        ResponseBuilder refreshResponse = Response.ok(new TokenResponseDTO(refreshedAccessToken, "bearer",
                TOKEN_LIFETIME * 60, refreshToken, session.getScope(), refreshTokenAuthenticatedUser));
        // update the last refresh time of the session in the user's profile
        session.setLastRefreshTime(new Date());
        userRegistry.update(refreshTokenAuthenticatedUser);
        return refreshResponse.build();
