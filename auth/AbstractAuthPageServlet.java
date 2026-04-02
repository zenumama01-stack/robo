package org.openhab.core.io.http.auth.internal;
import java.io.UncheckedIOException;
import java.util.ResourceBundle;
import java.util.ResourceBundle.Control;
 * Abstract class for servlets to perform sensible operations requiring user authentication.
public abstract class AbstractAuthPageServlet extends HttpServlet {
    private static final long serialVersionUID = 5340598701104679840L;
    private static final String MESSAGES_BUNDLE_NAME = "messages";
    private final Logger logger = LoggerFactory.getLogger(AbstractAuthPageServlet.class);
    protected UserRegistry userRegistry;
    protected AuthenticationProvider authProvider;
    protected LocaleProvider localeProvider;
    protected @Nullable Instant lastAuthenticationFailure;
    protected int authenticationFailureCount = 0;
    protected Map<String, Instant> csrfTokens = new HashMap<>();
    protected String pageTemplate;
    protected AbstractAuthPageServlet(BundleContext bundleContext, @Reference UserRegistry userRegistry,
            @Reference AuthenticationProvider authProvider, @Reference LocaleProvider localeProvider) {
        this.authProvider = authProvider;
        pageTemplate = "";
        URL resource = bundleContext.getBundle().getResource("pages/authorize.html");
        if (resource != null) {
            try (InputStream stream = resource.openStream()) {
                pageTemplate = new String(stream.readAllBytes(), StandardCharsets.UTF_8);
                throw new UncheckedIOException("Cannot load page template", e);
    protected String getPageTemplate() {
        String template = pageTemplate;
        for (String[] replace : new String[][] { //
                { "{usernamePlaceholder}", "auth.placeholder.username" },
                { "{passwordPlaceholder}", "auth.placeholder.password" },
                { "{newPasswordPlaceholder}", "auth.placeholder.newpassword" },
                { "{repeatPasswordPlaceholder}", "auth.placeholder.repeatpassword" },
                { "{tokenNamePlaceholder}", "auth.placeholder.tokenname" },
                { "{tokenScopePlaceholder}", "auth.placeholder.tokenscope" },
                { "{returnButtonLabel}", "auth.button.return" } //
            template = template.replace(replace[0], getLocalizedMessage(replace[1]));
    protected abstract String getPageBody(Map<String, String[]> params, String message, boolean hideForm);
    protected abstract String getFormFields(Map<String, String[]> params);
    protected String addCsrfToken() {
        String csrfToken = UUID.randomUUID().toString().replace("-", "");
        csrfTokens.put(csrfToken, Instant.now());
        // remove old tokens (created earlier than 10 minutes ago) - this gives users a 10-minute window to sign in
        csrfTokens.entrySet().removeIf(e -> e.getValue().isBefore(Instant.now().minus(Duration.ofMinutes(10))));
        return csrfToken;
    protected void removeCsrfToken(String csrfToken) {
        csrfTokens.remove(csrfToken);
    protected User login(String username, String password) throws AuthenticationException {
        // Enforce a dynamic cooldown period after a failed authentication attempt: the number of
        // consecutive failures in seconds
        if (lastAuthenticationFailure != null && lastAuthenticationFailure
                .isAfter(Instant.now().minus(Duration.ofSeconds(authenticationFailureCount)))) {
            throw new AuthenticationException("Too many consecutive login attempts");
        // Authenticate the user with the supplied credentials
        UsernamePasswordCredentials credentials = new UsernamePasswordCredentials(username, password);
        Authentication auth = authProvider.authenticate(credentials);
        logger.debug("Login successful: {}", auth.getUsername());
        lastAuthenticationFailure = null;
        authenticationFailureCount = 0;
        User user = userRegistry.get(auth.getUsername());
        if (user == null) {
            throw new AuthenticationException("User not found");
    protected void processFailedLogin(HttpServletResponse resp, String remoteAddr, Map<String, String[]> params,
            @Nullable String message) throws IOException {
        lastAuthenticationFailure = Instant.now();
        authenticationFailureCount += 1;
        resp.setContentType("text/html;charset=UTF-8");
        logger.warn("Authentication failed from {}: {}", remoteAddr, message);
        resp.getWriter().append(getPageBody(params, getLocalizedMessage("auth.login.fail"), false));
        resp.getWriter().close();
    protected String getLocalizedMessage(String messageKey) {
        ResourceBundle rb = ResourceBundle.getBundle(MESSAGES_BUNDLE_NAME, localeProvider.getLocale(),
                Control.getNoFallbackControl(Control.FORMAT_PROPERTIES));
        return rb.getString(messageKey);
