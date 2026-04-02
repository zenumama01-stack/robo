import org.openhab.core.io.net.http.HttpUtil;
 * This class provides static methods that can be used in automation rules
 * for sending HTTP requests
 * @author Jan N. Klug - add timeout methods
 * @author Dan Cunningham - add image download methods
public class HTTP {
    /** Constant which represents the content type <code>application/json</code> */
    public static final String CONTENT_TYPE_JSON = "application/json";
    private static Logger logger = LoggerFactory.getLogger(HTTP.class);
     * Send out a GET-HTTP request. Errors will be logged, success returns response
     * @param url the URL to be used for the GET request.
    public static String sendHttpGetRequest(String url) {
        return sendHttpGetRequest(url, 5000);
     * @param timeout timeout in ms
    public static String sendHttpGetRequest(String url, int timeout) {
        String response = null;
            return HttpUtil.executeUrl(HttpMethod.GET.name(), url, timeout);
            logger.error("Fatal transport error: {}", e.getMessage());
     * Send out a GET-HTTP request. Errors will be logged, returned values just ignored.
     * @param headers the HTTP headers to be sent in the request.
    public static String sendHttpGetRequest(String url, Map<String, String> headers, int timeout) {
            Properties headerProperties = new Properties();
            headerProperties.putAll(headers);
            return HttpUtil.executeUrl(HttpMethod.GET.name(), url, headerProperties, null, null, timeout);
     * Send out a PUT-HTTP request. Errors will be logged, returned values just ignored.
     * @param url the URL to be used for the PUT request.
    public static String sendHttpPutRequest(String url) {
        return sendHttpPutRequest(url, 1000);
    public static String sendHttpPutRequest(String url, int timeout) {
            response = HttpUtil.executeUrl(HttpMethod.PUT.name(), url, timeout);
     * @param content the content to be send to the given <code>url</code> or <code>null</code> if no content should be
     *            send.
    public static String sendHttpPutRequest(String url, String contentType, String content) {
        return sendHttpPutRequest(url, contentType, content, 1000);
    public static String sendHttpPutRequest(String url, String contentType, String content, int timeout) {
            response = HttpUtil.executeUrl(HttpMethod.PUT.name(), url,
                    new ByteArrayInputStream(content.getBytes(StandardCharsets.UTF_8)), contentType, timeout);
     *            sent.
    public static String sendHttpPutRequest(String url, String contentType, String content, Map<String, String> headers,
            int timeout) {
            return HttpUtil.executeUrl(HttpMethod.PUT.name(), url, headerProperties,
     * Send out a POST-HTTP request. Errors will be logged, returned values just ignored.
     * @param url the URL to be used for the POST request.
    public static String sendHttpPostRequest(String url) {
        return sendHttpPostRequest(url, 1000);
    public static String sendHttpPostRequest(String url, int timeout) {
            response = HttpUtil.executeUrl(HttpMethod.POST.name(), url, timeout);
    public static String sendHttpPostRequest(String url, String contentType, String content) {
        return sendHttpPostRequest(url, contentType, content, 1000);
    public static String sendHttpPostRequest(String url, String contentType, String content, int timeout) {
            response = HttpUtil.executeUrl(HttpMethod.POST.name(), url,
    public static String sendHttpPostRequest(String url, String contentType, String content,
            Map<String, String> headers, int timeout) {
            return HttpUtil.executeUrl(HttpMethod.POST.name(), url, headerProperties,
     * Send out a DELETE-HTTP request. Errors will be logged, returned values just ignored.
     * @param url the URL to be used for the DELETE request.
    public static String sendHttpDeleteRequest(String url) {
        return sendHttpDeleteRequest(url, 1000);
    public static String sendHttpDeleteRequest(String url, int timeout) {
            response = HttpUtil.executeUrl(HttpMethod.DELETE.name(), url, timeout);
    public static String sendHttpDeleteRequest(String url, Map<String, String> headers, int timeout) {
            return HttpUtil.executeUrl(HttpMethod.DELETE.name(), url, headerProperties, null, null, timeout);
    @ActionDoc(text = "downloads an image from a url and updates the Image item's state with it", returns = "true if successful, false otherwise")
    public static boolean setImage(
            @ParamDoc(name = "itemName", text = "the name of the target Image Item") String itemName,
            @ParamDoc(name = "url", text = "the URL of the image") String url) {
        return setImage(itemName, HttpUtil.downloadImage(url));
            @ParamDoc(name = "url", text = "the URL of the image") String url,
            @ParamDoc(name = "timeout", text = "timeout in milliseconds") int timeout) {
        return setImage(itemName, HttpUtil.downloadImage(url, timeout));
            @ParamDoc(name = "maxContentLength", text = "maximum data size in bytes, negative to ignore") long maxContentLength,
        return setImage(itemName, HttpUtil.downloadImage(url, true, maxContentLength, timeout));
    private static boolean setImage(String itemName, RawType raw) {
        if (raw == null) {
            logger.error("Image download failed for item '{}'", itemName);
        ItemRegistry registry = ScriptServiceUtil.getItemRegistry();
        if (registry == null) {
            logger.error("Item registry is not available.");
            Item item = registry.getItem(itemName);
            BusEvent.postUpdate(item, raw);
            logger.error("Item '{}' does not exist.", itemName);
            logger.error("Cannot update item '{}' with image: {}", itemName, e.getMessage());
