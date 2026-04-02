package org.openhab.core.ui.internal.chart;
import java.time.Period;
import java.time.temporal.TemporalAmount;
import javax.imageio.ImageIO;
import javax.imageio.stream.ImageOutputStream;
import javax.servlet.ServletConfig;
import org.openhab.core.ui.chart.ChartProvider;
 * This servlet generates time-series charts for a given set of items. It
 * accepts the following HTTP parameters:
 * <li>w: width in pixels of image to generate</li>
 * <li>h: height in pixels of image to generate</li>
 * <li>period: the time span for the x-axis. Value can be h,4h,8h,12h,D,3D,W,2W,M,2M,4M,Y</li>
 * <li>items: A comma separated list of item names to display</li>
 * <li>groups: A comma separated list of group names, whose members should be displayed</li>
 * <li>service: The persistence service name. If not supplied the first service found will be used.</li>
 * <li>theme: The chart theme to use. If not supplied the chart provider uses a default theme.</li>
 * <li>dpi: The DPI (dots per inch) value. If not supplied, a default is used.</code></li>
 * <li>legend: Show the legend? If not supplied, the ChartProvider should make his own decision.</li>
 * @author Laurent Garnier - Extend support to ISO8601 format for chart period parameter
 * @author Laurent Garnier - Extend support to past and future for chart period parameter
@Component(immediate = true, service = { ChartServlet.class, Servlet.class }, configurationPid = "org.openhab.chart", //
        property = Constants.SERVICE_PID + "=org.openhab.chart")
@ConfigurableService(category = "system", label = "Charts", description_uri = ChartServlet.CONFIG_URI)
@HttpWhiteboardServletName(ChartServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(ChartServlet.SERVLET_PATH + "/*")
public class ChartServlet extends HttpServlet {
    private static final long serialVersionUID = 7700873790924746422L;
    protected static final String CONFIG_URI = "system:chart";
    private static final int CHART_HEIGHT = 240;
    private static final int CHART_WIDTH = 480;
    private static final String DATE_FORMAT = "yyyyMMddHHmm";
    private static final DateTimeFormatter FORMATTER = DateTimeFormatter.ofPattern(DATE_FORMAT);
    private final Logger logger = LoggerFactory.getLogger(ChartServlet.class);
    private String providerName = "default";
    private int defaultHeight = CHART_HEIGHT;
    private int defaultWidth = CHART_WIDTH;
    private double scale = 1.0;
    private int maxWidth = -1;
    // The URI of this servlet
    public static final String SERVLET_PATH = "/chart";
    protected static final Duration DEFAULT_PERIOD = Duration.ofDays(1);
    protected static final Map<String, ChartProvider> CHART_PROVIDERS = new ConcurrentHashMap<>();
    public ChartServlet(final @Reference TimeZoneProvider timeZoneProvider) {
    public void addChartProvider(ChartProvider provider) {
        CHART_PROVIDERS.put(provider.getName(), provider);
    public void removeChartProvider(ChartProvider provider) {
        CHART_PROVIDERS.remove(provider.getName());
    public static Map<String, ChartProvider> getChartProviders() {
        return CHART_PROVIDERS;
    protected void modified(@Nullable Map<String, Object> config) {
     * Handle the initial or a changed configuration.
     * @param config the configuration
    private void applyConfig(@Nullable Map<String, Object> config) {
        final String providerNameString = Objects.toString(config.get("provider"), null);
        if (providerNameString != null) {
            providerName = providerNameString;
        final String defaultHeightString = Objects.toString(config.get("defaultHeight"), null);
        if (defaultHeightString != null) {
                defaultHeight = Integer.parseInt(defaultHeightString);
                logger.warn("'{}' is not a valid integer value for the defaultHeight parameter.", defaultHeightString);
        final String defaultWidthString = Objects.toString(config.get("defaultWidth"), null);
        if (defaultWidthString != null) {
                defaultWidth = Integer.parseInt(defaultWidthString);
                logger.warn("'{}' is not a valid integer value for the defaultWidth parameter.", defaultWidthString);
        final String scaleString = Objects.toString(config.get("scale"), null);
        if (scaleString != null) {
                scale = Double.parseDouble(scaleString);
                // Set scale to normal if the custom value is unrealistically low
                if (scale < 0.1) {
                    scale = 1.0;
                logger.warn("'{}' is not a valid number value for the scale parameter.", scaleString);
        final String maxWidthString = Objects.toString(config.get("maxWidth"), null);
        if (maxWidthString != null) {
                maxWidth = Integer.parseInt(maxWidthString);
                logger.warn("'{}' is not a valid integer value for the maxWidth parameter.", maxWidthString);
    protected void doGet(HttpServletRequest req, HttpServletResponse res) throws ServletException, IOException {
        logger.debug("Received incoming chart request: {}", req);
        int width = defaultWidth;
        String w = req.getParameter("w");
        if (w != null) {
                width = Integer.parseInt(w);
                logger.debug("Ignoring invalid value '{}' for HTTP request parameter 'w'", w);
        int height = defaultHeight;
        String h = req.getParameter("h");
                Double d = Double.parseDouble(h) * scale;
                height = d.intValue();
                logger.debug("Ignoring invalid value '{}' for HTTP request parameter 'h'", h);
        String periodParam = req.getParameter("period");
        String timeBeginParam = req.getParameter("begin");
        String timeEndParam = req.getParameter("end");
        // To avoid ambiguity you are not allowed to specify period, begin and end time at the same time.
        if (periodParam != null && timeBeginParam != null && timeEndParam != null) {
            res.sendError(HttpServletResponse.SC_BAD_REQUEST,
                    "Do not specify the three parameters period, begin and end at the same time.");
        ZonedDateTime timeBegin = null;
        ZonedDateTime timeEnd = null;
        if (timeBeginParam != null) {
                timeBegin = LocalDateTime.parse(timeBeginParam, FORMATTER).atZone(timeZoneProvider.getTimeZone());
                        "Begin and end must have this format: " + DATE_FORMAT + ".");
        if (timeEndParam != null) {
                timeEnd = LocalDateTime.parse(timeEndParam, FORMATTER).atZone(timeZoneProvider.getTimeZone());
        PeriodPastFuture period = getPeriodPastFuture(periodParam);
        PeriodBeginEnd beginEnd = getPeriodBeginEnd(timeBegin, timeEnd, period,
                ZonedDateTime.now(timeZoneProvider.getTimeZone()));
        if (beginEnd.begin() != null && beginEnd.end() != null && beginEnd.end().isBefore(beginEnd.begin())) {
            res.sendError(HttpServletResponse.SC_BAD_REQUEST, "The end is before the begin.");
        // If a persistence service is specified, find the provider
        String serviceName = req.getParameter("service");
        ChartProvider provider = getChartProviders().get(providerName);
            res.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, "Could not get chart provider.");
        // Read out the parameter 'dpi'
        Integer dpi = null;
        String dpiString = req.getParameter("dpi");
        if (dpiString != null) {
                dpi = Integer.valueOf(dpiString);
                res.sendError(HttpServletResponse.SC_BAD_REQUEST, "dpi parameter is invalid");
            if (dpi <= 0) {
                res.sendError(HttpServletResponse.SC_BAD_REQUEST, "dpi parameter is <= 0");
        String yAxisDecimalPattern = req.getParameter("yAxisDecimalPattern");
        String interpolation = req.getParameter("interpolation");
        // Read out parameter 'legend'
        String legendParam = req.getParameter("legend");
        Boolean legend = null;
        if (legendParam != null) {
            legend = Boolean.valueOf(legendParam);
        if (maxWidth > 0 && width > maxWidth) {
            height = Math.round((float) height / (float) width * maxWidth);
            if (dpi != null) {
                dpi = Math.round((float) dpi / (float) width * maxWidth);
            width = maxWidth;
        logger.debug("chart building with width {} height {} dpi {}", width, height, dpi);
            BufferedImage chart = provider.createChart(serviceName, req.getParameter("theme"), beginEnd.begin(),
                    beginEnd.end(), height, width, req.getParameter("items"), req.getParameter("groups"), dpi,
                    yAxisDecimalPattern, interpolation, legend);
            // Set the content type to that provided by the chart provider
            res.setContentType("image/" + provider.getChartType());
            try (ImageOutputStream imageOutputStream = ImageIO.createImageOutputStream(res.getOutputStream())) {
                ImageIO.write(chart, provider.getChartType().toString(), imageOutputStream);
                logger.debug("Chart successfully generated and written to the response.");
            res.sendError(HttpServletResponse.SC_BAD_REQUEST, e.getMessage());
            logger.warn("Illegal argument in chart: {}", e.getMessage());
            res.sendError(HttpServletResponse.SC_BAD_REQUEST, "Illegal argument in chart: " + e.getMessage());
            // this can happen if the request is terminated while the image is streamed, see
            // https://github.com/openhab/openhab-distro/issues/684
            logger.debug("Failed writing image to response stream", e);
            logger.warn("Chart generation failed: {}", e.getMessage(), logger.isDebugEnabled() ? e : null);
            res.sendError(HttpServletResponse.SC_INTERNAL_SERVER_ERROR, e.getMessage());
    public void init(@Nullable ServletConfig config) throws ServletException {
    public @Nullable ServletConfig getServletConfig() {
    public @Nullable String getServletInfo() {
    public void destroy() {
    protected static @Nullable TemporalAmount convertToTemporalAmount(@Nullable String periodParam,
            @Nullable TemporalAmount defaultPeriod) {
        TemporalAmount period = defaultPeriod;
        String convertedPeriod = convertPeriodToISO8601(periodParam);
        if (convertedPeriod != null) {
            boolean failed = false;
                period = Period.parse(convertedPeriod);
            if (failed) {
                    period = Duration.parse(convertedPeriod);
                    // Ignored
    private static @Nullable String convertPeriodToISO8601(@Nullable String period) {
        if (period == null || period.startsWith("P") || !(period.endsWith("h") || period.endsWith("D")
                || period.endsWith("W") || period.endsWith("M") || period.endsWith("Y"))) {
        String newPeriod = period.length() == 1 ? "1" + period : period;
        if (newPeriod.endsWith("h")) {
            newPeriod = "T" + newPeriod.replace("h", "H");
        return "P" + newPeriod;
    protected static PeriodPastFuture getPeriodPastFuture(@Nullable String periodParam) {
        String periodParamPast = null;
        String periodParamFuture = null;
        TemporalAmount defaultPeriodPast = DEFAULT_PERIOD;
        TemporalAmount defaultPeriodFuture = null;
        if (periodParam != null) {
            int idx = periodParam.indexOf("-");
                periodParamPast = periodParam;
                if (idx > 0) {
                    periodParamPast = periodParam.substring(0, idx);
                    defaultPeriodPast = null;
                    defaultPeriodFuture = DEFAULT_PERIOD;
                periodParamFuture = periodParam.substring(idx + 1);
        TemporalAmount periodPast = convertToTemporalAmount(periodParamPast, defaultPeriodPast);
        TemporalAmount periodFuture = convertToTemporalAmount(periodParamFuture, defaultPeriodFuture);
        return new PeriodPastFuture(periodPast, periodFuture);
    protected static PeriodBeginEnd getPeriodBeginEnd(@Nullable ZonedDateTime begin, @Nullable ZonedDateTime end,
            PeriodPastFuture period, ZonedDateTime now) {
        ZonedDateTime timeBegin = begin;
        ZonedDateTime timeEnd = end;
        TemporalAmount periodPast = period.past();
        TemporalAmount periodFuture = period.future();
        if (timeBegin == null && timeEnd == null) {
            timeBegin = timeEnd = now;
            if (periodPast != null) {
                timeBegin = timeBegin.minus(periodPast);
            if (periodFuture != null) {
                timeEnd = timeEnd.plus(periodFuture);
        } else if (timeBegin != null && timeEnd == null) {
            timeEnd = timeBegin;
                timeEnd = timeEnd.plus(periodPast);
        } else if (timeBegin == null && timeEnd != null) {
            timeBegin = timeEnd;
                timeBegin = timeBegin.minus(periodFuture);
        return new PeriodBeginEnd(Objects.requireNonNull(timeBegin), Objects.requireNonNull(timeEnd));
    record PeriodPastFuture(@Nullable TemporalAmount past, @Nullable TemporalAmount future) {
    record PeriodBeginEnd(ZonedDateTime begin, ZonedDateTime end) {
