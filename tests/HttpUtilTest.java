 * Tests for the HttpUtil
 * @author Martin van Wingerden - Added tests based on HttpClientFactory
public class HttpUtilTest extends BaseHttpUtilTest {
        String result = HttpUtil.executeUrl("GET", URL, 500);
        verify(requestMock).timeout(500, TimeUnit.MILLISECONDS);
    public void testAuthentication() throws Exception {
        when(httpClientMock.newRequest(URI.create("http://john:doe@example.org/"))).thenReturn(requestMock);
        String result = HttpUtil.executeUrl("GET", "http://john:doe@example.org/", 500);
        verify(requestMock).header(HttpHeader.AUTHORIZATION, "Basic am9objpkb2U=");
    public void testCreateHttpMethod() {
        assertEquals(HttpMethod.GET, HttpUtil.createHttpMethod("GET"));
        assertEquals(HttpMethod.PUT, HttpUtil.createHttpMethod("PUT"));
        assertEquals(HttpMethod.POST, HttpUtil.createHttpMethod("POST"));
        assertEquals(HttpMethod.DELETE, HttpUtil.createHttpMethod("DELETE"));
    public void testCreateHttpMethodForUnsupportedFake() {
        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class,
                () -> HttpUtil.createHttpMethod("FAKE"));
        assertEquals("Given HTTP Method 'FAKE' is unknown", exception.getMessage());
    public void testCreateHttpMethodForUnsupportedTrace() {
                () -> HttpUtil.createHttpMethod("TRACE"));
        assertEquals("Given HTTP Method 'TRACE' is unknown", exception.getMessage());
