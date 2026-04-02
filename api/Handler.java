package org.openhab.core.io.http;
 * Handler which is responsible for processing request and response.
 * This type is introduced to provide a unified way for injecting various logic on verification of requests sent via
 * HTTP. Handlers are called before servlet who will receive request, thus they can not mutate servlet response, but
 * they can generate its own response depending on actual needs.
 * Pay attention to error handling - as a proper executions might report exceptions, but fault path handled in
 * {@link #handleError(HttpServletRequest, HttpServletResponse, HandlerContext)} method must remain silent and take care
 * of all issues which might occur while handling error.
public interface Handler {
     * Returns priority of this handler.
     * Priority is any integer where 0 means earliest in the queue. The higher the number is, the later Handler will be
     * called.
     * @return Handler execution priority.
    int getPriority();
     * Method dedicated for processing incoming request and checking its contents.
     * @param request Http request.
     * @param response Http response.
     * @param context Handler execution context.
     * @throws Exception any error reported during processing will suspend execution of following handlers. Handlers
     *             will be then asked to handle error via
     *             {@link #handleError(HttpServletRequest, HttpServletResponse, HandlerContext)} method.
    void handle(HttpServletRequest request, HttpServletResponse response, HandlerContext context) throws Exception;
     * Method which is called only if any of {@link #handle(HttpServletRequest, HttpServletResponse, HandlerContext)}
     * method invocations thrown unexpected Exception.
     * Each handler might decide if it wants to handle error. If not then it should just let next handler in the queue
     * do its job via {@link HandlerContext#execute(HttpServletRequest, HttpServletResponse)} call.
    void handleError(HttpServletRequest request, HttpServletResponse response, HandlerContext context);
