import static org.openhab.core.addon.marketplace.internal.community.CommunityMarketplaceAddonService.*;
import org.openhab.core.addon.marketplace.internal.automation.MarketplaceRuleTemplateProvider;
 * A {@link MarketplaceAddonHandler} implementation, which handles rule templates as JSON files and installs
 * them by adding them to a {@link org.openhab.core.storage.Storage}. The templates are then served from this storage
 * through a dedicated
 * {@link org.openhab.core.automation.template.RuleTemplateProvider}.
public class CommunityRuleTemplateAddonHandler implements MarketplaceAddonHandler {
    private final Logger logger = LoggerFactory.getLogger(CommunityRuleTemplateAddonHandler.class);
    private final MarketplaceRuleTemplateProvider marketplaceRuleTemplateProvider;
    public CommunityRuleTemplateAddonHandler(
            @Reference MarketplaceRuleTemplateProvider marketplaceRuleTemplateProvider) {
        this.marketplaceRuleTemplateProvider = marketplaceRuleTemplateProvider;
        return "automation".equals(type) && RULETEMPLATES_CONTENT_TYPE.equals(contentType);
        return marketplaceRuleTemplateProvider.getAll().stream().anyMatch(t -> t.getTags().contains(id));
            String jsonDownloadUrl = (String) addon.getProperties().get(JSON_DOWNLOAD_URL_PROPERTY);
            String jsonContent = (String) addon.getProperties().get(JSON_CONTENT_PROPERTY);
            if (jsonDownloadUrl != null) {
                marketplaceRuleTemplateProvider.addTemplateAsJSON(addon.getUid(), getTemplateFromURL(jsonDownloadUrl));
            } else if (yamlDownloadUrl != null) {
                marketplaceRuleTemplateProvider.addTemplateAsYAML(addon.getUid(), getTemplateFromURL(yamlDownloadUrl));
            } else if (jsonContent != null) {
                marketplaceRuleTemplateProvider.addTemplateAsJSON(addon.getUid(), jsonContent);
                marketplaceRuleTemplateProvider.addTemplateAsYAML(addon.getUid(), yamlContent);
                logger.error("Rule template {} has neither download URL nor embedded content", addon.getUid());
                throw new MarketplaceHandlerException("Rule template has neither download URL nor embedded content",
            logger.error("Rule template from marketplace cannot be downloaded: {}", e.getMessage());
            throw new MarketplaceHandlerException("Rule template cannot be downloaded", e);
            logger.error("Failed to add rule template from the marketplace: {}", e.getMessage());
            throw new MarketplaceHandlerException("Rule template is invalid", e);
        marketplaceRuleTemplateProvider.getAll().stream().filter(t -> t.getTags().contains(addon.getUid()))
                .forEach(w -> {
                    marketplaceRuleTemplateProvider.remove(w.getUID());
    private String getTemplateFromURL(String urlString) throws IOException {
