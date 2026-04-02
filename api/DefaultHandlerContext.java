import java.util.Deque;
 * Default implementation of handler context which is responsible for pre-processing of request and checking if access
 * to servlet is granted or not.
 * Handler context should be initialized with static list of handlers who do not change during handlers execution.
 * Context is responsible for tracking current position of invocation chain and reseting it in order to handle errors
 * if any of processors fails to do its job.
public class DefaultHandlerContext implements HandlerContext {
    private final Logger logger = LoggerFactory.getLogger(DefaultHandlerContext.class);
    private final Deque<Handler> handlers;
    private Iterator<Handler> cursor;
    private Exception error;
    public DefaultHandlerContext(Deque<Handler> handlers) {
        this.handlers = handlers;
        this.cursor = handlers.iterator();
    public void execute(HttpServletRequest request, HttpServletResponse response) {
        if (cursor.hasNext()) {
            boolean hasError = hasError();
            Handler handler = new CatchHandler(cursor.next());
            if (hasError) {
                handler.handleError(request, response, this);
                    handler.handle(request, response, this);
                    if (!hasError()) {
                        error(e);
                        // this is major failure which couldn't be handled by catch handler, however we already
                        // processing a fault, there is nothing we can do about that beyond marking occurrence of error.
                        // It is definitely a fatality
                        logger.error("Could not handle request", e);
            if (!hasError && hasError()) {
                // we didn't have an error and we have it now, meaning a current handler reported issue.
                // so here reset cursor and restart execution forcing handleError method execution in reverse order.
                request.setAttribute(ERROR_ATTRIBUTE, error);
                cursor = handlers.descendingIterator();
                execute(request, response);
    public boolean hasError() {
        return error != null;
    public void error(Exception error) {
