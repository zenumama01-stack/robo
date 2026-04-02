 * {@link TestPortUtil} provides helper methods for working with ports in tests.
 * @author Wouter Born - Increase reusability
public final class TestPortUtil {
    private TestPortUtil() {
        // Hidden utility class constructor
     * Returns a free TCP/IP port number on localhost.
     * Heavily inspired from org.eclipse.jdt.launching.SocketUtil (to avoid a dependency to JDT just because of this).
     * Slightly improved with close() missing in JDT. And throws exception instead of returning -1.
     * @return a free TCP/IP port number on localhost
     * @throws IllegalStateException if unable to find a free port
    public static int findFreePort() {
        try (final ServerSocket socket = new ServerSocket(0)) {
            socket.setReuseAddress(true);
            return socket.getLocalPort();
            throw new IllegalStateException("Could not find a free TCP/IP port", ex);
