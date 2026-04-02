 * Provides a certificate for the given host name
 * Implement this interface to request the framework to use a specific certificate for the given host
 * NOTE: implementations of this interface should be immutable, to guarantee efficient and correct functionality
public interface TlsCertificateProvider extends TlsProvider {
     * A resources pointing to a X509 certificate for the specified host name
     * @return this should refer to a file containing a base64 encoded X.509 certificate
    URL getCertificate();
