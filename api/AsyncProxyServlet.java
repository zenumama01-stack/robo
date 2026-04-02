package org.openhab.core.ui.internal.proxy;
 * This version of the proxy servlet uses asynchronous I/O and request processing, and is based on Jetty's proxy
 * servlets. It depends on Servlet API 3.0 or later.
 * @author John Cocula - Initial contribution
public class AsyncProxyServlet extends org.eclipse.jetty.proxy.AsyncProxyServlet {
    private static final long serialVersionUID = -4716754591953017795L;
    private final ProxyServletService service;
    AsyncProxyServlet(ProxyServletService service) {
    public String getServletInfo() {
        return "Proxy (async)";
     * Override <code>newHttpClient</code> so we can proxy to HTTPS URIs.
    protected HttpClient newHttpClient() {
        return new HttpClient(new SslContextFactory.Client());
    protected void sendProxyRequest(HttpServletRequest clientRequest, HttpServletResponse proxyResponse,
            Request proxyRequest) {
        if (service.proxyingVideoWidget(clientRequest)) {
            // We disable the timeout for video
            proxyRequest.timeout(0, TimeUnit.MILLISECONDS);
            // We request the browser to not cache the video
            proxyResponse.setHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            proxyResponse.setHeader("Pragma", "no-cache");
            proxyResponse.setHeader("Expires", "0");
        super.sendProxyRequest(clientRequest, proxyResponse, proxyRequest);
     * Add Basic Authentication header to request if user and password are specified in URI.
    protected void copyRequestHeaders(HttpServletRequest clientRequest, Request proxyRequest) {
        super.copyRequestHeaders(clientRequest, proxyRequest);
        service.maybeAppendAuthHeader(service.uriFromRequest(clientRequest), proxyRequest);
    protected String rewriteTarget(HttpServletRequest request) {
        return Objects.toString(service.uriFromRequest(request), null);
    protected void onProxyRewriteFailed(HttpServletRequest request, HttpServletResponse response) {
        service.sendError(request, response);
