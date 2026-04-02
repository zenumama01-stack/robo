import java.awt.BasicStroke;
import java.awt.Graphics2D;
import org.knowm.xchart.XYChart;
import org.knowm.xchart.XYChartBuilder;
import org.knowm.xchart.XYSeries;
import org.knowm.xchart.style.AxesChartStyler.TextAlignment;
import org.knowm.xchart.style.Styler.LegendPosition;
import org.knowm.xchart.style.XYStyler;
import org.knowm.xchart.style.markers.None;
import org.openhab.core.ui.internal.chart.ChartServlet;
 * This default chart provider generates time-series charts for a given set of items.
 * See {@link ChartProvider} and {@link ChartServlet} for further details.
 * @author Christoph Weitkamp - Consider default persistence service
 * @author Jan N. Klug - Add y-axis label formatter
public class DefaultChartProvider implements ChartProvider {
    private static class LegendPositionDecider {
        private int counter = 0;
        private void addData(XYSeries series, List<Number> yData) {
            // If the start value is below the median, then count legend position down
            // Otherwise count up.
            // We use this to decide whether to put the legend in the top or bottom corner.
            if (yData.getFirst().floatValue() > ((series.getYMax() - series.getYMin()) / 2 + series.getYMin())) {
                counter--;
        private LegendPosition getLegendPosition() {
            return counter < 0 ? LegendPosition.InsideNW : LegendPosition.InsideSW;
    private static final Duration TEN_MINUTES = Duration.ofMinutes(10);
    private static final Duration ONE_DAY = Duration.ofDays(1);
    private static final Duration ONE_WEEK = Duration.ofDays(7);
    private static final ChartTheme CHART_THEME_DEFAULT = new ChartThemeBright();
    private static final Map<String, ChartTheme> CHART_THEMES = Stream
            .of(CHART_THEME_DEFAULT, new ChartThemeBrightTransparent(), //
                    new ChartThemeWhite(), new ChartThemeWhiteTransparent(), //
                    new ChartThemeDark(), new ChartThemeDarkTransparent(), //
                    new ChartThemeBlack(), new ChartThemeBlackTransparent()) //
            .collect(Collectors.toMap(ChartTheme::getThemeName, Function.identity()));
    private static final int DPI_DEFAULT = 96;
    private static final String INTERPOLATION_LINEAR = "linear";
    private static final String INTERPOLATION_STEP = "step";
    private final Logger logger = LoggerFactory.getLogger(DefaultChartProvider.class);
    public DefaultChartProvider(final @Reference ItemUIRegistry itemUIRegistry,
            logger.debug("Available themes for default chart provider: {}", String.join(", ", CHART_THEMES.keySet()));
        return "default";
    public BufferedImage createChart(@Nullable String serviceId, @Nullable String theme, ZonedDateTime startTime,
            @Nullable Integer dpiValue, @Nullable String interpolation, @Nullable Boolean legend)
            throws ItemNotFoundException, IllegalArgumentException {
        return createChart(serviceId, theme, startTime, endTime, height, width, items, groups, dpiValue, null,
                interpolation, legend);
            @Nullable Integer dpiValue, @Nullable String yAxisDecimalPattern, @Nullable String interpolation,
            @Nullable Boolean legend) throws ItemNotFoundException, IllegalArgumentException {
                "Rendering chart: service: '{}', theme: '{}', startTime: '{}', endTime: '{}', width: '{}', height: '{}', items: '{}', groups: '{}', dpi: '{}', yAxisDecimalPattern: '{}', interpolation: '{}', legend: '{}'",
                serviceId, theme, startTime, endTime, width, height, items, groups, dpiValue, yAxisDecimalPattern,
        // If a persistence service is specified, find the provider, or use the default provider
        PersistenceService service = (serviceId == null) ? persistenceServiceRegistry.getDefault()
                : persistenceServiceRegistry.get(serviceId);
        // Did we find a service?
        QueryablePersistenceService persistenceService = (service instanceof QueryablePersistenceService qps) ? qps
                : (QueryablePersistenceService) persistenceServiceRegistry.getAll() //
                        .stream() //
                        .filter(QueryablePersistenceService.class::isInstance) //
                        .orElseThrow(() -> new IllegalArgumentException("No Persistence service found."));
        int seriesCounter = 0;
        // get theme
        ChartTheme chartTheme = theme == null ? CHART_THEME_DEFAULT
                : CHART_THEMES.getOrDefault(theme, CHART_THEME_DEFAULT);
        // get DPI
        int dpi = dpiValue != null && dpiValue > 0 ? dpiValue : DPI_DEFAULT;
        // Create Chart
        XYChart chart = new XYChartBuilder().width(width).height(height).build();
        // Define the time axis - the defaults are not very nice
        Duration period = Duration.between(startTime, endTime);
        String pattern;
        if (period.compareTo(TEN_MINUTES) <= 0) {
            pattern = "mm:ss";
        } else if (period.compareTo(ONE_DAY) <= 0) {
            pattern = "HH:mm";
        } else if (period.compareTo(ONE_WEEK) <= 0) {
            pattern = "EEE d";
            pattern = "d MMM";
        XYStyler styler = chart.getStyler();
        styler.setDatePattern(pattern);
        // axis
        styler.setAxisTickLabelsFont(chartTheme.getAxisTickLabelsFont(dpi));
        styler.setAxisTickLabelsColor(chartTheme.getAxisTickLabelsColor());
        styler.setXAxisMin((double) startTime.toInstant().toEpochMilli());
        styler.setXAxisMax((double) endTime.toInstant().toEpochMilli());
        int yAxisSpacing = Math.max(height / 10, chartTheme.getAxisTickLabelsFont(dpi).getSize());
        if (yAxisDecimalPattern != null) {
            styler.setYAxisDecimalPattern(yAxisDecimalPattern);
        styler.setYAxisTickMarkSpacingHint(yAxisSpacing);
        styler.setYAxisLabelAlignment(TextAlignment.Right);
        // avoid overlapping labels on x-axis;
        // Font.getSize() is related to font height, but will also give an indication of the width;
        // pattern is not the actual label, but also gives a rough estimation of the width of the label
        int xAxisSpacing = Math.max(width / 15,
                chartTheme.getAxisTickLabelsFont(dpi).getSize() * pattern.length() * 4 / 3);
        styler.setXAxisTickMarkSpacingHint(xAxisSpacing);
        // chart
        styler.setChartBackgroundColor(chartTheme.getChartBackgroundColor());
        styler.setChartFontColor(chartTheme.getChartFontColor());
        styler.setChartPadding(chartTheme.getChartPadding(dpi));
        styler.setPlotBackgroundColor(chartTheme.getPlotBackgroundColor());
        float plotGridLinesDash = (float) chartTheme.getPlotGridLinesDash(dpi);
        float[] plotGridLinesDashArray = { plotGridLinesDash, plotGridLinesDash };
        styler.setPlotGridLinesStroke(new BasicStroke((float) chartTheme.getPlotGridLinesWidth(dpi),
                BasicStroke.CAP_BUTT, BasicStroke.JOIN_BEVEL, 10, plotGridLinesDashArray, 0));
        styler.setPlotGridLinesColor(chartTheme.getPlotGridLinesColor());
        // legend
        styler.setLegendBackgroundColor(chartTheme.getLegendBackgroundColor());
        styler.setLegendFont(chartTheme.getLegendFont(dpi));
        styler.setLegendSeriesLineLength(chartTheme.getLegendSeriesLineLength(dpi));
        LegendPositionDecider legendPositionDecider = new LegendPositionDecider();
        // Loop through all the items
            String[] itemNames = items.split(",");
                if (addItem(chart, persistenceService, startTime, endTime, item, seriesCounter, chartTheme, dpi,
                        interpolation, legendPositionDecider)) {
                    seriesCounter++;
        // Loop through all the groups and add each item from each group
            String[] groupNames = groups.split(",");
                Item item = itemUIRegistry.getItem(groupName);
                        if (addItem(chart, persistenceService, startTime, endTime, member, seriesCounter, chartTheme,
                                dpi, interpolation, legendPositionDecider)) {
                    throw new ItemNotFoundException("Item '" + item.getName() + "' defined in groups is not a group.");
        Boolean showLegend = null;
        // If there are no series, render a blank chart
        if (seriesCounter == 0) {
            // always hide the legend
            showLegend = false;
            List<Date> xData = new ArrayList<>();
            List<Number> yData = new ArrayList<>();
            xData.add(Date.from(startTime.toInstant()));
            yData.add(0);
            xData.add(Date.from(endTime.toInstant()));
            XYSeries series = chart.addSeries("NONE", xData, yData);
            series.setMarker(new None());
            series.setLineStyle(new BasicStroke(0f));
        // if the legend is not already hidden, check if legend parameter is supplied, or calculate a sensible value
        if (showLegend == null) {
            if (legend == null) {
                // more than one series, show the legend. otherwise hide it.
                showLegend = seriesCounter > 1;
                // take value from supplied legend parameter
                showLegend = legend;
        // Legend position (top-left or bottom-left) is dynamically selected based on the data
        // This won't be perfect, but it's a good compromise
            styler.setLegendPosition(legendPositionDecider.getLegendPosition());
        } else { // hide the whole legend
            styler.setLegendVisible(false);
        // Write the chart as a PNG image
        BufferedImage lBufferedImage = new BufferedImage(chart.getWidth(), chart.getHeight(),
                BufferedImage.TYPE_INT_ARGB);
        Graphics2D lGraphics2D = lBufferedImage.createGraphics();
        chart.paint(lGraphics2D, chart.getWidth(), chart.getHeight());
        return lBufferedImage;
    private double convertData(State state) {
        if (state instanceof DecimalType type) {
            return type.doubleValue();
        } else if (state instanceof QuantityType type) {
        } else if (state instanceof OnOffType) {
            return state == OnOffType.OFF ? 0 : 1;
        } else if (state instanceof OpenClosedType) {
            return state == OpenClosedType.CLOSED ? 0 : 1;
            logger.debug("Unsupported item type in chart: {}", state.getClass());
    private boolean addItem(XYChart chart, QueryablePersistenceService service, ZonedDateTime timeBegin,
            ZonedDateTime timeEnd, Item item, int seriesCounter, ChartTheme chartTheme, int dpi,
            @Nullable String interpolation, LegendPositionDecider legendPositionDecider) {
        Color color = chartTheme.getLineColor(seriesCounter);
        // Get the item label
        String label = itemUIRegistry.getLabel(item.getName());
            label = item.getName();
        } else if (label.contains("[") && label.contains("]")) {
            label = label.substring(0, label.indexOf('['));
        FilterCriteria filter;
        // Generate data collections
        // Declare state here so it will hold the last value at the end of the process
        State state = null;
        // This is necessary for values that don't change often otherwise data will start
        // after the start of the graph (or not at all if there's no change during the graph period)
        filter = new FilterCriteria();
        filter.setEndDate(timeBegin);
        PersistenceServiceConfiguration config = persistenceServiceConfigurationRegistry.get(service.getId());
        result = service.query(filter, alias);
            HistoricItem historicItem = result.iterator().next();
            state = historicItem.getState();
            xData.add(Date.from(timeBegin.toInstant()));
            yData.add(convertData(state));
        // Now, get all the data between the start and end time
        filter.setBeginDate(timeBegin);
        filter.setEndDate(timeEnd);
        // Get the data from the persistence store
        for (HistoricItem historicItem : result) {
            if (state != null && INTERPOLATION_STEP.equals(interpolation)
                    || ((state instanceof OnOffType || state instanceof OpenClosedType)
                            && !INTERPOLATION_LINEAR.equals(interpolation))) {
                xData.add(Date.from(historicItem.getInstant().minus(1, ChronoUnit.MILLIS)));
            xData.add(Date.from(historicItem.getInstant()));
        // Lastly, add the final state at the endtime
            xData.add(Date.from(timeEnd.toInstant()));
        // Add the new series to the chart - only if there's data elements to display
        // The chart engine will throw an exception if there's no data
        if (xData.isEmpty()) {
        // If there's only 1 data point, plot it again!
        if (xData.size() == 1) {
            xData.add(xData.getFirst());
            yData.add(yData.getFirst());
        XYSeries series = chart.addSeries(label, xData, yData);
        float lineWidth = (float) chartTheme.getLineWidth(dpi);
        series.setLineStyle(new BasicStroke(lineWidth, BasicStroke.CAP_ROUND, BasicStroke.JOIN_MITER));
        series.setLineColor(color);
        legendPositionDecider.addData(series, yData);
    public ImageType getChartType() {
        return ImageType.png;
