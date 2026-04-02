 * Provides a trust manager for the given host name
 * Implement this interface to request the framework to use a specific trust manager for the given host
public interface TlsTrustManagerProvider extends TlsProvider {
     * A X509ExtendedTrustManager for the specified host name
     * Note that the implementation might call this method multiple times make sure to return the same instance in that
     * case
     * @return this can for example be a trustManager extracted after importing a JKS trust-store
    X509ExtendedTrustManager getTrustManager();
