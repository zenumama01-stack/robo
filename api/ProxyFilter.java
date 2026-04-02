 * A filter used to update both base and request URIs in Jersey's request
 * context if proxy headers are detected.
@Component(configurationPid = "org.openhab.proxyfilter")
public class ProxyFilter implements ContainerRequestFilter {
    static final String PROTO_PROXY_HEADER = "x-forwarded-proto";
    static final String HOST_PROXY_HEADER = "x-forwarded-host";
    private final transient Logger logger = LoggerFactory.getLogger(ProxyFilter.class);
    public void filter(@NonNullByDefault({}) ContainerRequestContext ctx) throws IOException {
        String host = getValue(ctx.getHeaders(), HOST_PROXY_HEADER);
        String scheme = getValue(ctx.getHeaders(), PROTO_PROXY_HEADER);
        // if our request does not have scheme or headers end right here
        if (scheme == null && host == null) {
        UriInfo uriInfo = ctx.getUriInfo();
        URI requestUri = uriInfo.getRequestUri();
        UriBuilder baseBuilder = uriInfo.getBaseUriBuilder();
        UriBuilder requestBuilder = uriInfo.getRequestUriBuilder();
        // if only one of our headers is missing replace it with default value
        if (scheme == null) {
            scheme = requestUri.getScheme();
        if (host == null) {
            host = requestUri.getHost();
            int port = requestUri.getPort();
            if (port != -1) {
                host += (":" + port);
        // host may contain a list of hosts, cf. https://httpd.apache.org/docs/2.4/mod/mod_proxy.html#x-headers
        // we only take the first hostname
        if (host.indexOf(",") > 0) {
            host = host.substring(0, host.indexOf(","));
        // create a new URI from the current scheme + host in order to validate
        // it
        String uriString = scheme + "://" + host.trim();
        URI newBaseUri;
            newBaseUri = new URI(uriString);
            logger.error("Invalid X-Forwarded-Proto + X-Forwarded-Host header combination: {}", uriString, e);
        // URI is valid replace base and request builder parts with ones
        // obtained from the given headers
        host = newBaseUri.getHost();
        if (host != null) {
            baseBuilder.host(host);
            requestBuilder.host(host);
        int port = newBaseUri.getPort();
        baseBuilder.port(port);
        requestBuilder.port(port);
        scheme = newBaseUri.getScheme();
        if (scheme != null) {
            baseBuilder.scheme(scheme);
            requestBuilder.scheme(scheme);
        ctx.setRequestUri(baseBuilder.build(), requestBuilder.build());
