package org.openhab.core.io.transport.mqtt.ssl;
import javax.net.ssl.ManagerFactoryParameters;
import io.netty.handler.ssl.util.SimpleTrustManagerFactory;
 * The {@link CustomTrustManagerFactory} is a TrustManagerFactory that provides a custom {@link TrustManager}
public class CustomTrustManagerFactory extends SimpleTrustManagerFactory {
    private final TrustManager[] trustManagers;
    public CustomTrustManagerFactory(TrustManager[] trustManagers) {
        this.trustManagers = trustManagers;
    protected void engineInit(@Nullable KeyStore keyStore) throws Exception {
    protected void engineInit(@Nullable ManagerFactoryParameters managerFactoryParameters) throws Exception {
    protected TrustManager[] engineGetTrustManagers() {
        return trustManagers;
