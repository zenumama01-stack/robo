 * A representation of a sitemap Setpoint widget.
public interface Setpoint extends NonLinkableWidget {
     * Get minimum setpoint value.
     * Set minimum setpoint value.
     * Get maximum setpoint value.
     * Set maximum setpoint value.
     * Get setpoint step.
     * @return step
    BigDecimal getStep();
     * Set setpoint step.
     * @param step
    void setStep(@Nullable BigDecimal step);
