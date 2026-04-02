 * JUnit tests for {@link OAuthConnectorRFC8628}
class OAuthRFC8628ClientTest {
    private final Gson gson = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_JSON_COMPAT)
            .registerTypeAdapter(Instant.class, (JsonSerializer<Instant>) (date, type,
            .create();
     * Private wrapper class for test purposes
    private static class OAuthConnectorRFC8628Ext extends OAuthConnectorRFC8628 {
        public OAuthConnectorRFC8628Ext(OAuthClientService oAuthClientService, String handle,
                OAuthStoreHandler oAuthStoreHandler, HttpClientFactory httpClientFactory,
                @Nullable GsonBuilder gsonBuilder, String accessTokenRequestUrl, String deviceCodeRequestUrl,
                String clientId, String scope) throws OAuthException {
            super(oAuthClientService, handle, oAuthStoreHandler, httpClientFactory, gsonBuilder, accessTokenRequestUrl,
                    deviceCodeRequestUrl, clientId, scope);
            // this suppresses the life-cycle errors that otherwise appear in the test log
    void testDeviceCodeResponseGoodWhenLoadedFromRemote() {
        OAuthClientService oAuthClientService = mock(OAuthClientService.class);
        OAuthStoreHandler oAuthStoreHandler = mock(OAuthStoreHandler.class);
        HttpClientFactory httpClientFactory = mock(HttpClientFactory.class);
        try (OAuthConnectorRFC8628 oAuthConnectorRFC8628 = new OAuthConnectorRFC8628Ext(oAuthClientService, "handle",
                oAuthStoreHandler, httpClientFactory, null, "accessTokenRequestUrl", "deviceCodeRequestUrl", "clientId",
                "scope")) {
            assertNotNull(oAuthConnectorRFC8628);
            Request request = mock(Request.class);
            HttpClient httpClient = mock(HttpClient.class);
            ContentResponse contentResponse = Mockito.mock(ContentResponse.class);
            when(httpClientFactory.createHttpClient(anyString())).thenReturn(httpClient);
            when(httpClient.isStarted()).thenReturn(true);
            when(httpClient.newRequest(anyString())).thenReturn(request);
                when(request.send()).thenReturn(contentResponse);
            DeviceCodeResponseDTO dcr = new DeviceCodeResponseDTO();
            dcr.setDeviceCode("DeviceCode");
            dcr.setExpiresIn(123);
            dcr.setInterval(4L);
            dcr.setUserCode("UserCode");
            dcr.setVerificationUri("VerificationUri");
            dcr.setVerificationUriComplete("VerificationUriComplete");
            when(contentResponse.getStatus()).thenReturn(200);
            when(contentResponse.getContentAsString()).thenReturn(gson.toJson(dcr), "");
            DeviceCodeResponseDTO dcrOut = null;
                dcrOut = oAuthConnectorRFC8628.getDeviceCodeResponse();
            assertNotNull(dcrOut);
            dcr.setCreatedOn(dcrOut.getCreatedOn()); // allow for test running time;
            assertEquals(dcr, dcrOut);
            verify(oAuthClientService, times(1)).isClosed();
                verify(oAuthClientService, times(1)).getAccessTokenResponse();
            } catch (OAuthException | IOException | OAuthResponseException e) {
                verify(oAuthStoreHandler, times(1)).loadDeviceCodeResponse(anyString());
                verify(oAuthClientService, times(0)).importAccessTokenResponse(any(AccessTokenResponse.class));
                verify(oAuthClientService, times(0)).notifyAccessTokenResponse(any(AccessTokenResponse.class));
    void testDeviceCodeResponseGoodWhenLoadedFromStorage() {
            when(contentResponse.getContentAsString()).thenReturn("");
                when(oAuthStoreHandler.loadDeviceCodeResponse(anyString())).thenReturn(dcr);
    void testDeviceCodeResponseNullWhenAccessTokenResponseLoadedFromRemote() {
            AccessTokenResponse atr = new AccessTokenResponse();
            atr.setAccessToken("AccessToken");
            atr.setExpiresIn(123);
            atr.setRefreshToken("RefreshToken");
            when(contentResponse.getContentAsString()).thenReturn(gson.toJson(dcr), gson.toJson(atr));
            assertNull(dcrOut);
                verify(oAuthClientService, times(1)).importAccessTokenResponse(any(AccessTokenResponse.class));
                verify(oAuthClientService, times(1)).notifyAccessTokenResponse(any(AccessTokenResponse.class));
    void testDeviceCodeResponseNullWhenAccessTokenResponseLoadedFromStorage() {
                when(oAuthClientService.getAccessTokenResponse()).thenReturn(atr);
            DeviceCodeResponseDTO dcr = null;
                dcr = oAuthConnectorRFC8628.getDeviceCodeResponse();
            assertNull(dcr);
                verify(oAuthStoreHandler, times(0)).loadDeviceCodeResponse(anyString());
    void testDeviceCodeResponseNullWhenServiceIsClosed() {
        when(oAuthClientService.isClosed()).thenReturn(true);
            assertThrows(OAuthException.class, () -> oAuthConnectorRFC8628.getDeviceCodeResponse());
