import java.util.ArrayDeque;
import org.osgi.service.http.whiteboard.propertytypes.HttpWhiteboardContext;
 * Default HTTP context implementation which groups all openHAB related HTTP elements into one logical application.
 * Additionally to the standard HTTP context, this class provides its own implementation of the
 * {@link #handleSecurity(HttpServletRequest, HttpServletResponse)} method which is based on the injected list of
 * {@link Handler}s.
@Component(service = { HttpContext.class, WrappingHttpContext.class })
@HttpWhiteboardContext(path = "/", name = "oh-dfl-http-ctx")
public class OpenHABHttpContext implements WrappingHttpContext {
     * Sorted list of handlers, where handler with priority 0 is first.
    private final List<Handler> handlers = new CopyOnWriteArrayList<>();
        Deque<Handler> queue = new ArrayDeque<>(handlers);
        DefaultHandlerContext handlerContext = new DefaultHandlerContext(queue);
        handlerContext.execute(request, response);
        return !handlerContext.hasError();
    public HttpContext wrap(Bundle bundle) {
        return new BundleHttpContext(this, bundle);
    public void addHandler(Handler handler) {
        this.handlers.add(handler);
        handlers.sort(Comparator.comparingInt(Handler::getPriority));
    public void removeHandler(Handler handler) {
        this.handlers.remove(handler);
