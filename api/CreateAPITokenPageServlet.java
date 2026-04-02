 * A servlet serving a page allowing users to create a new API token, after confirming their identity by signing in.
@HttpWhiteboardServletName(CreateAPITokenPageServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(CreateAPITokenPageServlet.SERVLET_PATH + "/*")
public class CreateAPITokenPageServlet extends AbstractAuthPageServlet {
    public static final String SERVLET_PATH = "/createApiToken";
    public CreateAPITokenPageServlet(BundleContext bundleContext, @Reference UserRegistry userRegistry,
            String message = getLocalizedMessage("auth.createapitoken.prompt");
            if (!params.containsKey("token_name")) {
                throw new AuthenticationException("no token name");
            String tokenName = params.get("token_name")[0];
            String tokenScope = params.get("token_scope")[0];
            String newApiToken;
                if (authenticatedUser.getApiTokens().stream()
                        .anyMatch(apiToken -> apiToken.getName().equals(tokenName))) {
                    resp.getWriter().append(
                            getPageBody(params, getLocalizedMessage("auth.createapitoken.name.unique.fail"), false));
                if (!tokenName.matches("[a-zA-Z0-9]+")) {
                            getPageBody(params, getLocalizedMessage("auth.createapitoken.name.format.fail"), false));
                newApiToken = userRegistry.addUserApiToken(user, tokenName, tokenScope);
                throw new AuthenticationException("User authentication is not managed by openHAB");
            String resultMessage = getLocalizedMessage("auth.createapitoken.success") + "<br /><br /><code>"
                    + newApiToken + "</code>";
            resultMessage += "<br /><br /><small>" + getLocalizedMessage("auth.createapitoken.success.footer")
                    + "</small>";
            resp.getWriter().append(getResultPageBody(params, resultMessage));
        String buttonLabel = getLocalizedMessage("auth.button.createapitoken");
        responseBody = responseBody.replace("{formAction}", "/createApiToken");
        responseBody = responseBody.replace("{repeatPasswordFieldType}", "hidden");
        responseBody = responseBody.replace("{tokenNameFieldType}", "text");
        responseBody = responseBody.replace("{tokenScopeFieldType}", "text");
