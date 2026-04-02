package org.openhab.core.io.net.http.internal;
import java.util.Queue;
import javax.security.auth.x500.X500Principal;
import org.openhab.core.io.net.http.ExtensibleTrustManager;
import org.openhab.core.io.net.http.TlsCertificateProvider;
import org.openhab.core.io.net.http.TlsTrustManagerProvider;
@Component(service = ExtensibleTrustManager.class, immediate = true)
public class ExtensibleTrustManagerImpl extends X509ExtendedTrustManager implements ExtensibleTrustManager {
    private final Logger logger = LoggerFactory.getLogger(ExtensibleTrustManagerImpl.class);
    private static final Queue<X509ExtendedTrustManager> EMPTY_QUEUE = new ConcurrentLinkedQueue<>();
    private final X509ExtendedTrustManager defaultTrustManager = TrustManagerUtil.keyStoreToTrustManager(null);
    private final Map<String, Queue<X509ExtendedTrustManager>> linkedTrustManager = new ConcurrentHashMap<>();
    private final Map<TlsCertificateProvider, X509ExtendedTrustManager> mappingFromTlsCertificateProvider = new ConcurrentHashMap<>();
        checkClientTrusted(chain, authType, (Socket) null);
        checkServerTrusted(chain, authType, (Socket) null);
        return defaultTrustManager.getAcceptedIssuers();
        X509ExtendedTrustManager linkedTrustManager = getLinkedTrustMananger(chain);
        if (linkedTrustManager == null) {
            logger.trace("No specific trust manager found, falling back to default");
            defaultTrustManager.checkClientTrusted(chain, authType, socket);
            linkedTrustManager.checkClientTrusted(chain, authType, socket);
            @Nullable SSLEngine sslEngine) throws CertificateException {
        X509ExtendedTrustManager linkedTrustManager = getLinkedTrustMananger(chain, sslEngine);
            defaultTrustManager.checkClientTrusted(chain, authType, sslEngine);
            linkedTrustManager.checkClientTrusted(chain, authType, sslEngine);
            defaultTrustManager.checkServerTrusted(chain, authType, socket);
            linkedTrustManager.checkServerTrusted(chain, authType, socket);
            defaultTrustManager.checkServerTrusted(chain, authType, sslEngine);
            linkedTrustManager.checkServerTrusted(chain, authType, sslEngine);
    private @Nullable X509ExtendedTrustManager getLinkedTrustMananger(X509Certificate @Nullable [] chain,
            @Nullable SSLEngine sslEngine) {
        if (sslEngine != null) {
            X509ExtendedTrustManager trustManager = null;
            String peer = null;
            if (sslEngine.getPeerHost() != null) {
                peer = sslEngine.getPeerHost() + ":" + sslEngine.getPeerPort();
                trustManager = linkedTrustManager.getOrDefault(peer, EMPTY_QUEUE).peek();
            if (trustManager != null) {
                logger.trace("Found trustManager by sslEngine peer/host: {}", peer);
                return trustManager;
                logger.trace("Did NOT find trustManager by sslEngine peer/host: {}", peer);
        return getLinkedTrustMananger(chain);
    private @Nullable X509ExtendedTrustManager getLinkedTrustMananger(X509Certificate @Nullable [] chain) {
        if (chain != null) {
                String commonName = getCommonName(chain[0]);
                X509ExtendedTrustManager trustManager = linkedTrustManager.getOrDefault(commonName, EMPTY_QUEUE).peek();
                    logger.trace("Found trustManager by common name: {}", commonName);
                Collection<List<?>> subjectAlternatives = getSubjectAlternatives(chain);
                logger.trace("Searching trustManager by Subject Alternative Names: {}", subjectAlternatives);
            return subjectAlternatives.stream()
                    .map(e -> e.get(1))
                    .map(Object::toString)
                    .map(linkedTrustManager::get)
                    .map(queue -> queue == null ? null : queue.peek())
                    .filter(Objects::nonNull)
                    .findFirst()
            } catch (CommonNameNotFoundException e) {
                logger.debug("CN not found", e);
            } catch (CertificateParsingException e) {
                logger.debug("Problem while parsing certificate", e);
    private Collection<List<?>> getSubjectAlternatives(X509Certificate[] chain) throws CertificateParsingException {
        Collection<List<?>> subjectAlternativeNames = chain[0].getSubjectAlternativeNames();
        return (subjectAlternativeNames != null) ? subjectAlternativeNames : List.of();
    private String getCommonName(X509Certificate x509Certificate) {
        String dn = x509Certificate.getSubjectX500Principal().getName(X500Principal.RFC2253);
        for (String group : dn.split(",")) {
            if (group.contains("CN=")) {
                return group.trim().replace("CN=", "");
        throw new CommonNameNotFoundException("No Common Name found in: '" + dn + "'");
    public void addTlsCertificateProvider(TlsCertificateProvider tlsCertificateProvider) {
        X509ExtendedTrustManager trustManager = new TlsCertificateTrustManagerAdapter(tlsCertificateProvider)
                .getTrustManager();
        mappingFromTlsCertificateProvider.put(tlsCertificateProvider, trustManager);
        addLinkedTrustManager(tlsCertificateProvider.getHostName(), trustManager);
    public void removeTlsCertificateProvider(TlsCertificateProvider tlsCertificateProvider) {
        X509ExtendedTrustManager trustManager = mappingFromTlsCertificateProvider.remove(tlsCertificateProvider);
            removeLinkedTrustManager(tlsCertificateProvider.getHostName(), trustManager);
    public void addTlsTrustManagerProvider(TlsTrustManagerProvider tlsTrustManagerProvider) {
        addLinkedTrustManager(tlsTrustManagerProvider.getHostName(), tlsTrustManagerProvider.getTrustManager());
    public void removeTlsTrustManagerProvider(TlsTrustManagerProvider tlsTrustManagerProvider) {
        removeLinkedTrustManager(tlsTrustManagerProvider.getHostName(), tlsTrustManagerProvider.getTrustManager());
    private void addLinkedTrustManager(String hostName, X509ExtendedTrustManager trustManager) {
        linkedTrustManager.computeIfAbsent(hostName, h -> new ConcurrentLinkedQueue<>()).add(trustManager);
    private void removeLinkedTrustManager(String hostName, X509ExtendedTrustManager trustManager) {
        linkedTrustManager.computeIfAbsent(hostName, h -> new ConcurrentLinkedQueue<>()).remove(trustManager);
    private static class CommonNameNotFoundException extends RuntimeException {
        public CommonNameNotFoundException(String message) {
