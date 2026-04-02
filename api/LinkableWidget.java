 * A representation of a sitemap linkable widget (a widget that can have children).
public interface LinkableWidget extends Widget, Parent {
     * Get the child {@link Widget}s. This method should return a modifiable list, allowing updates to the child
     * widgets.
     * @return widgets
    List<Widget> getWidgets();
     * Replace the child widgets with a new list of widgets.
    void setWidgets(List<Widget> widgets);
