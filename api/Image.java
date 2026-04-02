 * A representation of a sitemap Image widget.
public interface Image extends LinkableWidget {
     * Get the url of the image.
     * @return url
    String getUrl();
     * Set the url of the video.
    void setUrl(@Nullable String url);
     * Get the image refresh interval in s. If no interval is set, 0 should be returned.
     * Set the image refresh interval in s.
