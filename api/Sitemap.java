 * A representation of a '<em><b>Sitemap</b></em>'.
public interface Sitemap extends Identifiable<String>, Parent {
     * Returns the sitemap name.
     * @return sitemap name.
     * Sets the sitemap name.
     * @param name the new sitemap name.
    void setName(String name);
     * Returns the sitemap label.
     * @return sitemap label.
     * Sets the sitemap label.
     * @param label the new sitemap label.
    void setLabel(@Nullable String label);
     * Returns the sitemap icon.
     * @return sitemap icon.
     * Sets the sitemap icon.
     * @param icon the new sitemap icon.
     * Returns the top level list of widgets in the sitemap. The returned list is a modifiable list.
     * @return list of widgets.
     * Replace the sitemap child widgets with a new list of widgets.
