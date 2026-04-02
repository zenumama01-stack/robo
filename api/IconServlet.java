import org.eclipse.jetty.io.EofException;
import org.osgi.service.http.whiteboard.propertytypes.HttpWhiteboardServletAsyncSupported;
 * Registers a servlet that serves icons through {@link IconProvider}s.
@Component(service = Servlet.class)
@HttpWhiteboardServletAsyncSupported(asyncSupported = true)
@HttpWhiteboardServletName(IconServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(IconServlet.SERVLET_PATH + "/*")
public class IconServlet extends HttpServlet {
    private static final long serialVersionUID = 2880642275858634578L;
    private final Logger logger = LoggerFactory.getLogger(IconServlet.class);
    static final String SERVLET_PATH = "/icon";
    static final String PARAM_ICONSET = "iconset";
    static final String PARAM_FORMAT = "format";
    static final String PARAM_ANY_FORMAT = "anyFormat";
    static final String PARAM_STATE = "state";
    protected String defaultIconSetId = "classic";
    private final List<IconProvider> iconProvider = new ArrayList<>();
    public void addIconProvider(IconProvider iconProvider) {
        this.iconProvider.add(iconProvider);
    public void removeIconProvider(IconProvider iconProvider) {
        this.iconProvider.remove(iconProvider);
        Object iconSetId = config.get("default");
        if (iconSetId instanceof String string) {
            defaultIconSetId = string;
        String category = getCategory(req);
        if (category.isEmpty()) {
            logger.debug("URI must start with '{}' but is '{}'", SERVLET_PATH, req.getRequestURI());
            resp.sendError(400);
        String state = req.getParameter(PARAM_STATE);
        String iconSetId = getIconSetId(req);
        Format format = getFormat(req);
        Format otherFormat = null;
        if ("true".equalsIgnoreCase(req.getParameter(PARAM_ANY_FORMAT))) {
            otherFormat = (format == Format.PNG) ? Format.SVG : Format.PNG;
        IconProvider provider = getIconProvider(category, iconSetId, format);
        if (otherFormat != null) {
            IconProvider provider2 = getIconProvider(category, iconSetId, otherFormat);
            if (provider2 != null) {
                    provider = provider2;
                    format = otherFormat;
                } else if (!provider2.equals(provider)) {
                    Integer prio = provider.hasIcon(category, iconSetId, format);
                    Integer prio2 = provider2.hasIcon(category, iconSetId, otherFormat);
                    if ((prio != null && prio2 != null && prio < prio2) || (prio == null && prio2 != null)) {
            logger.debug("Requested icon category {} provided by no icon provider", category);
            resp.sendError(404);
        try (InputStream is = provider.getIcon(category, iconSetId, state, format)) {
            resp.setContentType(Format.SVG.equals(format) ? "image/svg+xml" : "image/png");
            resp.setHeader("Cache-Control", "max-age=31536000");
            is.transferTo(resp.getOutputStream());
            if (resp.isCommitted() && e instanceof EofException) {
                logger.debug("Client {} disconnected while sending the icon byte stream as response",
                        req.getRemoteAddr());
                // don't send a response as client is already gone
            logger.error("Failed sending the icon byte stream as a response: {}", e.getMessage());
            resp.sendError(500, e.getMessage());
    private String substringAfterLast(@Nullable String str, String separator) {
        if (str == null) {
    private String getCategory(HttpServletRequest req) {
        String category = substringAfterLast(req.getRequestURI(), "/");
        return substringBeforeLast(category, ".");
    private Format getFormat(HttpServletRequest req) {
        String format = req.getParameter(PARAM_FORMAT);
        if (format == null) {
                logger.debug("null request URI in HTTP request - falling back to PNG");
                return Format.PNG;
            String filename = substringAfterLast(requestURI, "/");
            format = substringAfterLast(filename, ".");
            return Format.valueOf(format.toUpperCase());
            logger.debug("unknown format '{}' in HTTP request - falling back to PNG", format);
    private String getIconSetId(HttpServletRequest req) {
        String iconSetId = req.getParameter(PARAM_ICONSET);
        if (iconSetId == null || iconSetId.isEmpty()) {
            return defaultIconSetId;
            return iconSetId;
    private @Nullable IconProvider getIconProvider(String category, String iconSetId, Format format) {
        IconProvider topProvider = null;
        int maxPrio = Integer.MIN_VALUE;
        for (IconProvider provider : iconProvider) {
            if (prio != null && prio > maxPrio) {
                maxPrio = prio;
                topProvider = provider;
        return topProvider;
