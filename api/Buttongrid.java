 * A representation of a sitemap Buttongrid widget.
public interface Buttongrid extends LinkableWidget {
     * Get the button grid buttons. This method should return a modifiable list, allowing updates to the list of
     * buttons.
     * @return buttons
    List<ButtonDefinition> getButtons();
     * Replace the button grid buttons with a new list of buttons.
     * @param buttons
    void setButtons(List<ButtonDefinition> buttons);
