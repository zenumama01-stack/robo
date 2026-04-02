 * Create {@link HttpContext} instances when registering servlets, resources or filters using the
 * {@link HttpService#registerServlet} and corresponding methods.
public interface HttpContextFactoryService {
     * Creates an {@link HttpContext} according to the OSGi specification of
     * {@link HttpService#createDefaultHttpContext()}.
     * @param bundle the bundle which will be used by this {@link HttpContext} to resolve resources.
     * @return the {@link HttpContext} for the given bundle.
    HttpContext createDefaultHttpContext(Bundle bundle);
