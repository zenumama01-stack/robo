 * Builder class to construct http requests
public class HttpRequestBuilder {
    private final String method;
    private final String url;
    private Duration timeout = Duration.ofSeconds(5);
    private @Nullable Properties headers;
    private @Nullable InputStream inputStream;
    private @Nullable String contentType;
     * Private constructor, to hide away the construction behind some factory methods
     * @param method Http method to be used
     * @param url url to fetch or post data to
    private HttpRequestBuilder(String method, String url) {
     * Construct an http request builder to get data from an <code>url</code>
     * @param url to fetch the data from
     * @return a request builder to construct and complete the request
    public static HttpRequestBuilder getFrom(String url) {
        return new HttpRequestBuilder("GET", url);
     * Construct an http request builder to post data to a <code>url</code>
     * @param url to post data to
    public static HttpRequestBuilder postTo(String url) {
        return new HttpRequestBuilder("POST", url);
     * Add a timeout for this request
     * @param timeout the timeout for this http request as a <code>Duration</code>
    public HttpRequestBuilder withTimeout(Duration timeout) {
     * Add an additional header to the request
     * @param header name of the header, eg. Content-Type
     * @param value value of the header, eg. "application/json"
    public HttpRequestBuilder withHeader(String header, String value) {
        Properties localHeaders = headers;
        if (localHeaders == null) {
            headers = new Properties();
            localHeaders = headers;
        localHeaders.put(header, value);
     * Add content to this request
     * @param content a string containing the data to be pushed to the <code>url</code>
    public HttpRequestBuilder withContent(String content) {
        this.inputStream = new ByteArrayInputStream(content.getBytes(StandardCharsets.UTF_8));
     * Add content with a specific type to this request
     * @param contentType the content type of the given <code>content</code>
    public HttpRequestBuilder withContent(String content, String contentType) {
        withContent(content);
     * Executes the build request
     * @return the response body or <code>null</code> when the request went wrong
     * @throws IOException when the request execution failed, timed out or it was interrupted
    public String getContentAsString() throws IOException {
        // this cast should be okay, a requested timeout should normally not exceed a long
        int timeoutMillis = (int) timeout.toMillis();
        return HttpUtil.executeUrl(method, url, headers, inputStream, contentType, timeoutMillis);
