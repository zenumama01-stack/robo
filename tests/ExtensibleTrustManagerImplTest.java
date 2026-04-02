 * Tests which validate the behavior of the ExtensibleTrustManager
public class ExtensibleTrustManagerImplTest {
    private @NonNullByDefault({}) X509Certificate[] chain;
    private @NonNullByDefault({}) ExtensibleTrustManagerImpl subject;
    private @Mock @NonNullByDefault({}) TlsTrustManagerProvider trustmanagerProviderMock;
    private @Mock @NonNullByDefault({}) TlsTrustManagerProvider trustmanagerProviderHostPortMock;
    private @Mock @NonNullByDefault({}) X509ExtendedTrustManager trustmanagerMock;
    private @Mock @NonNullByDefault({}) X509ExtendedTrustManager trustmanager2Mock;
    private @Mock @NonNullByDefault({}) X509ExtendedTrustManager defaultTrustManagerMock;
    private @Mock @NonNullByDefault({}) SSLEngine sslEngineMock;
    private @Mock @NonNullByDefault({}) X509Certificate topOfChainMock;
    private @Mock @NonNullByDefault({}) X509Certificate bottomOfChainMock;
        when(trustmanagerProviderMock.getHostName()).thenReturn("example.org");
        when(trustmanagerProviderMock.getTrustManager()).thenReturn(trustmanagerMock);
        when(trustmanagerProviderHostPortMock.getHostName()).thenReturn("example.org:443");
        when(trustmanagerProviderHostPortMock.getTrustManager()).thenReturn(trustmanager2Mock);
        subject = new ExtensibleTrustManagerImpl();
        subject.addTlsTrustManagerProvider(trustmanagerProviderMock);
        subject.addTlsTrustManagerProvider(trustmanagerProviderHostPortMock);
        chain = new X509Certificate[] { topOfChainMock, bottomOfChainMock };
    public void shouldForwardCallsToMockForMatchingCN() throws CertificateException {
        when(topOfChainMock.getSubjectX500Principal())
                .thenReturn(new X500Principal("CN=example.org, OU=Core, O=openHAB, C=DE"));
        subject.checkServerTrusted(chain, "just");
        verify(trustmanagerMock).checkServerTrusted(chain, "just", (Socket) null);
        verifyNoMoreInteractions(trustmanagerMock, trustmanager2Mock);
    public void shouldForwardCallsToMockForMatchingHost() throws CertificateException {
        when(sslEngineMock.getPeerHost()).thenReturn("example.org");
        when(sslEngineMock.getPeerPort()).thenReturn(443);
        subject.checkServerTrusted(chain, "just", sslEngineMock);
        verify(trustmanager2Mock).checkServerTrusted(chain, "just", sslEngineMock);
    public void shouldForwardCallsToMockForMatchingAlternativeNames() throws CertificateException {
                .thenReturn(new X500Principal("CN=example.com, OU=Core, O=openHAB, C=DE"));
        when(topOfChainMock.getSubjectAlternativeNames())
                .thenReturn(constructAlternativeNames("example1.com", "example.org"));
        subject.checkClientTrusted(chain, "just");
        verify(trustmanagerMock).checkClientTrusted(chain, "just", (Socket) null);
        verifyNoMoreInteractions(trustmanagerMock);
    public void shouldBeResilientAgainstNullSubjectAlternativeNames()
            throws CertificateException, IllegalAccessException, NoSuchFieldException, SecurityException {
        writeField(subject, "defaultTrustManager", defaultTrustManagerMock, true);
        when(topOfChainMock.getSubjectAlternativeNames()).thenReturn(null);
        verify(defaultTrustManagerMock).checkClientTrusted(chain, "just", (Socket) null);
    public void shouldBeResilientAgainstMissingCommonNames() throws CertificateException, IllegalAccessException,
            NoSuchFieldException, SecurityException, IllegalArgumentException {
        when(topOfChainMock.getSubjectX500Principal()).thenReturn(new X500Principal("OU=Core, O=openHAB, C=DE"));
    public void shouldBeResilientAgainstInvalidCertificates() throws CertificateException, IllegalAccessException,
                .thenThrow(new CertificateParsingException("Invalid certificate!!!"));
    public void shouldNotForwardCallsToMockForDifferentCN() throws CertificateException, IllegalAccessException,
        mockSubjectForCertificate(topOfChainMock, "CN=example.com, OU=Core, O=openHAB, C=DE");
        mockIssuerForCertificate(topOfChainMock, "CN=openHAB, OU=Core, O=openHAB, C=DE");
        mockSubjectForCertificate(bottomOfChainMock, "CN=openHAB, OU=Core, O=openHAB, C=DE");
        mockIssuerForCertificate(bottomOfChainMock, "");
        when(topOfChainMock.getEncoded()).thenReturn(new byte[0]);
        verify(defaultTrustManagerMock).checkServerTrusted(chain, "just", (Socket) null);
        verifyNoInteractions(trustmanagerMock);
    private Collection<List<?>> constructAlternativeNames(String... alternatives) {
        Collection<List<?>> alternativeNames = new ArrayList<>();
        for (String alternative : alternatives) {
            alternativeNames.add(List.of(0, alternative));
        return alternativeNames;
    private void mockSubjectForCertificate(X509Certificate certificate, String principal) {
        when(certificate.getSubjectX500Principal()).thenReturn(new X500Principal(principal));
    private void mockIssuerForCertificate(X509Certificate certificate, String principal) {
        when(certificate.getIssuerX500Principal()).thenReturn(new X500Principal(principal));
    private void writeField(Object target, String fieldName, Object value, boolean forceAccess)
            throws NoSuchFieldException, SecurityException, IllegalArgumentException, IllegalAccessException {
        Field field = target.getClass().getDeclaredField(fieldName);
        field.setAccessible(forceAccess);
        field.set(target, value);
