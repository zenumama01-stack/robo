 * Extension of standard {@link HttpContext} interface which allows creation of "sub contexts".
 * These sub contexts are nothing else but custom resource locators which provide new files to host, but should not
 * influence overall processing logic of
 * {@link #handleSecurity(javax.servlet.http.HttpServletRequest, javax.servlet.http.HttpServletResponse)} and
 * {@link #getMimeType(String)}.
public interface WrappingHttpContext extends HttpContext {
     * Creates new http context which hosts resources from given bundle.
     * @param bundle Bundle with resources.
     * @return New context instance.
    HttpContext wrap(Bundle bundle);
