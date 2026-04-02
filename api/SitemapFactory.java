package org.openhab.core.sitemap.registry;
 * The {@link SitemapFactory} is used to create {@link Sitemap}s and {@link Widget}s from their type string.
public interface SitemapFactory {
     * Creates a new {@link Sitemap} instance with name <code>sitemapName</code>
     * @param sitemapName
     * @return a new Sitemap.
    Sitemap createSitemap(String sitemapName);
     * Creates a new {@link Widget} instance of type <code>widgetTypeName</code>
     * @param widgetTypeName
     * @return a new Widget of type <code>widgetTypeName</code> or <code>null</code> if no matching class is known.
    Widget createWidget(String widgetTypeName);
     * Creates a new {@link Widget} instance of type <code>widgetTypeName</code> and with {@link Parent}
     * <code>parent</code>
    Widget createWidget(String widgetTypeName, Parent parent);
     * Creates a {@link ButtonDefinition} instance
     * @return a new ButtonDefinition.
    ButtonDefinition createButtonDefinition();
     * Creates a {@link Mapping} instance
     * @return a new Mapping.
    Mapping createMapping();
     * Creates a {@link Rule} instance
     * @return a new Rule.
    Rule createRule();
     * Creates a {@link Rule} {@link Condition} instance
     * @return a new Rule Condition.
    Condition createCondition();
     * Returns the list of all supported WidgetTypes of this Factory.
     * @return the supported WidgetTypes
    String[] getSupportedWidgetTypes();
