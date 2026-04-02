 * Implementation of the black {@link ChartTheme chart theme}.
public class ChartThemeBlack implements ChartTheme {
    private static final String THEME_NAME = "black";
    private static final Color[] LINECOLORS = new Color[] { //
            new Color(244, 67, 54), // red
            new Color(76, 175, 80), // green
            new Color(63, 81, 181), // blue
            new Color(156, 39, 176), // magenta/purple
            new Color(255, 152, 0), // orange
            new Color(0, 188, 212), // cyan
            new Color(233, 30, 99), // pink
            Color.WHITE, // white
            new Color(255, 235, 59) // yellow
    private static final String FONT_NAME = "SansSerif";
    public String getThemeName() {
        return THEME_NAME;
    public Color getPlotBackgroundColor() {
        return new Color(15, 15, 26);
    public Color getPlotGridLinesColor() {
        return Color.WHITE.darker();
    public double getPlotGridLinesWidth(int dpi) {
        return Math.max(1.0, dpi / 64.0);
    public double getPlotGridLinesDash(int dpi) {
        return Math.max(3.0f, dpi / 32.0);
    public Color getLegendBackgroundColor() {
        return new Color(30, 30, 50, 160);
    public Color getChartBackgroundColor() {
        return new Color(0, 0, 0);
    public Color getChartFontColor() {
        return new Color(255, 255, 255, 255);
    public Color getLineColor(int series) {
        return LINECOLORS[series % LINECOLORS.length];
    public double getLineWidth(int dpi) {
    public Color getAxisTickLabelsColor() {
        return getChartFontColor();
    public Font getAxisTickLabelsFont(int dpi) {
        int fontsize = (int) Math.max(8, Math.round(dpi / 8.5));
        return new Font(FONT_NAME, Font.PLAIN, fontsize);
    public Font getLegendFont(int dpi) {
        int fontsize = (int) Math.max(8, Math.round(dpi / 9.6));
    public int getChartPadding(int dpi) {
        return (int) Math.max(5, Math.round(dpi / 19.0));
    public int getLegendSeriesLineLength(int dpi) {
        return (int) Math.max(10, Math.round(dpi / 12.0));
