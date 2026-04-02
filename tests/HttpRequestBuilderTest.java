import java.nio.Buffer;
import org.eclipse.jetty.client.api.ContentProvider;
 * Test cases for the <code>HttpRequestBuilder</code> to validate its behaviour
public class HttpRequestBuilderTest extends BaseHttpUtilTest {
    public void baseTest() throws Exception {
        mockResponse(HttpStatus.OK_200);
        String result = HttpRequestBuilder.getFrom(URL).getContentAsString();
        assertEquals("Some content", result);
        verify(httpClientMock).newRequest(URI.create(URL));
        verify(requestMock).method(HttpMethod.GET);
        verify(requestMock).send();
    public void testHeader() throws Exception {
        String result = HttpRequestBuilder.getFrom(URL)
                .withHeader("Authorization", "Bearer sometoken")
                .withHeader("X-Token", "test")
                .getContentAsString();
        // verify the headers to be added to the request
        verify(requestMock).header("Authorization", "Bearer sometoken");
        verify(requestMock).header("X-Token", "test");
    public void testTimeout() throws Exception {
        String result = HttpRequestBuilder.getFrom(URL).withTimeout(Duration.ofMillis(200)).getContentAsString();
        // verify the timeout to be forwarded
        verify(requestMock).timeout(200, TimeUnit.MILLISECONDS);
    public void testPostWithContent() throws Exception {
        ArgumentCaptor<ContentProvider> argumentCaptor = ArgumentCaptor.forClass(ContentProvider.class);
        String result = HttpRequestBuilder.postTo(URL).withContent("{json: true}").getContentAsString();
        // verify the content to be added to the request
        verify(requestMock).content(argumentCaptor.capture(), ArgumentMatchers.eq(null));
        assertEquals("{json: true}", getContentFromProvider(argumentCaptor.getValue()));
    private String getContentFromProvider(ContentProvider value) {
        ByteBuffer element = value.iterator().next();
        byte[] data = new byte[element.limit()];
        // Explicit cast for compatibility with covariant return type on JDK 9's ByteBuffer
        ((ByteBuffer) ((Buffer) element.duplicate()).clear()).get(data);
        return new String(data, StandardCharsets.UTF_8);
    public void testPostWithContentType() throws Exception {
        String result = HttpRequestBuilder.postTo(URL).withContent("{json: true}", "application/json")
        // verify just the content-type to be added to the request
        verify(requestMock).method(HttpMethod.POST);
        verify(requestMock).content(argumentCaptor.capture(), ArgumentMatchers.eq("application/json"));
