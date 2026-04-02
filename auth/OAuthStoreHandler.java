 * This is for OAuth client internal use.
 * @author Andrew Fiddian-Green - added RFC-8628 support
public interface OAuthStoreHandler {
     * Get an AccessTokenResponse from the store. The access token and refresh token are encrypted
     * and therefore will be decrypted before returning.
     * If the storage is not available, it is still possible to get the AccessTokenResponse from memory cache.
     * However, the last-used statistics will be broken. It is a measured risk to take.
     * @param handle the handle given by the call
     *            {@code OAuthFactory#createOAuthClientService(String, String, String, String, String, Boolean)}
     * @return AccessTokenResponse if available, null if not.
     * @throws GeneralSecurityException when the token cannot be decrypted.
    AccessTokenResponse loadAccessTokenResponse(String handle) throws GeneralSecurityException;
     * Save the {@code AccessTokenResponse} by the handle
     * @param handle unique string used as a handle/ reference to the OAuth client service, and the underlying
     *            access tokens, configs.
     * @param accessTokenResponse This can be null, which explicitly removes the AccessTokenResponse from store.
    void saveAccessTokenResponse(String handle, @Nullable AccessTokenResponse accessTokenResponse);
     * Get a {@link DeviceCodeResponseDTO} from the store. The device code is encrypted and therefore will be decrypted
     * before returning.
     * If the storage is not available, it is still possible to get the DeviceCodeResponse from memory cache.
     * @return DeviceCodeResponse if available, null if not.
    DeviceCodeResponseDTO loadDeviceCodeResponse(String handle) throws GeneralSecurityException;
     * Save the {@code DeviceCodeResponse} by the handle
     * @param deviceCodeResponse This can be null, which explicitly removes the DeviceCodeResponse from store.
    void saveDeviceCodeResponse(String handle, @Nullable DeviceCodeResponseDTO deviceCodeResponse);
     * Remove the token for the given handler. No exception is thrown in all cases
    void remove(String handle);
     * Remove all data in the oauth store, !!!use with caution!!!
    void removeAll();
     * Save the {@code PersistedParams} into the store
     * @param persistedParams These parameters are static with respect to the oauth provider and thus can be persisted.
    void savePersistedParams(String handle, @Nullable PersistedParams persistedParams);
     * Load the {@code PersistedParams} from the store
     * @return PersistedParams when available, null if not exist
    PersistedParams loadPersistedParams(String handle);
