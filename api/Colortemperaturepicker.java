 * A representation of a sitemap Colortemperaturepicker widget.
public interface Colortemperaturepicker extends NonLinkableWidget {
     * Get minimum color temperature value.
     * @return minValue
    BigDecimal getMinValue();
     * Set minimum color temperature value.
     * @param minValue
    void setMinValue(@Nullable BigDecimal minValue);
     * Get maximum color temperature value.
     * @return maxValue
    BigDecimal getMaxValue();
     * Set maximum color temperature value.
     * @param maxValue
    void setMaxValue(@Nullable BigDecimal maxValue);
