 * A servlet serving a page allowing users to change their password, after confirming their identity by signing in.
@HttpWhiteboardServletName(ChangePasswordPageServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(ChangePasswordPageServlet.SERVLET_PATH + "/*")
public class ChangePasswordPageServlet extends AbstractAuthPageServlet {
    public static final String SERVLET_PATH = "/changePassword";
    public ChangePasswordPageServlet(BundleContext bundleContext, @Reference UserRegistry userRegistry,
            String message = "";
            if (!params.containsKey("new_password")) {
                throw new AuthenticationException("no new password");
            String newPassword = params.get("new_password")[0];
            if (!params.containsKey("password_repeat") || !newPassword.equals(params.get("password_repeat")[0])) {
                resp.getWriter().append(getPageBody(params, getLocalizedMessage("auth.password.confirm.fail"), false));
            User user = login(username, password);
            if (user instanceof ManagedUser) {
                userRegistry.changePassword(user, newPassword);
                throw new AuthenticationException("User is not managed");
            resp.getWriter().append(getResultPageBody(params, getLocalizedMessage("auth.changepassword.success")));
        String buttonLabel = getLocalizedMessage("auth.button.changepassword");
        responseBody = responseBody.replace("{formAction}", "/changePassword");
        responseBody = responseBody.replace("{formClass}", hideForm ? "hide" : "show");
        responseBody = responseBody.replace("{repeatPasswordFieldType}", "password");
        responseBody = responseBody.replace("{newPasswordFieldType}", "password");
    protected String getResultPageBody(Map<String, String[]> params, String message) {
        String responseBody = getPageTemplate().replace("{form_fields}", "");
        responseBody = responseBody.replace("{formClass}", "hide");
        responseBody = responseBody.replace("{resultClass}", "Password");
