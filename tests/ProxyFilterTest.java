import static org.openhab.core.io.rest.internal.filter.ProxyFilter.*;
 * Test for {@link ProxyFilter}
public class ProxyFilterTest {
    private final ProxyFilter filter = new ProxyFilter();
    private @Mock @NonNullByDefault({}) ContainerRequestContext contextMock;
    public void before() {
        when(contextMock.getUriInfo()).thenReturn(uriInfoMock);
    public void basicTest() throws Exception {
        String baseURI = "http://localhost:8080/rest";
        String requestURI = "http://localhost:8080/rest/test";
        setupContextURIs(baseURI, requestURI);
        setupContextHeaders("https", "eclipse.org");
        filter.filter(contextMock);
        URI newBaseURI = new URI("https://eclipse.org/rest");
        URI newRequestURI = new URI("https://eclipse.org/rest/test");
        verify(contextMock).setRequestUri(eq(newBaseURI), eq(newRequestURI));
    public void basicTest2() throws Exception {
        setupContextHeaders("http", "eclipse.org:8081");
        URI newBaseURI = new URI("http://eclipse.org:8081/rest");
        URI newRequestURI = new URI("http://eclipse.org:8081/rest/test");
    public void hostListTest() throws Exception {
        setupContextHeaders("https", "eclipse.org, foo.bar");
    public void noHeaderTest() throws Exception {
        setupContextHeaders(null, null);
        verify(contextMock, never()).setRequestUri(any(URI.class), any(URI.class));
    public void onlySchemeTest() throws Exception {
        setupContextHeaders("https", null);
        URI newBaseURI = new URI("https://localhost:8080/rest");
        URI newRequestURI = new URI("https://localhost:8080/rest/test");
    public void onlySchemeDefaultHostWithoutPortTest() throws Exception {
        String baseURI = "http://localhost/rest";
        String requestURI = "http://localhost/rest/test";
        URI newBaseURI = new URI("https://localhost/rest");
        URI newRequestURI = new URI("https://localhost/rest/test");
    public void onlyHostTest() throws Exception {
        setupContextHeaders(null, "eclipse.org:8081");
    public void invalidHeaderTest() throws Exception {
        setupContextHeaders("https", "://sometext\\\\///");
    private void setupContextURIs(String baseURI, String requestURI) {
        when(uriInfoMock.getBaseUri()).thenReturn(URI.create(baseURI));
        when(uriInfoMock.getBaseUriBuilder()).thenReturn(UriBuilder.fromUri(baseURI));
        when(uriInfoMock.getRequestUri()).thenReturn(URI.create(requestURI));
        when(uriInfoMock.getRequestUriBuilder()).thenReturn(UriBuilder.fromUri(requestURI));
    private void setupContextHeaders(@Nullable String protoHeader, @Nullable String hostHeader) {
        if (protoHeader != null) {
            headers.put(PROTO_PROXY_HEADER, List.of(protoHeader));
        if (hostHeader != null) {
            headers.put(HOST_PROXY_HEADER, List.of(hostHeader));
        when(contextMock.getHeaders()).thenReturn(headers);
