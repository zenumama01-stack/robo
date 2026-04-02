import org.openhab.core.auth.AuthenticatedUser;
import org.openhab.core.auth.PendingToken;
 * A servlet serving the authorization page part of the OAuth2 authorization code flow.
 * The page can register the first administrator account when there are no users yet in the {@link UserRegistry}, and
 * authenticates the user otherwise. It also presents the scope that is about to be granted to the client, so the user
 * can review what kind of access is being authorized. If successful, it redirects the client back to the URI which was
 * specified and creates an authorization code stored for later in the user's profile.
@Component(immediate = true, service = Servlet.class)
@HttpWhiteboardServletName(AuthorizePageServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(AuthorizePageServlet.SERVLET_PATH + "/*")
public class AuthorizePageServlet extends AbstractAuthPageServlet {
    public static final String SERVLET_PATH = "/auth";
    private static final long serialVersionUID = 5340598701104679843L;
    private final Logger logger = LoggerFactory.getLogger(AuthorizePageServlet.class);
    public AuthorizePageServlet(BundleContext bundleContext, @Reference UserRegistry userRegistry,
        super(bundleContext, userRegistry, authProvider, localeProvider);
        Map<String, String[]> params = req.getParameterMap();
            String message;
            String scope = params.containsKey("scope") ? params.get("scope")[0] : "";
            String clientId = params.containsKey("client_id") ? params.get("client_id")[0] : "";
            // Basic sanity check
            if (scope.contains("<") || clientId.contains("<")) {
                throw new IllegalArgumentException("invalid_request");
            if (isSignupMode()) {
                message = getLocalizedMessage("auth.createaccount.prompt");
                message = String.format(getLocalizedMessage("auth.login.prompt"), scope, clientId);
            resp.getWriter().append(getPageBody(params, message, false));
            resp.setContentType("text/plain;charset=UTF-8");
            resp.getWriter().append(e.getMessage());
    protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
            if (!params.containsKey("username")) {
                throw new AuthenticationException("no username");
            if (!params.containsKey("password")) {
                throw new AuthenticationException("no password");
            if (!params.containsKey("csrf_token") || !csrfTokens.containsKey(params.get("csrf_token")[0])) {
                throw new AuthenticationException("CSRF check failed");
            if (!params.containsKey("redirect_uri")) {
            if (!params.containsKey("response_type")) {
                throw new IllegalArgumentException("unsupported_response_type");
            if (!params.containsKey("client_id")) {
                throw new IllegalArgumentException("unauthorized_client");
            if (!params.containsKey("scope")) {
                throw new IllegalArgumentException("invalid_scope");
            removeCsrfToken(params.get("csrf_token")[0]);
            String baseRedirectUri = params.get("redirect_uri")[0];
            String responseType = params.get("response_type")[0];
            String clientId = params.get("client_id")[0];
            String scope = params.get("scope")[0];
            if (!"code".equals(responseType)) {
                throw new AuthenticationException("unsupported_response_type");
            if (!clientId.equals(baseRedirectUri)) {
            String username = params.get("username")[0];
            String password = params.get("password")[0];
            User user;
                // Create a first administrator account with the supplied credentials
                // first verify the password confirmation and bail out if necessary
                if (!params.containsKey("password_repeat") || !password.equals(params.get("password_repeat")[0])) {
                    resp.getWriter()
                            .append(getPageBody(params, getLocalizedMessage("auth.password.confirm.fail"), false));
                user = userRegistry.register(username, password, Set.of(Role.ADMIN));
                logger.info("First user account created: {}", username);
                user = login(username, password);
            String authorizationCode = UUID.randomUUID().toString().replace("-", "");
            if (user instanceof AuthenticatedUser authenticatedUser) {
                String codeChallenge = params.containsKey("code_challenge") ? params.get("code_challenge")[0] : null;
                String codeChallengeMethod = params.containsKey("code_challenge_method")
                        ? params.get("code_challenge_method")[0]
                PendingToken pendingToken = new PendingToken(authorizationCode, clientId, baseRedirectUri, scope,
                        codeChallenge, codeChallengeMethod);
                authenticatedUser.setPendingToken(pendingToken);
                userRegistry.update(authenticatedUser);
            String state = params.containsKey("state") ? params.get("state")[0] : null;
            resp.addHeader(HttpHeaders.LOCATION, getRedirectUri(baseRedirectUri, authorizationCode, null, state));
            resp.setStatus(HttpStatus.MOVED_TEMPORARILY_302);
            processFailedLogin(resp, req.getRemoteAddr(), params, e.getMessage());
            String baseRedirectUri = params.containsKey("redirect_uri") ? params.get("redirect_uri")[0] : null;
            if (baseRedirectUri != null) {
                resp.addHeader(HttpHeaders.LOCATION, getRedirectUri(baseRedirectUri, null, e.getMessage(), state));
    protected String getPageBody(Map<String, String[]> params, String message, boolean hideForm) {
        String responseBody = getPageTemplate().replace("{form_fields}", getFormFields(params));
        String repeatPasswordFieldType = isSignupMode() ? "password" : "hidden";
        String buttonLabel = getLocalizedMessage(isSignupMode() ? "auth.button.createaccount" : "auth.button.signin");
        responseBody = responseBody.replace("{message}", message);
        responseBody = responseBody.replace("{formAction}", "/auth");
        responseBody = responseBody.replace("{formClass}", "show");
        responseBody = responseBody.replace("{repeatPasswordFieldType}", repeatPasswordFieldType);
        responseBody = responseBody.replace("{newPasswordFieldType}", "hidden");
        responseBody = responseBody.replace("{tokenNameFieldType}", "hidden");
        responseBody = responseBody.replace("{tokenScopeFieldType}", "hidden");
        responseBody = responseBody.replace("{buttonLabel}", buttonLabel);
        responseBody = responseBody.replace("{resultClass}", "");
        return responseBody;
    protected String getFormFields(Map<String, String[]> params) {
        String hiddenFormFields = "";
        String csrfToken = addCsrfToken();
        String redirectUri = params.get("redirect_uri")[0];
        hiddenFormFields += "<input type=\"hidden\" name=\"csrf_token\" value=\"" + csrfToken + "\">";
        hiddenFormFields += "<input type=\"hidden\" name=\"redirect_uri\" value=\"" + redirectUri + "\">";
        hiddenFormFields += "<input type=\"hidden\" name=\"response_type\" value=\"" + responseType + "\">";
        hiddenFormFields += "<input type=\"hidden\" name=\"client_id\" value=\"" + clientId + "\">";
        hiddenFormFields += "<input type=\"hidden\" name=\"scope\" value=\"" + scope + "\">";
            hiddenFormFields += "<input type=\"hidden\" name=\"state\" value=\"" + state + "\">";
        if (codeChallenge != null && codeChallengeMethod != null) {
            hiddenFormFields += "<input type=\"hidden\" name=\"code_challenge\" value=\"" + codeChallenge + "\">";
            hiddenFormFields += "<input type=\"hidden\" name=\"code_challenge_method\" value=\"" + codeChallengeMethod
                    + "\">";
        return hiddenFormFields;
    private String getRedirectUri(String baseRedirectUri, @Nullable String authorizationCode, @Nullable String error,
            @Nullable String state) {
        String redirectUri = baseRedirectUri;
        if (authorizationCode != null) {
            redirectUri += "?code=" + authorizationCode;
        } else if (error != null) {
            redirectUri += "?error=" + error;
            redirectUri += "&state=" + state;
        return redirectUri;
    private boolean isSignupMode() {
        return userRegistry.getAll().isEmpty();
