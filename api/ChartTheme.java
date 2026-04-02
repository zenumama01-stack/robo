package org.openhab.core.ui.internal.chart.defaultchartprovider;
import java.awt.Color;
import java.awt.Font;
 * Chart styling theme for the {@link DefaultChartProvider}.
 * @author Holger Reichert - Initial contribution
public interface ChartTheme {
     * Theme name. Has to be unique across all themes.
     * @return theme name
    String getThemeName();
     * Background color, plot area.
     * @return background color, plot area
    Color getPlotBackgroundColor();
     * Color for the grid lines.
     * @return color for the grid lines
    Color getPlotGridLinesColor();
     * Return the width of the grid lines.
     * @param dpi DPI dots per inch to calculate the width
     * @return width of the grid lines
    double getPlotGridLinesWidth(int dpi);
     * Return the dash spacing for the grid lines.
     * @return dash spacing for the grid lines
    double getPlotGridLinesDash(int dpi);
     * Background color, legend area.
     * @return background color, legend area
    Color getLegendBackgroundColor();
     * Background color, whole chart
     * @return background color, whole chart
    Color getChartBackgroundColor();
     * Font color, legend and general use.
    Color getChartFontColor();
     * Return a color for the given series number.
     * @param series series number
     * @return color for the given series numer
    Color getLineColor(int series);
     * Return the width of the series lines.
     * @return width of the series lines
    double getLineWidth(int dpi);
     * Color for the axis labels.
    Color getAxisTickLabelsColor();
     * Font for the axis labels.
     * The font size gets calculated with the dpi parameter.
     * @param dpi the DPI to calculate the font size
     * @return {@link Font} for the axis labels.
    Font getAxisTickLabelsFont(int dpi);
     * Font for the legend text.
     * @return {@link Font} for the legend text
    Font getLegendFont(int dpi);
     * Padding of the chart.
     * @param dpi the DPI to calculate the padding
     * @return padding of the chart
    int getChartPadding(int dpi);
     * Length of the line markers in the legend, in px.
     * @param dpi the DPI to calculate the line length
     * @return length of the line markers in the legend, in px
    int getLegendSeriesLineLength(int dpi);
