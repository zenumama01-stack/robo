import static org.mockito.ArgumentMatchers.anyLong;
 * Base class for tests for the <code>HttpRequestBuilder</code> and <code>HttpUtil</code> to validate their behavior
 * @author Martin van Wingerden and Wouter Born - Initial contribution
 * @author Markus Rathgeb - Base test classes without tests needs to be abstract
public abstract class BaseHttpUtilTest {
    static final String URL = "http://example.org/test";
    protected @Mock @NonNullByDefault({}) HttpClientFactory clientFactoryMock;
    protected @Mock @NonNullByDefault({}) HttpClient httpClientMock;
    protected @Mock @NonNullByDefault({}) Request requestMock;
    protected @Mock @NonNullByDefault({}) ContentResponse contentResponseMock;
        Field httpClientFactory = HttpUtil.class.getDeclaredField("httpClientFactory");
        httpClientFactory.setAccessible(true);
        httpClientFactory.set(null, clientFactoryMock);
        when(clientFactoryMock.getCommonHttpClient()).thenReturn(httpClientMock);
        when(httpClientMock.newRequest(URI.create(URL))).thenReturn(requestMock);
        when(requestMock.method(any(HttpMethod.class))).thenReturn(requestMock);
        when(requestMock.timeout(anyLong(), any(TimeUnit.class))).thenReturn(requestMock);
        when(requestMock.send()).thenReturn(contentResponseMock);
    void mockResponse(int httpStatus) {
        when(contentResponseMock.getStatus()).thenReturn(httpStatus);
        when(contentResponseMock.getContent()).thenReturn("Some content".getBytes());
