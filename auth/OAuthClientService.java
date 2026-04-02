 * This is the service factory to produce an OAuth2 service client that authenticates using OAUTH2.
 * This is a service factory pattern; the OAuth2 service client is not shared between bundles.
 * The basic uses of this OAuthClient are as follows:
 * Use case 1 - For the full authorization code grant flow, as described in RFC 6749 section 4.1
 * https://tools.ietf.org/html/rfc6749#section-4.1
 * <li>Method {@code #getAuthorizationUrl(String, String, String)} to get an authorization code url
 * <li>Redirect the user-agent/ real user (outside scope of this client)
 * <li>Method {@code #extractAuthCodeFromAuthResponse(String)} to verify and extract the authorization
 * code from the response
 * <li>Method {@code #getAccessTokenResponseByAuthorizationCode(String, String)} to get an access token (may contain
 * optional refresh token) by authorization code extracted in above step.
 * <li>Use the {@code AccessTokenResponse} in code
 * <li>When access token is expired, see Use case 3 - refresh token.
 * Use case 2 - For Resource Owner Password Credentials Grant, as described in RFC 6749 section 4.3
 * https://tools.ietf.org/html/rfc6749#section-4.3
 * <li>Method {@code #getAccessTokenByResourceOwnerPasswordCredentials(String, String, String)} to get
 * {@code AccessTokenResponse} (may contain optional refresh token) by username and password
 * <li>When access token is expired, Use {@code #refreshToken()} to get another access token
 * Use case 3 - Refresh token
 * <li>Method {@code #refreshToken}
 * Use case 4 - Client Credentials. This is used to get the AccessToken by purely the client credential.
 * <li>Method {@code #getAccessTokenByClientCredentials(String)}
 * Use case 5 - Implicit Grant (RFC 6749 section 4.2). The implicit grant usually involves browser/javascript
 * redirection flows.
 * <li>Method {@code #getAccessTokenByImplicit(String, String, String)}
 * Use case 6 - Import OAuth access token for data migration. Existing implementations may choose to migrate
 * existing OAuth access tokens to be managed by this client.
 * <li>Method {@code #importAccessTokenResponse(AccessTokenResponse)}
 * Use case 7 - Get tokens - continue from Use case 1/2/4/5.
 * <li>Method {@code #getAccessTokenResponse()}
 * @author Hilbrand Bouwkamp - Added AccessTokenRefreshListener, fixed javadoc warnings
public interface OAuthClientService extends AutoCloseable {
     * Use case 1 Part (A)
     * This call produces a URL which can be used during the Authorization Code Grant part (A).
     * The OAuthClientService generate an authorization URL, which contains the HTTP query parameters needed for
     * authorization code grant.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.1">Authorization Code Grant illustration - rfc6749
     *      section-4.1</a>
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.1.1">Concerning which parameters must be set and
     *      which ones are optional - rfc6749 section-4.1.1</a>
     * @param redirectURI is the http request parameter which tells the oauth provider the URI to redirect the
     *            e.g. after the human user authenticate with the oauth provider by the browser, the oauth provider
     *            will redirect the browser to this redirectURL.
     * @param scope Specific scopes, if null the service specified scopes will be used
     * @param state If the state is not null, it will be added as the HTTP query parameter state=xxxxxxxx .
     *            If the state is null, a random UUID will be generated and added state=<random UUID>,
     *            the state will be assigned to the requestParams in this case.
     * @return An authorization URL during the Authorization Code Grant with http request parameters filled in.
     *         e.g Produces a URL string like this:
     *         {@code https://oauth.provider?response_type=code&client_id=myClientId&redirect_uri=redirectURI&scope=myScope&state=mySecureRandomState}
     * @throws OAuthException if authorizationUrl or clientId were not previously provided (null)
    String getAuthorizationUrl(@Nullable String redirectURI, @Nullable String scope, @Nullable String state)
            throws OAuthException;
     * Use case 1 Part (C). Part (B) is not in this client. Part (B) is about
     * redirecting the user and user-agent and is not in scope. This is a continuation of the flow of Authorization Code
     * Grant, part (C).
     * @param redirectURLwithParams This is the full redirectURI from Part (A),
     *            {@link #getAuthorizationUrl(String, String, String)}, but added with authorizationCode and state
     *            parameters
     *            returned by the oauth provider. It is encoded in application/x-www-form-urlencoded format
     *            as stated in RFC 6749 section 4.1.2.
     *            To quote from the RFC:
     *            <pre>{@code
     *            HTTP/1.1 302 Found
     *            Location: https://client.example.com/cb?code=SplxlOBeZQQYbYS6WxSbIA&state=xyz
     *            }</pre>
     * @return AuthorizationCode This authorizationCode can be used in the call {#getOAuthTokenByAuthCode(String)}
     * @throws OAuthException If the state from redirectURLwithParams does not exactly match the expectedState, or
     *             exceptions arise while parsing redirectURLwithParams.
     * @see #getAuthorizationUrl(String, String, String) for part (A)
    String extractAuthCodeFromAuthResponse(String redirectURLwithParams) throws OAuthException;
     * Use case 1 Part (D)
     * This is a continuation of the flow of Authorization Code Grant, part (D).
     * Get the cached access token by authorizationCode. This is exactly
     * RFC 4.1.3 Access Token Request
     * @param authorizationCode authorization code given by part (C) of the Authorization Code Grant
     *            {{@link #extractAuthCodeFromAuthResponse(String)}
     * @return AccessTokenResponse
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.1.3">Access Token Request - rfc6749 section-4.1.3</a>
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-5.2">Error Response - rfc6749 section-5.2</a>
    AccessTokenResponse getAccessTokenResponseByAuthorizationCode(String authorizationCode,
            @Nullable String redirectURI) throws OAuthException, IOException, OAuthResponseException;
     * Use case 2 - Resource Owner Password Credentials
     * This is for when the username and password of the actual resource owner (user) is known to the client.
     * @param username of the user
     * @param password of the user
     * @param scope of the access, a space delimited separated list
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.3.2">rfc6749 section-4.3.2</a>
    AccessTokenResponse getAccessTokenByResourceOwnerPasswordCredentials(String username, String password,
            @Nullable String scope) throws OAuthException, IOException, OAuthResponseException;
     * Use case 3 - refreshToken. Usually, it is only necessary to call {@code #getAccessTokenResponse()} directly.
     * It automatically takes care of refreshing the token if the token has become expired.
     * If the authorization server has invalidated the access token before the expiry,
     * then this call can be used to get a new acess token by using the refresh token.
     * @return new AccessTokenResponse from authorization server
     * @throws IOException Web/ network issues etc.
     * @throws OAuthResponseException For OAUTH error responses.
     * @throws OAuthException For other exceptions.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-5.2">rfc6749 section-5.2</a>
    AccessTokenResponse refreshToken() throws OAuthException, IOException, OAuthResponseException;
     * Use case 4 - Client Credentials
     * This is used to get the AccessToken by purely the client credential. The client
     * in this context is the program making the call to OAuthClientService. The actual
     * resource owner (human user) is not involved.
    AccessTokenResponse getAccessTokenByClientCredentials(@Nullable String scope)
            throws OAuthException, IOException, OAuthResponseException;
     * Use case 5 - Implicit Grant
     * The implicit grant usually involves browser/javascript redirection flows.
     * @param state An opaque value used by the client to maintain state between the request and callback. Recommended
     *            to prevent cross-site forgery.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.2">Implicit Grant - rfc6749 section-4.2</a>
    AccessTokenResponse getAccessTokenByImplicit(@Nullable String redirectURI, @Nullable String scope,
            @Nullable String state) throws OAuthException, IOException, OAuthResponseException;
     * Use case 6 - Import This method is used for importing/ migrating existing Access Token Response to be stored by
     * this service.
     * @param accessTokenResponse
     * @throws OAuthException if client is closed
    void importAccessTokenResponse(AccessTokenResponse accessTokenResponse) throws OAuthException;
     * Use case 7 - get access token response. The tokens may have been retrieved previously through Use cases: 1d, 2,
     * 3,
     * 4, 6.
     * The implementation uses following ways to get the AccesstokenResponse,
     * in following order :--
     * 1. no token in store ==> return null
     * 2. get from the store, token is still valid ==> return it.
     * 3. get from the store, but token is expired, no refresh token ==> return null
     * 4. get from the store, but token is expired, refresh token available ==> use refresh token to get new access
     * token.
     * @return AccessTokenResponse or null, depending on situations listed above.
    AccessTokenResponse getAccessTokenResponse() throws OAuthException, IOException, OAuthResponseException;
     * Remove all access token issued under this OAuthClientService.
     * Use this to remove existing token or if the access and refresh token has become invalid/ invalidated.
    void remove() throws OAuthException;
     * Stop the service and free underlying resources. This will not remove access tokens stored under the service.
     * The client cannot be used anymore if close has been previously called.
     * @return true if client is closed. i.e. {@link #close()} has been called.
    boolean isClosed();
     * Adds an {@link AccessTokenRefreshListener}.
    void addAccessTokenRefreshListener(AccessTokenRefreshListener listener);
     * Removes the {@link AccessTokenRefreshListener}.
    boolean removeAccessTokenRefreshListener(AccessTokenRefreshListener listener);
    void addExtraAuthField(String key, String value);
     * Adds a custom GsonBuilder to be used with the OAuth service instance.
     * @param gsonBuilder the custom GsonBuilder instance
     * @return the OAuth service
    OAuthClientService withGsonBuilder(GsonBuilder gsonBuilder);
     * @throws OAuthResponseException
    DeviceCodeResponseDTO getDeviceCodeResponse() throws OAuthException;
     * Notify the service listeners about the given {@link AccessTokenResponse}.
    void notifyAccessTokenResponse(AccessTokenResponse accessTokenResponse);
