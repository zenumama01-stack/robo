 * The {@link TrustAllTrustManager} is a "trust all" implementation of {@link X509ExtendedTrustManager}.
 * @author Matthew Bowman - Initial contribution
public final class TrustAllTrustManager extends X509ExtendedTrustManager {
    private static TrustAllTrustManager instance = new TrustAllTrustManager();
    public static TrustAllTrustManager getInstance() {
     * private construction - singleton
    private TrustAllTrustManager() {
        return new X509Certificate[0];
