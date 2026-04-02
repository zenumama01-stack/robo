public class ColortemperaturepickerImpl extends NonLinkableWidgetImpl implements Colortemperaturepicker {
    private @Nullable BigDecimal minValue;
    private @Nullable BigDecimal maxValue;
    public ColortemperaturepickerImpl() {
    public ColortemperaturepickerImpl(Parent parent) {
    public @Nullable BigDecimal getMinValue() {
    public void setMinValue(@Nullable BigDecimal minValue) {
        this.minValue = minValue;
    public @Nullable BigDecimal getMaxValue() {
    public void setMaxValue(@Nullable BigDecimal maxValue) {
        this.maxValue = maxValue;
