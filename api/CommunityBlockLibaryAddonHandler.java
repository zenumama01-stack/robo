package org.openhab.core.addon.marketplace.internal.community;
import static org.openhab.core.addon.marketplace.internal.community.CommunityMarketplaceAddonService.YAML_CONTENT_PROPERTY;
import java.text.SimpleDateFormat;
import org.openhab.core.ui.components.RootUIComponent;
import org.openhab.core.ui.components.UIComponentRegistry;
import org.openhab.core.ui.components.UIComponentRegistryFactory;
 * A {@link MarketplaceAddonHandler} implementation, which handles block libraries as YAML files and installs
 * them by adding them to the {@link UIComponentRegistry} for the ui:blocks namespace.
public class CommunityBlockLibaryAddonHandler implements MarketplaceAddonHandler {
    private final Logger logger = LoggerFactory.getLogger(CommunityBlockLibaryAddonHandler.class);
    private UIComponentRegistry blocksRegistry;
    public CommunityBlockLibaryAddonHandler(final @Reference UIComponentRegistryFactory uiComponentRegistryFactory) {
        this.blocksRegistry = uiComponentRegistryFactory.getRegistry("ui:blocks");
        this.yamlMapper.setDateFormat(new SimpleDateFormat("MMM d, yyyy, hh:mm:ss aa", Locale.ENGLISH));
        return "automation".equals(type) && BLOCKLIBRARIES_CONTENT_TYPE.equals(contentType);
    public boolean isInstalled(String id) {
        return blocksRegistry.getAll().stream().anyMatch(w -> w.hasTag(id));
            String yamlDownloadUrl = (String) addon.getProperties().get(YAML_DOWNLOAD_URL_PROPERTY);
            String yamlContent = (String) addon.getProperties().get(YAML_CONTENT_PROPERTY);
            if (yamlDownloadUrl != null) {
                addWidgetAsYAML(addon.getUid(), getWidgetFromURL(yamlDownloadUrl));
            } else if (yamlContent != null) {
                addWidgetAsYAML(addon.getUid(), yamlContent);
                logger.error("Block library {} has neither download URL nor embedded content", addon.getUid());
                throw new MarketplaceHandlerException("Block library has neither download URL nor embedded content",
            logger.error("Block library from marketplace cannot be downloaded: {}", e.getMessage());
            throw new MarketplaceHandlerException("Widget cannot be downloaded.", e);
            logger.error("Block library from marketplace is invalid: {}", e.getMessage());
            throw new MarketplaceHandlerException("Widget is not valid.", e);
        blocksRegistry.getAll().stream().filter(w -> w.hasTag(addon.getUid())).forEach(w -> {
            blocksRegistry.remove(w.getUID());
    private String getWidgetFromURL(String urlString) throws IOException {
        URL u;
            u = (new URI(urlString)).toURL();
        } catch (IllegalArgumentException | URISyntaxException e) {
            throw new IOException(e);
        try (InputStream in = u.openStream()) {
            return new String(in.readAllBytes(), StandardCharsets.UTF_8);
    private void addWidgetAsYAML(String id, String yaml) {
            RootUIComponent widget = yamlMapper.readValue(yaml, RootUIComponent.class);
            // add a tag with the add-on ID to be able to identify the block library in the registry
            widget.addTag(id);
            blocksRegistry.add(widget);
            logger.error("Unable to parse YAML: {}", e.getMessage());
            throw new IllegalArgumentException("Unable to parse YAML");
