 * A representation of a sitemap Slider widget.
public interface Slider extends NonLinkableWidget {
     * Return true if the UI should render switch capabilities for the slider.
     * @return true if switch enabled
    boolean isSwitchEnabled();
     * Set switch enabled.
     * @param switchEnabled
    void setSwitchEnabled(@Nullable Boolean switchEnabled);
     * Return true if the UI should only send updates to core on mouse release.
     * @return true if release only
    boolean isReleaseOnly();
     * Set release only.
     * @param releaseOnly
    void setReleaseOnly(@Nullable Boolean releaseOnly);
     * Get minimum slider value.
     * Set minimum slider value.
     * Get maximum slider value.
     * Set maximum slider value.
     * Get slider step.
     * Set slider step.
