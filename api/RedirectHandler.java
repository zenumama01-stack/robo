 * Handler located after authentication which redirect client to page from which he started authentication process.
public class RedirectHandler implements Handler {
        return HandlerPriorities.AUTHENTICATION + 10;
    public void handle(HttpServletRequest request, HttpServletResponse response, HandlerContext context) {
        Optional<Authentication> authhentication = Optional
                .ofNullable(request.getAttribute(Authentication.class.getName()))
                .filter(Authentication.class::isInstance).map(Authentication.class::cast);
        Optional<String> redirect = Optional
                .ofNullable(request.getParameter(AuthenticationHandler.REDIRECT_PARAM_NAME));
        if (authhentication.isPresent() && redirect.isPresent()) {
            response.setHeader("Location", redirect.get());
