 * Http context which does nothing but lets the delegate do its job.
class DelegatingHttpContext implements HttpContext {
    private final HttpContext delegate;
    public DelegatingHttpContext(HttpContext delegate) {
    public boolean handleSecurity(HttpServletRequest request, HttpServletResponse response) throws IOException {
        return delegate.handleSecurity(request, response);
        return delegate.getResource(name);
    public String getMimeType(String name) {
        return delegate.getMimeType(name);
