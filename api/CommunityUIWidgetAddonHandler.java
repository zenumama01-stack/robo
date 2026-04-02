 * A {@link MarketplaceAddonHandler} implementation, which handles UI widgets as YAML files and installs
 * them by adding them to the {@link UIComponentRegistry} for the ui:widget namespace.
public class CommunityUIWidgetAddonHandler implements MarketplaceAddonHandler {
    private final Logger logger = LoggerFactory.getLogger(CommunityUIWidgetAddonHandler.class);
    private final UIComponentRegistry widgetRegistry;
    public CommunityUIWidgetAddonHandler(final @Reference UIComponentRegistryFactory uiComponentRegistryFactory) {
        this.widgetRegistry = uiComponentRegistryFactory.getRegistry("ui:widget");
        return "ui".equals(type) && UIWIDGETS_CONTENT_TYPE.equals(contentType);
        return widgetRegistry.getAll().stream().anyMatch(w -> w.hasTag(id));
                logger.error("UI Widget {} has neither download URL nor embedded content", addon.getUid());
                throw new MarketplaceHandlerException("UI Widget has neither download URL nor embedded content", null);
            logger.error("Widget from marketplace cannot be downloaded: {}", e.getMessage());
            logger.error("Widget from marketplace is invalid: {}", e.getMessage());
        widgetRegistry.getAll().stream().filter(w -> w.hasTag(addon.getUid())).forEach(w -> {
            widgetRegistry.remove(w.getUID());
            // add a tag with the add-on ID to be able to identify the widget in the registry
            widgetRegistry.add(widget);
