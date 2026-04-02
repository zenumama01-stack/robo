 * JUnit tests for {@link AccessTokenResponse}
 * @author Laurent Arnal - Initial contribution
class AccessTokenResponseExtraFieldTest {
    public void testExtraFieldDeserialization() {
        Gson gson = OAuthConnector.getGson(new GsonBuilder());
        // \"created_on\":\"2026-02-26T15:10:49.965249200Z\"
        String json = "{\"access_token\":\"AccessToken\",\"expires_in\":60,\"refresh_token\":\"RefreshToken\",\"app_client_id\":\"ApplicationClientId\"}";
        AccessTokenResponse atr = gson.fromJson(json, AccessTokenResponse.class);
            assertEquals("AccessToken", atr.getAccessToken());
            assertEquals(60, atr.getExpiresIn());
            assertEquals("RefreshToken", atr.getRefreshToken());
            Map<String, String> extraFields = atr.getExtraFields();
            assertEquals(1, extraFields.size());
            assertTrue(extraFields.containsKey("app_client_id"));
            assertEquals("ApplicationClientId", extraFields.get("app_client_id"));
