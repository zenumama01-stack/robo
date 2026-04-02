package org.openhab.core.io.net.http;
import javax.net.ssl.TrustManager;
 * Provides an extensible composite TrustManager
 * The trust manager can be extended with implementations of the following interfaces:
 * - {@code TlsTrustManagerProvider}
 * - {@code TlsCertificateProvider}
 * @author Martin van Wingerden - Initial contribution
public interface ExtensibleTrustManager extends TrustManager {
     * Add a {@code TlsCertificateProvider} to be used by HttpClient / WebSocket Client's
     * When the Provider is no longer valid please make sure to remove it.
     * @param tlsCertificateProvider same instance as given when removing
    void addTlsCertificateProvider(TlsCertificateProvider tlsCertificateProvider);
     * Remove a {@code TlsCertificateProvider} so it will longer be used by HttpClient / WebSocket Client's
     * @param tlsCertificateProvider same instance as given when adding
    void removeTlsCertificateProvider(TlsCertificateProvider tlsCertificateProvider);
     * Add a {@code TlsTrustManagerProvider} to be used by HttpClient / WebSocket Client's
     * @param tlsTrustManagerProvider same instance as given when removing
    void addTlsTrustManagerProvider(TlsTrustManagerProvider tlsTrustManagerProvider);
     * Remove a {@code TlsTrustManagerProvider} so it will longer be used by HttpClient / WebSocket Client's
     * @param tlsTrustManagerProvider same instance as given when adding
    void removeTlsTrustManagerProvider(TlsTrustManagerProvider tlsTrustManagerProvider);
