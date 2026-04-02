 * The OAuth Factory interface
 * @author Gary Tse - ESH Adaptation
 * @author Hilbrand Bouwkamp - Change create to have a handle as parameter.
public interface OAuthFactory {
     * Creates a new OAuth service. Use this method only once to obtain a handle and store
     * this handle for further in a persistent storage container.
     * @param handle the handle to the OAuth service
     * @param tokenUrl the token url of the OAuth provider. This is used for getting access token.
     * @param authorizationUrl the authorization url of the OAuth provider. This is used purely for generating
    OAuthClientService createOAuthClientService(String handle, String tokenUrl, @Nullable String authorizationUrl,
            String clientId, @Nullable String clientSecret, @Nullable String scope,
            @Nullable Boolean supportsBasicAuth);
     * Gets the OAuth service for a given handle
     * @return the OAuth service or null if it doesn't exist
    OAuthClientService getOAuthClientService(String handle);
     * Unget an OAuth service, this unget/unregister the service, and frees the resources.
     * The existing tokens/ configurations (persisted parameters) are still saved
     * in the store. It will internally call {@code OAuthClientService#close()}.
     * Best practise: unget, and close the OAuth service with this method!
     * If OAuth service is closed directly, without using {@code #ungetOAuthService(String)},
     * then a small residual footprint is left in the cache.
    void ungetOAuthService(String handle);
     * This method is for unget/unregister the service,
     * then <strong>DELETE</strong> access token, configuration data from the store
    void deleteServiceAndAccessToken(String handle);
