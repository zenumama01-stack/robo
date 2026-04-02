 * Adapter to use a {@code TlsCertificateProvider} as a {@code TlsTrustManagerProvider}
class TlsCertificateTrustManagerAdapter implements TlsTrustManagerProvider {
    private final String hostname;
    private final X509ExtendedTrustManager trustManager;
    TlsCertificateTrustManagerAdapter(TlsCertificateProvider tlsCertificateProvider) {
        this.hostname = tlsCertificateProvider.getHostName();
        this.trustManager = trustManagerFromCertificate(this.hostname, tlsCertificateProvider.getCertificate());
    public String getHostName() {
    public X509ExtendedTrustManager getTrustManager() {
    private static X509ExtendedTrustManager trustManagerFromCertificate(String hostname, URL certificateUrl) {
            CertificateFactory certificateFactory = CertificateFactory.getInstance("X.509");
            try (InputStream inputStream = certificateUrl.openStream()) {
                KeyStore keyStore = KeyStore.getInstance("PKCS12");
                Certificate certificate = certificateFactory.generateCertificate(inputStream);
                keyStore.setCertificateEntry(hostname, certificate);
                return TrustManagerUtil.keyStoreToTrustManager(keyStore);
            } catch (KeyStoreException | NoSuchAlgorithmException e) {
                throw new IllegalStateException("Failed to initialize internal keystore", e);
        } catch (CertificateException | IOException e) {
            throw new IllegalStateException("Failed to initialize TrustManager", e);
