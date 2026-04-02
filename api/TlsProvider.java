 * Provides some TLS validation implementation for the given host name
 * You should implement one of children of this interface, in order to request the framework to use a specific
 * implementation for the given host.
public interface TlsProvider {
     * Host name for which this tls-provider is intended.
     * It can either be matched on common-name (from the certificate) or peer-host / peer-port based on the actual
     * ssl-connection. Both options can be used without further configuration.
     * @return a host name in string format, eg: www.eclipse.org (based on certificate common-name) or
     *         www.eclipse.org:443 (based on peer host/port)
    String getHostName();
