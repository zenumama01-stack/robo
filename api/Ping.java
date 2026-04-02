 * This Action checks the vitality of the given host.
public class Ping {
     * Checks the vitality of <code>host</code>. If <code>port</code> '0'
     * is specified (which is the default when configuring just the host), a
     * regular ping is issued. If other ports are specified we try open a new
     * Socket with the given <code>timeout</code>.
     * @param host
     * @param port
     * @return <code>true</code> when <code>host</code> is reachable on <code>port</code> within the given
     *         <code>timeout</code> and <code>false</code> in all other
     *         cases.
     * @throws SocketTimeoutException
    public static boolean checkVitality(String host, int port, int timeout) throws IOException, SocketTimeoutException {
        boolean success = false;
        if (host != null && timeout > 0) {
            if (port == 0) {
                success = InetAddress.getByName(host).isReachable(timeout);
                SocketAddress socketAddress = new InetSocketAddress(host, port);
                Socket socket = new Socket();
                socket.connect(socketAddress, timeout);
