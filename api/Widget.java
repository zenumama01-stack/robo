 * A representation of a sitemap widget.
public interface Widget {
     * Get the direct parent {@link Widget} or {@link Sitemap}.
     * @return parent
    Parent getParent();
     * Sets the parent {@link Widget} or {@link Sitemap}.
     * Widgets in a sitemap should always have a parent. Implementations of {@link Widget} should have a constructor
     * with {@link Parent} parameter to make building a sitemap easier.
     * @param parent
    void setParent(Parent parent);
     * Gets the item name for the widget. For specific widget type, the item is required and for these widgets, this
     * method should not return null.
     * @return item, or null if no item defined for the widget
     * Sets the widget item.
     * Get widget label.
     * Set widget label.
     * Get widget icon.
     * Set widget icon.
     * Get the widget icon rules. This method should return a modifiable list, allowing updates to the icon rules.
     * @return icon rules
    List<Rule> getIconRules();
     * Replace the widget icon rules with a new list of icon rules.
     * @param iconRules
    void setIconRules(List<Rule> iconRules);
     * True if the widget icon is static, false otherwise.
     * @return static icon
    boolean isStaticIcon();
     * Set to true if the widget icon is static.
     * @param staticIcon
    void setStaticIcon(@Nullable Boolean staticIcon);
     * Get the widget label color rules. This method should return a modifiable list, allowing updates to the label
     * color rules.
     * @return label color rules
    List<Rule> getLabelColor();
     * Replace the widget label color rules with a new list of label color rules.
     * @param labelColorRules
    void setLabelColor(List<Rule> labelColorRules);
     * Get the widget value color rules. This method should return a modifiable list, allowing updates to the value
     * @return value color rules
    List<Rule> getValueColor();
     * Replace the widget value color rules with a new list of value color rules.
     * @param valueColorRules
    void setValueColor(List<Rule> valueColorRules);
     * Get the widget icon color rules. This method should return a modifiable list, allowing updates to the icon
     * @return icon color rules
    List<Rule> getIconColor();
     * Replace the widget icon color rules with a new list of icon color rules.
     * @param iconColorRules
    void setIconColor(List<Rule> iconColorRules);
     * Get the widget visibility rules. This method should return a modifiable list, allowing updates to the visibility
     * rules.
     * @return visibility rules
    List<Rule> getVisibility();
     * Replace the widget visibility rules with a new list of visibility rules.
     * @param visibilityRules
    void setVisibility(List<Rule> visibilityRules);
     * Get type of widget.
     * @return widget type
    String getWidgetType();
