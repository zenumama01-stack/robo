import static java.util.stream.Collectors.toList;
import static org.openhab.core.io.rest.internal.filter.CorsFilter.*;
import javax.ws.rs.core.MultivaluedHashMap;
 * Test for the {@link CorsFilter} filter
 * @author Wouter Born - Migrate tests from Groovy to Java and use Mockito
public class CorsFilterTest {
    private static final String CONTENT_TYPE_HEADER = HttpHeaders.CONTENT_TYPE;
    private static final String AUTHORIZATION_HEADER = HttpHeaders.AUTHORIZATION;
    private static final String ACCESS_CONTROL_REQUEST_HEADERS = "Access-Control-Request-Headers";
    private static final String ECLIPSE_ORIGIN = "http://eclipse.org";
    private static final String VARY_HEADER_VALUE = "Content-Type";
    private static final String REQUEST_HEADERS = "X-Custom, X-Mine";
    private @NonNullByDefault({}) CorsFilter filter;
    private MultivaluedMap<String, String> responseHeaders = new MultivaluedHashMap<>();
    private @Mock @NonNullByDefault({}) ContainerRequestContext requestContextMock;
    private @Mock @NonNullByDefault({}) ContainerResponseContext responseContextMock;
        filter = new CorsFilter();
        filter.activate(Map.of("enable", "true"));
    public void notCorsOptionsRequestTest() throws IOException {
        setupRequestContext(HTTP_OPTIONS_METHOD, null, null, null);
        setupResponseContext(null);
        filter.filter(requestContextMock, responseContextMock);
        // Not a CORS request, thus no CORS headers should be added.
        assertResponseWithoutHeader(ACCESS_CONTROL_ALLOW_METHODS_HEADER);
        assertResponseWithoutHeader(ACCESS_CONTROL_ALLOW_ORIGIN_HEADER);
        assertResponseWithoutHeader(ACCESS_CONTROL_ALLOW_HEADERS);
        assertResponseWithoutHeader(VARY_HEADER);
    public void notCorsRealRequestTest() throws IOException {
        setupRequestContext(HTTP_GET_METHOD, null, null, null);
    public void corsPreflightRequestTest() throws IOException {
        setupRequestContext(HTTP_OPTIONS_METHOD, ECLIPSE_ORIGIN, HTTP_GET_METHOD, REQUEST_HEADERS);
        setupResponseContext(VARY_HEADER_VALUE);
        assertResponseHasHeader(ACCESS_CONTROL_ALLOW_METHODS_HEADER, ACCEPTED_HTTP_METHODS);
        assertResponseHasHeader(ACCESS_CONTROL_ALLOW_ORIGIN_HEADER, ECLIPSE_ORIGIN);
        assertResponseHasHeader(ACCESS_CONTROL_ALLOW_HEADERS, CONTENT_TYPE_HEADER);
        assertResponseHasHeader(ACCESS_CONTROL_ALLOW_HEADERS, AUTHORIZATION_HEADER);
        assertResponseHasHeader(VARY_HEADER, VARY_HEADER_VALUE + "," + ORIGIN_HEADER);
    public void partialCorsPreflightRequestTest() throws IOException {
        setupRequestContext(HTTP_OPTIONS_METHOD, ECLIPSE_ORIGIN, null, REQUEST_HEADERS);
        // Since the requestMethod header is not present in the request, it is not a valid Preflight CORS request.
        // Thus, no CORS header should be added to the response.
        assertResponseHasHeader(VARY_HEADER, VARY_HEADER_VALUE);
    public void corsPreflightRequestWithoutRequestHeadersTest() throws IOException {
        setupRequestContext(HTTP_OPTIONS_METHOD, ECLIPSE_ORIGIN, HTTP_GET_METHOD, null);
    public void corsRealRequestTest() throws IOException {
        setupRequestContext(HTTP_GET_METHOD, ECLIPSE_ORIGIN, null, null);
    private void assertResponseWithoutHeader(String header) {
        assertFalse(responseHeaders.containsKey(header));
    private void assertResponseHasHeader(String header, String value) {
        assertTrue(responseHeaders.containsKey(header));
        assertTrue(responseHeaders.get(header).contains(value));
    private void setupRequestContext(String methodValue, @Nullable String originValue,
            @Nullable String requestMethodValue, @Nullable String requestHeadersValue) {
        MultivaluedMap<String, String> headers = new MultivaluedHashMap<>();
        if (originValue != null) {
            headers.put(ORIGIN_HEADER, List.of(originValue));
        if (requestMethodValue != null) {
            headers.put(ACCESS_CONTROL_REQUEST_METHOD, List.of(requestMethodValue));
        if (requestHeadersValue != null) {
            headers.put(ACCESS_CONTROL_REQUEST_HEADERS, List.of(requestHeadersValue));
        when(requestContextMock.getHeaders()).thenReturn(headers);
        when(requestContextMock.getMethod()).thenReturn(methodValue);
    private void setupResponseContext(@Nullable String varyHeaderValue) {
        String localVaryHeaderValue = varyHeaderValue;
        if (localVaryHeaderValue != null) {
            responseHeaders.put(VARY_HEADER, Stream.of(localVaryHeaderValue).collect(toList()));
        when(responseContextMock.getHeaders()).thenReturn((MultivaluedHashMap) responseHeaders);
        when(responseContextMock.getStringHeaders()).thenReturn(responseHeaders);
