 * A representation of a sitemap Default widget.
public interface Default extends NonLinkableWidget {
     * Get the configured height of the widget. If no height is configured, 0 should be returned.
     * @return height
    int getHeight();
     * Set the height of the widget, null means no height is configured.
     * @param height
    void setHeight(@Nullable Integer height);
