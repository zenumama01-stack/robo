package org.openhab.core.ui.items;
 * This interface describes the methods that need to be implemented by a provider that
 * wants to define the appearance of an item in the UI.
public interface ItemUIProvider {
     * Returns the category to use.
     * @param itemName the name of the item to return the icon for
     * @return the name of the category to use or null if undefined.
    String getCategory(String itemName);
     * Returns the label text to be used for an item in the UI.
     * @param itemName the name of the item to return the label text for
     * @return the label text to be used in the UI or null if undefined.
    String getLabel(String itemName);
     * Provides a default widget for a given item (class). This is used whenever
     * the UI needs to be created dynamically and there is no other source
     * of information about the widgets.
     * @param itemType the class of the item or null, if unknown
     * @param itemName the item name to get the default widget for
     * @return a widget implementation that can be used for the given item or null, if no default is available for the
     *         type
    Widget getDefaultWidget(@Nullable Class<? extends Item> itemType, String itemName);
     * Provides a widget for a given item. This can be used to overwrite the widget listed in the sitemap. A use case
     * for this is that the sitemap defines merely the parent-child-relation of widgets, but the concrete widget to be
     * used for rendering might be selected dynamically at runtime.
     * If the sitemap widget should not be overridden, this method must return <code>null</code>.
     * @param itemName the item name to get the widget for
     * @return a widget to use for the given item or <code>null</code> if sitemap should not be overridden.
    Widget getWidget(String itemName);
