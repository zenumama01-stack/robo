import java.io.PrintWriter;
import org.openhab.core.auth.AuthenticationManager;
import org.openhab.core.io.http.Handler;
import org.openhab.core.io.http.HandlerContext;
import org.openhab.core.io.http.HandlerPriorities;
import org.openhab.core.io.http.auth.CredentialsExtractor;
 * Request handler which allows to verify authentication.
@Component(configurationPid = "org.openhab.auth")
public class AuthenticationHandler implements Handler {
    private static final String AUTHENTICATION_ENABLED = "enabled";
    private static final String AUTHENTICATION_ENDPOINT = "loginUri";
    private static final String DEFAULT_LOGIN_URI = "/login";
    static final String REDIRECT_PARAM_NAME = "redirect";
    private final Logger logger = LoggerFactory.getLogger(AuthenticationHandler.class);
    private final List<CredentialsExtractor<HttpServletRequest>> extractors = new CopyOnWriteArrayList<>();
    private AuthenticationManager authenticationManager;
    // configuration properties
    private boolean enabled = false;
    private String loginUri = DEFAULT_LOGIN_URI;
    public int getPriority() {
        return HandlerPriorities.AUTHENTICATION;
    public void handle(final HttpServletRequest request, final HttpServletResponse response,
            final HandlerContext context) throws Exception {
        String requestUri = request.getRequestURI();
        if (this.enabled && isSecured(requestUri, request.getMethod())) {
            if (authenticationManager == null) {
                throw new AuthenticationException("Failed to authenticate request.");
            int found = 0, failed = 0;
            for (CredentialsExtractor<HttpServletRequest> extractor : extractors) {
                Optional<Credentials> extracted = extractor.retrieveCredentials(request);
                if (extracted.isPresent()) {
                    found++;
                    Credentials credentials = extracted.get();
                        Authentication authentication = authenticationManager.authenticate(credentials);
                        request.setAttribute(Authentication.class.getName(), authentication);
                        context.execute(request, response);
                            logger.debug("Failed to authenticate using credentials {}", credentials, e);
                            logger.info("Failed to authenticate using credentials {}", credentials);
            throw new AuthenticationException("Could not authenticate request. Found " + found
                    + " credentials in request out of which " + failed + " were invalid");
    public void handleError(HttpServletRequest request, HttpServletResponse response, HandlerContext context) {
        Object error = request.getAttribute(HandlerContext.ERROR_ATTRIBUTE);
        if (response.getStatus() == 403 || response.getStatus() == 401) {
            // already handled
        if (error instanceof AuthenticationException) {
            // force client redirect
            String redirectUri = loginUri + "?" + REDIRECT_PARAM_NAME + "=" + request.getRequestURI();
            response.setHeader("Location", redirectUri);
                PrintWriter writer = response.getWriter();
                writer.println("<html><head>");
                writer.println("<meta http-equiv=\"refresh\" content=\"0; url=" + redirectUri + "\" />");
                writer.println("</head><body>Redirecting to login page</body></html>");
                writer.flush();
                logger.warn("Couldn't generate or send client response", e);
            // let other handler handle error
    private boolean isSecured(String requestUri, String method) {
        if (requestUri.startsWith(loginUri) && !"post".equalsIgnoreCase(method)) {
        // TODO add decision logic so not all URIs gets secured but only these which are told to be
    void activate(Map<String, Object> properties) {
    void modified(Map<String, Object> properties) {
        Object authenticationEnabled = properties.get(AUTHENTICATION_ENABLED);
        if (authenticationEnabled != null) {
            this.enabled = Boolean.parseBoolean(authenticationEnabled.toString());
        Object loginUri = properties.get(AUTHENTICATION_ENDPOINT);
        if (loginUri instanceof String string) {
            this.loginUri = string;
    public void setAuthenticationManager(AuthenticationManager authenticationManager) {
        this.authenticationManager = authenticationManager;
    public void unsetAuthenticationManager(AuthenticationManager authenticationManager) {
        this.authenticationManager = null;
    @Reference(cardinality = ReferenceCardinality.MULTIPLE, policy = ReferencePolicy.DYNAMIC, target = "(context=javax.servlet.http.HttpServletRequest)")
    public void addCredentialsExtractor(CredentialsExtractor<HttpServletRequest> extractor) {
        this.extractors.add(extractor);
    public void removeCredentialsExtractor(CredentialsExtractor<HttpServletRequest> extractor) {
        this.extractors.remove(extractor);
