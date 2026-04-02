import static org.hamcrest.CoreMatchers.notNullValue;
import static org.hamcrest.Matchers.is;
 * The {@link OAuthStoreHandlerTest} contains tests for
 * {@link org.openhab.core.auth.oauth2client.internal.OAuthStoreHandlerImpl}
 * @author Jacob Laursen - Initial contribution
public class OAuthStoreHandlerTest {
    private @NonNullByDefault({}) OAuthStoreHandlerImpl storeHandler;
    void initialize() throws IOException {
        Mockito.doReturn(storage).when(storageService).getStorage(STORE_NAME);
        storeHandler = new OAuthStoreHandlerImpl(storageService);
    void loadAccessTokenResponseWhenCreatedOnIsLocalDateTime() throws GeneralSecurityException {
        final String handle = "test";
        final String createdOn = "2022-08-14T21:21:05.568991";
        final Instant expected = LocalDateTime.parse(createdOn).atZone(ZoneId.systemDefault()).toInstant();
        storage.put(StorageRecordType.ACCESS_TOKEN_RESPONSE.getKey(handle), getJsonforCreatedOn(createdOn));
        AccessTokenResponse response = storeHandler.loadAccessTokenResponse(handle);
        assertThat(response, is(notNullValue()));
        if (response != null) {
            assertThat(response.getCreatedOn(), is(expected));
    void loadAccessTokenResponseWhenCreatedOnIsInstant() throws GeneralSecurityException {
        final String createdOn = "2022-08-14T19:21:05.568991Z";
        final Instant expected = Instant.parse(createdOn);
    void savePersistedParamsShouldNotThrow() {
        storeHandler.savePersistedParams(handle, new PersistedParams());
    private String getJsonforCreatedOn(String createdOn) {
        return "{\"accessToken\": \"x\", \"tokenType\": \"Bearer\", \"expiresIn\": 2592000, \"refreshToken\": \"x\", \"createdOn\": \""
                + createdOn + "\"}";
