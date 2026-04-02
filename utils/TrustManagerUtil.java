import javax.net.ssl.TrustManagerFactory;
 * Internal utility class to handle TrustManager's
class TrustManagerUtil {
    static X509ExtendedTrustManager keyStoreToTrustManager(@Nullable KeyStore keyStore) {
            TrustManagerFactory tmf = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
            tmf.init(keyStore);
            // Get hold of the X509ExtendedTrustManager
            for (TrustManager tm : tmf.getTrustManagers()) {
                if (tm instanceof X509ExtendedTrustManager manager) {
            throw new IllegalStateException("Default algorithm missing...", e);
        } catch (KeyStoreException e) {
            throw new IllegalStateException("Problem while processing keystore", e);
        throw new IllegalStateException("Could not find X509ExtendedTrustManager");
