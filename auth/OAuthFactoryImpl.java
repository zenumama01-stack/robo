import org.openhab.core.auth.client.oauth2.OAuthFactory;
 * Implementation of {@link OAuthFactory}.
 * @author Hilbrand Bouwkamp - Changed implementation of createOAuthClientService
public class OAuthFactoryImpl implements OAuthFactory {
    private final Logger logger = LoggerFactory.getLogger(OAuthFactoryImpl.class);
    private int tokenExpiresInBuffer = OAuthClientServiceImpl.DEFAULT_TOKEN_EXPIRES_IN_BUFFER_SECOND;
    private final Map<String, OAuthClientService> oauthClientServiceCache = new ConcurrentHashMap<>();
    public OAuthFactoryImpl(final @Reference HttpClientFactory httpClientFactory,
            final @Reference OAuthStoreHandler oAuthStoreHandler) {
        // close each service
        for (OAuthClientService clientServiceImpl : oauthClientServiceCache.values()) {
            clientServiceImpl.close();
        oauthClientServiceCache.clear();
    public OAuthClientService createOAuthClientService(String handle, String tokenUrl,
            @Nullable String authorizationUrl, String clientId, @Nullable String clientSecret, @Nullable String scope,
            @Nullable Boolean supportsBasicAuth) {
        PersistedParams params = oAuthStoreHandler.loadPersistedParams(handle);
        PersistedParams newParams = new PersistedParams(handle, tokenUrl, authorizationUrl, clientId, clientSecret,
                scope, supportsBasicAuth, tokenExpiresInBuffer, null);
        OAuthClientService clientImpl = null;
        // If parameters in storage and parameters are the same as arguments passed get the client from storage
        if (params != null && params.equals(newParams)) {
            clientImpl = getOAuthClientService(handle);
        // If no client with parameters or with different parameters create or update (if parameters are different)
        // client in storage.
        if (clientImpl == null) {
            clientImpl = OAuthClientServiceImpl.createInstance(handle, oAuthStoreHandler, httpClientFactory, newParams);
            oauthClientServiceCache.put(handle, clientImpl);
        return clientImpl;
    public @Nullable OAuthClientService getOAuthClientService(String handle) {
        OAuthClientService clientImpl = oauthClientServiceCache.get(handle);
        if (clientImpl == null || clientImpl.isClosed()) {
            // This happens after reboot, or client was closed without factory knowing; create a new client
            // the store has the handle/config data
            clientImpl = OAuthClientServiceImpl.getInstance(handle, oAuthStoreHandler, tokenExpiresInBuffer,
                    httpClientFactory);
    public void ungetOAuthService(String handle) {
            logger.debug("{} handle not found. Cannot unregisterOAuthServie", handle);
        clientImpl.close();
        oauthClientServiceCache.remove(handle);
    public void deleteServiceAndAccessToken(String handle) {
        if (clientImpl != null) {
                clientImpl.remove();
                // client was already closed, does not matter
        oAuthStoreHandler.remove(handle);
    public int getTokenExpiresInBuffer() {
        return tokenExpiresInBuffer;
    public void setTokenExpiresInBuffer(int bufferInSeconds) {
        tokenExpiresInBuffer = bufferInSeconds;
