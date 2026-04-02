 * Handler context represents a present state of all handlers placed in execution chain.
 * There are two basic operations located in this type - first allows to continue execution and let next handler be
 * called. Second is intended to break the chain and force handlers to process error and generate error response.
 * When Handler decide to not call context by delegating further handler to get called via
 * {@link #execute(HttpServletRequest, HttpServletResponse)} nor {@link #error(Exception)} then chain is stopped.
 * By this simple way handlers can decide to hold processing and generate own response.
public interface HandlerContext {
    String ERROR_ATTRIBUTE = "handler.error";
     * Delegate execution to next handler in the chain, if available.
     * When current handler is last in processing queue then nothing happens, execution chain returns to its caller.
     * @param request Request.
     * @param response Response.
    void execute(HttpServletRequest request, HttpServletResponse response);
     * Signal that an error occurred during handling of request.
     * Call to this method will break normal execution chain and force handling of error.
    void error(Exception error);
     * Checks if has any errors occurred while handling request.
     * @return True if an exception occurred while handling request.
    boolean hasError();
