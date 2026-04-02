 * Some utility functions related to the http service
public class HttpServiceUtil {
    private HttpServiceUtil() {
     * Get the port that is used by the HTTP service.
     * @param bc the bundle context used for lookup
     * @return the port if used, -1 if no port could be found
    public static int getHttpServicePort(final BundleContext bc) {
        return getHttpServicePortProperty(bc, "org.osgi.service.http.port");
    public static int getHttpServicePortSecure(final BundleContext bc) {
        return getHttpServicePortProperty(bc, "org.osgi.service.http.port.secure");
    // Utility method that could be used for non-secure and secure port.
    private static int getHttpServicePortProperty(final BundleContext bc, final String propertyName) {
        // Try to find the port by using the service property (respect service ranking).
        final ServiceReference<?>[] refs;
            refs = bc.getAllServiceReferences("org.osgi.service.http.HttpService", null);
        } catch (final InvalidSyntaxException ex) {
            // This point of code should never be reached.
            final Logger logger = LoggerFactory.getLogger(HttpServiceUtil.class);
            logger.warn("This error should only be thrown if a filter could not be parsed. We don't use a filter...");
        int port = -1;
            int candidate = Integer.MIN_VALUE;
            for (final ServiceReference<?> ref : refs) {
                value = ref.getProperty(propertyName);
                final int servicePort;
                    servicePort = Integer.parseInt(value.toString());
                } catch (final NumberFormatException ex) {
                value = ref.getProperty(Constants.SERVICE_RANKING);
                final int serviceRanking;
                if (!(value instanceof Integer)) {
                    serviceRanking = 0;
                    serviceRanking = (Integer) value;
                if (serviceRanking >= candidate) {
                    candidate = serviceRanking;
                    port = servicePort;
        if (port > 0) {
        // If the service does not provide the port, try to use the system property.
        value = bc.getProperty(propertyName);
            if (value instanceof String) {
                    // If the property could not be parsed, the HTTP servlet itself has to care and warn about.
            } else if (value instanceof Integer integerValue) {
