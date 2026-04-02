 * A sitemap event, which provides details about a widget that has changed.
 * @author Laurent Garnier - New field iconcolor
 * @author Laurent Garnier - New field reloadIcon
 * @author Danny Baumann - New field labelSource
public class SitemapWidgetEvent extends SitemapEvent {
    public String widgetId;
    public String labelSource;
    public boolean reloadIcon;
    public String labelcolor;
    public String valuecolor;
    public String iconcolor;
    public boolean visibility;
    public EnrichedItemDTO item;
    public boolean descriptionChanged;
    public SitemapWidgetEvent() {
