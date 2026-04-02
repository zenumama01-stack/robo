 * Unit tests for the {@link ProxyServletService} class.
public class ProxyServletServiceTest {
    private static final String SITEMAP_NAME = "testSitemap";
    private static final String SWITCH_WIDGET_ID = "switchWidget";
    private static final String IMAGE_WIDGET_ID = "imageWidget";
    private static final String VIDEO_WIDGET_ID = "videoWidget";
    private static final String ITEM_NAME_UNDEF_STATE = "itemUNDEF";
    private static final String ITEM_NAME_NULL_STATE = "itemNULL";
    private static final String ITEM_NAME_ON_STATE = "itemON";
    private static final String ITEM_NAME_INVALID_URL = "itemInvalidUrl";
    private static final String ITEM_NAME_VALID_IMAGE_URL = "itemValidImageUrl";
    private static final String ITEM_NAME_VALID_VIDEO_URL = "itemValidVideoUrl";
    private static final String INVALID_URL = "test";
    private static final String ITEM_VALID_IMAGE_URL = "https://openhab.org/item.jpg";
    private static final String ITEM_VALID_VIDEO_URL = "https://openhab.org/item.mp4";
    private static final String VALID_IMAGE_URL = "https://openhab.org/test.jpg";
    private static final String VALID_VIDEO_URL = "https://openhab.org/test.mp4";
    private @NonNullByDefault({}) ProxyServletService service;
    private @Mock @NonNullByDefault({}) HttpService httpServiceMock;
    private @Mock @NonNullByDefault({}) Switch switchWidgetMock;
    private @Mock @NonNullByDefault({}) Image imageWidgetMock;
    private @Mock @NonNullByDefault({}) Video videoWidgetMock;
        service = new ProxyServletService(httpServiceMock, itemUIRegistryMock, sitemapRegistryMock, Map.of());
        sitemapMock = mock(Sitemap.class);
        when(sitemapRegistryMock.get(eq(SITEMAP_NAME))).thenReturn(sitemapMock);
        when(itemUIRegistryMock.getWidget(eq(sitemapMock), eq(SWITCH_WIDGET_ID))).thenReturn(switchWidgetMock);
        when(itemUIRegistryMock.getWidget(eq(sitemapMock), eq(IMAGE_WIDGET_ID))).thenReturn(imageWidgetMock);
        when(itemUIRegistryMock.getWidget(eq(sitemapMock), eq(VIDEO_WIDGET_ID))).thenReturn(videoWidgetMock);
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_UNDEF_STATE))).thenReturn(UnDefType.UNDEF);
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_NULL_STATE))).thenReturn(UnDefType.NULL);
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_ON_STATE))).thenReturn(OnOffType.ON);
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_INVALID_URL))).thenReturn(new StringType(INVALID_URL));
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_VALID_IMAGE_URL)))
                .thenReturn(new StringType(ITEM_VALID_IMAGE_URL));
        when(itemUIRegistryMock.getItemState(eq(ITEM_NAME_VALID_VIDEO_URL)))
                .thenReturn(new StringType(ITEM_VALID_VIDEO_URL));
        when(requestMock.getParameter(eq("sitemap"))).thenReturn(SITEMAP_NAME);
    public void testMaybeAppendAuthHeaderWithFullCredentials() {
        URI uri = URI.create("http://testuser:testpassword@127.0.0.1:8080/content");
        service.maybeAppendAuthHeader(uri, request);
        verify(request).header(HttpHeader.AUTHORIZATION,
                "Basic " + Base64.getEncoder().encodeToString("testuser:testpassword".getBytes()));
    public void testMaybeAppendAuthHeaderWithoutPassword() {
        URI uri = URI.create("http://testuser@127.0.0.1:8080/content");
                "Basic " + Base64.getEncoder().encodeToString("testuser:".getBytes()));
    public void testMaybeAppendAuthHeaderWithoutCredentials() {
        URI uri = URI.create("http://127.0.0.1:8080/content");
        verify(request, never()).header(any(HttpHeader.class), anyString());
    public void testProxyUriUnexpectedWidgetType() {
        when(requestMock.getParameter(eq("widgetId"))).thenReturn(SWITCH_WIDGET_ID);
        URI uri = service.uriFromRequest(requestMock);
        assertNull(uri);
    public void testProxyUriImageWithoutItemButValidUrl() {
        when(requestMock.getParameter(eq("widgetId"))).thenReturn(IMAGE_WIDGET_ID);
        when(imageWidgetMock.getUrl()).thenReturn(VALID_IMAGE_URL);
        when(imageWidgetMock.getItem()).thenReturn(null);
        assertNotNull(uri);
        assertEquals(VALID_IMAGE_URL, uri.toString());
    public void testProxyUriImageWithoutItemAndInvalidUrl() {
        when(imageWidgetMock.getUrl()).thenReturn(INVALID_URL);
    public void testProxyUriImageWithItemButUndefState() {
        when(imageWidgetMock.getItem()).thenReturn(ITEM_NAME_UNDEF_STATE);
    public void testProxyUriImageWithItemButNullState() {
        when(imageWidgetMock.getItem()).thenReturn(ITEM_NAME_NULL_STATE);
    public void testProxyUriImageWithItemButUnexpectedState() {
        when(imageWidgetMock.getItem()).thenReturn(ITEM_NAME_ON_STATE);
    public void testProxyUriImageWithItemButStateWithInvalidUrl() {
        when(imageWidgetMock.getItem()).thenReturn(ITEM_NAME_INVALID_URL);
    public void testProxyUriImageWithItemAndStateWithValidUrl() {
        when(imageWidgetMock.getItem()).thenReturn(ITEM_NAME_VALID_IMAGE_URL);
        assertEquals(ITEM_VALID_IMAGE_URL, uri.toString());
    public void testProxyUriVideoWithoutItemButValidUrl() {
        when(requestMock.getParameter(eq("widgetId"))).thenReturn(VIDEO_WIDGET_ID);
        when(videoWidgetMock.getUrl()).thenReturn(VALID_VIDEO_URL);
        when(videoWidgetMock.getItem()).thenReturn(null);
        assertEquals(VALID_VIDEO_URL, uri.toString());
    public void testProxyUriVideoWithoutItemAndInvalidUrl() {
        when(videoWidgetMock.getUrl()).thenReturn(INVALID_URL);
    public void testProxyUriVideoWithItemButUndefState() {
        when(videoWidgetMock.getItem()).thenReturn(ITEM_NAME_UNDEF_STATE);
    public void testProxyUriVideoWithItemButNullState() {
        when(videoWidgetMock.getItem()).thenReturn(ITEM_NAME_NULL_STATE);
    public void testProxyUriVideoWithItemButUnexpectedState() {
        when(videoWidgetMock.getItem()).thenReturn(ITEM_NAME_ON_STATE);
    public void testProxyUriVideoWithItemButStateWithInvalidUrl() {
        when(videoWidgetMock.getItem()).thenReturn(ITEM_NAME_INVALID_URL);
    public void testProxyUriVideoWithItemAndStateWithValidUrl() {
        when(videoWidgetMock.getItem()).thenReturn(ITEM_NAME_VALID_VIDEO_URL);
        assertEquals(ITEM_VALID_VIDEO_URL, uri.toString());
