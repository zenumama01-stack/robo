 * A representation of a sitemap {@link Buttongrid} button definition. All buttons will act on the same item defined in
 * the button grid.
public interface ButtonDefinition {
     * Get button command.
     * Get button label.
     * @return label
     * Set button label.
    void setLabel(String label);
     * Get button icon.
     * @return icon
    String getIcon();
     * Set button icon.
     * @param icon
    void setIcon(@Nullable String icon);
