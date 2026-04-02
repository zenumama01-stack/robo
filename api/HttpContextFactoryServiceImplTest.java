public class HttpContextFactoryServiceImplTest {
    private static final String RESOURCE = "resource";
    private @NonNullByDefault({}) HttpContextFactoryServiceImpl httpContextFactoryService;
    private @Mock @NonNullByDefault({}) Bundle bundleMock;
    private @Mock @NonNullByDefault({}) WrappingHttpContext httpContextMock;
        httpContextFactoryService = new HttpContextFactoryServiceImpl();
        httpContextFactoryService.setHttpContext(httpContextMock);
        when(httpContextMock.wrap(bundleMock)).thenReturn(new BundleHttpContext(httpContextMock, bundleMock));
    public void shouldCreateHttpContext() {
        HttpContext context = httpContextFactoryService.createDefaultHttpContext(bundleMock);
        assertThat(context, is(notNullValue()));
        verify(httpContextMock).wrap(bundleMock);
    public void httpContextShouldCallgetResourceOnBundle() {
        context.getResource(RESOURCE);
        verify(bundleMock).getResource(RESOURCE);
    public void httpContextShouldCallgetResourceOnBundleWithoutLeadingSlash() {
        context.getResource("/" + RESOURCE);
