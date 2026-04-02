public class ChartImpl extends NonLinkableWidgetImpl implements Chart {
    private @Nullable String service;
    private @Nullable Integer refresh;
    private String period = "";
    private @Nullable Boolean legend;
    private @Nullable Boolean forceAsItem;
    private @Nullable String yAxisDecimalPattern;
    private @Nullable String interpolation;
    public ChartImpl() {
    public ChartImpl(Parent parent) {
    public @Nullable String getService() {
    public void setService(@Nullable String service) {
        this.service = service;
    public int getRefresh() {
        return refresh != null ? refresh : 0;
    public void setRefresh(@Nullable Integer refresh) {
        this.refresh = refresh;
    public String getPeriod() {
        return period;
    public void setPeriod(String period) {
        this.period = period;
    public @Nullable Boolean hasLegend() {
        return legend;
    public void setLegend(@Nullable Boolean legend) {
        this.legend = legend;
    public boolean forceAsItem() {
        return forceAsItem != null ? forceAsItem : false;
    public void setForceAsItem(@Nullable Boolean forceAsItem) {
        this.forceAsItem = forceAsItem;
    public @Nullable String getYAxisDecimalPattern() {
        return yAxisDecimalPattern;
    public void setYAxisDecimalPattern(@Nullable String yAxisDecimalPattern) {
        this.yAxisDecimalPattern = yAxisDecimalPattern;
    public @Nullable String getInterpolation() {
        return interpolation;
    public void setInterpolation(@Nullable String interpolation) {
        this.interpolation = interpolation;
