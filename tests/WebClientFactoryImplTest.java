import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.ThreadPoolExecutor;
import javax.net.ssl.SSLHandshakeException;
public class WebClientFactoryImplTest {
    private @NonNullByDefault({}) WebClientFactoryImpl webClientFactory;
    private static final String TEST_URL = "https://www.eclipse.org/";
    private @Mock @NonNullByDefault({}) ExtensibleTrustManagerImpl extensibleTrustManagerMock;
        webClientFactory = new WebClientFactoryImpl(extensibleTrustManagerMock);
        webClientFactory.activate(createConfigMap(4, 200, 60, 2, 10, 60));
    public void tearDown() throws InterruptedException {
        // Sometimes a java.nio.channels.ClosedSelectorException occurs when the commonWebSocketClient
        // is stopped while its threads are still starting. This would cause webClientFactory.deactivate()
        // to block forever so continue if it has not completed after 2 seconds.
        Thread deactivateThread = new Thread(() -> webClientFactory.deactivate());
        deactivateThread.start();
        deactivateThread.join(2000);
    public void testGetClients() throws Exception {
        HttpClient httpClient = webClientFactory.getCommonHttpClient();
        WebSocketClient webSocketClient = webClientFactory.getCommonWebSocketClient();
        assertThat(httpClient, is(notNullValue()));
        assertThat(webSocketClient, is(notNullValue()));
    @Disabled("connecting to the outside world makes this test flaky")
    public void testCommonClientUsesExtensibleTrustManager() throws Exception {
        ArgumentCaptor<X509Certificate[]> certificateChainCaptor = ArgumentCaptor.forClass(X509Certificate[].class);
        ArgumentCaptor<SSLEngine> sslEngineCaptor = ArgumentCaptor.forClass(SSLEngine.class);
        ContentResponse response = httpClient.GET(TEST_URL);
        if (response.getStatus() != 200) {
            fail("Statuscode != 200");
        verify(extensibleTrustManagerMock).checkServerTrusted(certificateChainCaptor.capture(), anyString(),
                sslEngineCaptor.capture());
        verifyNoMoreInteractions(extensibleTrustManagerMock);
        assertThat(sslEngineCaptor.getValue().getPeerHost(), is("www.eclipse.org"));
        assertThat(sslEngineCaptor.getValue().getPeerPort(), is(443));
        assertThat(certificateChainCaptor.getValue()[0].getSubjectX500Principal().getName(),
                containsString("eclipse.org"));
    public void testCommonClientUsesExtensibleTrustManagerFailure() throws Exception {
        doThrow(new CertificateException()).when(extensibleTrustManagerMock).checkServerTrusted(
                ArgumentMatchers.any(X509Certificate[].class), anyString(), ArgumentMatchers.any(SSLEngine.class));
        assertThrows(SSLHandshakeException.class, () -> {
                httpClient.GET(TEST_URL);
                throw e.getCause();
    @Disabled("only for manual test")
    public void testMultiThreadedShared() throws Exception {
        ThreadPoolExecutor workers = new ThreadPoolExecutor(20, 80, 60, TimeUnit.SECONDS,
                new ArrayBlockingQueue<>(50 * 50));
        final List<HttpClient> clients = new ArrayList<>();
        final int maxClients = 2;
        final int maxRequests = 2;
        for (int i = 0; i < maxClients; i++) {
            clients.add(httpClient);
        final List<String> failures = new ArrayList<>();
        for (int i = 0; i < maxRequests; i++) {
            for (final HttpClient client : clients) {
                workers.execute(new Runnable() {
                            ContentResponse response = client.GET(TEST_URL);
                                failures.add("Statuscode != 200");
                        } catch (InterruptedException | ExecutionException | TimeoutException e) {
                            failures.add("Unexpected exception:" + e.getMessage());
        workers.shutdown();
        workers.awaitTermination(120, TimeUnit.SECONDS);
        if (!failures.isEmpty()) {
            fail(failures.toString());
    public void testMultiThreadedCustom() throws Exception {
            HttpClient httpClient = webClientFactory.createHttpClient("consumer" + i);
        for (HttpClient client : clients) {
            client.stop();
    private Map<String, Object> createConfigMap(int minThreadsShared, int maxThreadsShared, int keepAliveTimeoutShared,
            int minThreadsCustom, int maxThreadsCustom, int keepAliveTimeoutCustom) {
        Map<String, Object> configMap = new HashMap<>();
        configMap.put("minThreadsShared", minThreadsShared);
        configMap.put("maxThreadsShared", maxThreadsShared);
        configMap.put("keepAliveTimeoutShared", keepAliveTimeoutShared);
        configMap.put("minThreadsCustom", minThreadsCustom);
        configMap.put("maxThreadsCustom", maxThreadsCustom);
        configMap.put("keepAliveTimeoutCustom", keepAliveTimeoutCustom);
        return configMap;
