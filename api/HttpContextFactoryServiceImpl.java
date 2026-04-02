import org.openhab.core.io.http.HttpContextFactoryService;
import org.openhab.core.io.http.WrappingHttpContext;
 * The resulting {@link HttpContext} complies with the OSGi specification when it comes to resource resolving.
@Component(service = HttpContextFactoryService.class)
public class HttpContextFactoryServiceImpl implements HttpContextFactoryService {
    private WrappingHttpContext httpContext;
    public HttpContext createDefaultHttpContext(Bundle bundle) {
        return httpContext.wrap(bundle);
    public void setHttpContext(WrappingHttpContext httpContext) {
        this.httpContext = httpContext;
    public void unsetHttpContext(WrappingHttpContext httpContext) {
        this.httpContext = null;
