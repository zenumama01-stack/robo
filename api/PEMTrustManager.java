import java.security.KeyManagementException;
import java.security.cert.CertificateFactory;
import java.security.cert.CertificateParsingException;
import java.security.cert.X509Certificate;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLEngine;
import javax.net.ssl.X509ExtendedTrustManager;
 * The {@link PEMTrustManager} is a {@link X509ExtendedTrustManager} implementation which loads a certificate in
 * PEM format and validates it against the servers certificate.
public final class PEMTrustManager extends X509ExtendedTrustManager {
    public static final String BEGIN_CERT = "-----BEGIN CERTIFICATE-----";
    public static final String END_CERT = "-----END CERTIFICATE-----";
    private final Logger logger = LoggerFactory.getLogger(PEMTrustManager.class);
    private final X509Certificate trustedCert;
     * Creates a {@link PEMTrustManager} instance by passing the PEM certificate as {@link String}.
     * The PEM format typically starts with <code>"-----BEGIN CERTIFICATE-----"</code> and ends with
     * <code>"-----END CERTIFICATE-----"</code>. The base 64 encoded certificate information are placed in between.
     * @param pemCert the PEM certificate
     * @throws CertificateInstantiationException
     * @throws CertificateParsingException
    public PEMTrustManager(String pemCert) throws CertificateException {
        if (!pemCert.isBlank() && pemCert.startsWith(BEGIN_CERT)) {
            try (InputStream certInputStream = new ByteArrayInputStream(pemCert.getBytes(StandardCharsets.UTF_8))) {
                trustedCert = (X509Certificate) CertificateFactory.getInstance("X.509")
                        .generateCertificate(certInputStream);
                logger.debug("An IOException occurred: {}", e.getMessage());
                throw new CertificateInstantiationException(e);
            throw new CertificateParsingException("Certificate is either empty or cannot be parsed correctly");
     * Creates a {@link PEMTrustManager} instance by reading the PEM certificate from the given file.
     * This is useful if you have a private CA certificate stored in a file. Be aware that the certificate is read once
     * at the start of the system. There is no automatic refresh e.g. if the certificate will expire.
     * @param path path to the PEM file
     * @return a {@link PEMTrustManager} instance
    public static PEMTrustManager getInstanceFromFile(String path) throws FileNotFoundException, CertificateException {
        String pemCert = readPEMCertificateStringFromFile(path);
        if (pemCert != null) {
            return new PEMTrustManager(pemCert);
        throw new CertificateInstantiationException(String.format("Unable to read certificate from file: %s", path));
     * Creates a {@link PEMTrustManager} instance by downloading the PEM certificate from the given server.
     * This is useful if you have to deal with self-signed certificates which may differ on each server. This method
     * pins the certificate on first connection with the server ("trust on first use") by using a trust all connection
     * and retrieves the servers certificate chain. Be aware that the certificate is downloaded once at the start of the
     * system. There is no automatic refresh e.g. if the certificate will expire.
     * @param url url of the server
     * @throws MalformedURLException
    public static PEMTrustManager getInstanceFromServer(String url) throws MalformedURLException, CertificateException {
            return getInstanceFromServer((new URI(url)).toURL());
            throw new MalformedURLException(e.getMessage());
    public static PEMTrustManager getInstanceFromServer(URL url) throws CertificateException {
        String pemCert = getPEMCertificateFromServer(url);
        throw new CertificateInstantiationException(String.format("Unable to load certificate from server: %s", url));
    public void checkClientTrusted(X509Certificate @Nullable [] chain, @Nullable String authType)
            throws CertificateException {
        validatePEMCertificate(chain);
    public void checkServerTrusted(X509Certificate @Nullable [] chain, @Nullable String authType)
    public X509Certificate[] getAcceptedIssuers() {
        X509Certificate[] certs = { trustedCert };
        return certs;
    public void checkClientTrusted(X509Certificate @Nullable [] chain, @Nullable String authType,
            @Nullable Socket socket) throws CertificateException {
            @Nullable SSLEngine engine) throws CertificateException {
    public void checkServerTrusted(X509Certificate @Nullable [] chain, @Nullable String authType,
        return trustedCert.hashCode();
        if (!(obj instanceof PEMTrustManager)) {
        return trustedCert.equals(((PEMTrustManager) obj).trustedCert);
    private static @Nullable String getPEMCertificateFromServer(URL url) throws CertificateException {
        HttpsURLConnection connection = null;
            TrustManager[] trustManagers = { TrustAllTrustManager.getInstance() };
            SSLContext sslContext = SSLContext.getInstance("SSL");
            sslContext.init(null, trustManagers, new SecureRandom());
            connection = (HttpsURLConnection) url.openConnection();
            connection.setSSLSocketFactory(sslContext.getSocketFactory());
            connection.connect();
            Certificate[] certs = connection.getServerCertificates();
            byte[] bytes = ((X509Certificate) certs[0]).getEncoded();
            if (bytes.length != 0) {
                return BEGIN_CERT + System.lineSeparator() + Base64.getEncoder().encodeToString(bytes)
                        + System.lineSeparator() + END_CERT;
            LoggerFactory.getLogger(PEMTrustManager.class).debug("An IOException occurred: {}", e.getMessage());
        } catch (NoSuchAlgorithmException | KeyManagementException e) {
            LoggerFactory.getLogger(PEMTrustManager.class).error("An unexpected exception occurred: ", e);
                connection.disconnect();
    private static @Nullable String readPEMCertificateStringFromFile(String path) throws FileNotFoundException {
        File certFile = new File(path);
        if (certFile.exists()) {
                return Files.readString(certFile.toPath());
                LoggerFactory.getLogger(PEMTrustManager.class).error("An unexpected IOException occurred: ", e);
            throw new FileNotFoundException(String.format("File %s does not exist", path));
    private void validatePEMCertificate(X509Certificate @Nullable [] chain) throws CertificateException {
        if (chain == null || !trustedCert.equals(chain[0])) {
            throw new CertificateException();
    public static class CertificateInstantiationException extends CertificateException {
        private static final long serialVersionUID = -5861764697217665026L;
        public CertificateInstantiationException(String message) {
        public CertificateInstantiationException(Throwable cause) {
