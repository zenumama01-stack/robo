 * A representation of a sitemap Video widget.
public interface Video extends NonLinkableWidget {
     * Get the url of the video.
    void setUrl(String url);
     * Get the configured video encoding.
     * @return encoding, null if no encoding is configured
    String getEncoding();
     * Set the video encoding.
     * @param encoding
    void setEncoding(@Nullable String encoding);
