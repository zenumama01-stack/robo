 * Dedicated kind of delegate handler which provides error handling.
 * Each exception is set back on HandlerContext allowing HandlerChain to process it. When there is already error set in
 * current processing pipeline thrown exception is logged and ignored.
public class CatchHandler implements Handler {
    private final Logger logger = LoggerFactory.getLogger(CatchHandler.class);
    private final Handler delegate;
    public CatchHandler(Handler delegate) {
        return delegate.getPriority();
            delegate.handle(request, response, context);
            if (!context.hasError()) {
                context.error(e);
                logger.error("Could not handle exception thrown by delegate handler {}", delegate, e);
        delegate.handleError(request, response, context);
