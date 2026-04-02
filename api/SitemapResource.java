import java.lang.ref.ReferenceQueue;
import java.lang.ref.WeakReference;
import javax.ws.rs.sse.OutboundSseEvent;
import javax.ws.rs.sse.Sse;
import javax.ws.rs.sse.SseEventSink;
import org.openhab.core.io.rest.SseBroadcaster;
import org.openhab.core.io.rest.sitemap.SitemapSubscriptionService;
import org.openhab.core.io.rest.sitemap.SitemapSubscriptionService.SitemapSubscriptionCallback;
import org.openhab.core.items.events.ItemEvent;
import org.openhab.core.sitemap.Button;
import org.openhab.core.sitemap.ButtonDefinition;
import org.openhab.core.sitemap.Buttongrid;
import org.openhab.core.sitemap.Chart;
import org.openhab.core.sitemap.Colortemperaturepicker;
import org.openhab.core.sitemap.Condition;
import org.openhab.core.sitemap.Frame;
import org.openhab.core.sitemap.Image;
import org.openhab.core.sitemap.Input;
import org.openhab.core.sitemap.Mapping;
import org.openhab.core.sitemap.Mapview;
import org.openhab.core.sitemap.Parent;
import org.openhab.core.sitemap.Rule;
import org.openhab.core.sitemap.Selection;
import org.openhab.core.sitemap.Setpoint;
import org.openhab.core.sitemap.Slider;
import org.openhab.core.sitemap.Switch;
import org.openhab.core.sitemap.Video;
import org.openhab.core.sitemap.Webview;
import org.openhab.core.ui.items.ItemUIRegistry.WidgetLabelSource;
import org.openhab.core.util.ColorUtil;
 * This class acts as a REST resource for sitemaps and provides different methods to interact with them, like retrieving
 * a list of all available sitemaps or just getting the widgets of a single page.
 * @author Laurent Garnier - Added support for icon color
 * @author Mark Herwege - Added pattern and unit fields
 * @author Laurent Garnier - Added support for new sitemap element Buttongrid
 * @author Laurent Garnier - Added icon field for mappings used for switch element
 * @author Laurent Garnier - Support added for multiple AND conditions in labelcolor/valuecolor/visibility
 * @author Laurent Garnier - New widget icon parameter based on conditional rules
 * @author Laurent Garnier - Added releaseCmd field for mappings used for switch element
 * @author Laurent Garnier - Added support for Buttongrid as container for Button elements
 * @author Laurent Garnier - Added support for new sitemap element Colortemperaturepicker
 * @author Mark Herwege - Implement sitemap registry, remove Guava dependency
@Component(service = { RESTResource.class, EventSubscriber.class })
@JaxrsName(SitemapResource.PATH_SITEMAPS)
@Path(SitemapResource.PATH_SITEMAPS)
@Tag(name = SitemapResource.PATH_SITEMAPS)
public class SitemapResource
        implements RESTResource, SitemapSubscriptionCallback, SseBroadcaster.Listener<SseSinkInfo>, EventSubscriber {
    private final Logger logger = LoggerFactory.getLogger(SitemapResource.class);
    public static final String PATH_SITEMAPS = "sitemaps";
    private static final String SEGMENT_EVENTS = "events";
    private static final String X_ACCEL_BUFFERING_HEADER = "X-Accel-Buffering";
    private static final long TIMEOUT_IN_MS = 30000;
    private SseBroadcaster<SseSinkInfo> broadcaster;
    UriInfo uriInfo;
    HttpServletRequest request;
    HttpServletResponse response;
    Sse sse;
    private final SitemapSubscriptionService subscriptions;
    private final WeakValueConcurrentHashMap<String, SseSinkInfo> knownSubscriptions = new WeakValueConcurrentHashMap<>();
    private @Nullable ScheduledFuture<?> cleanSubscriptionsJob;
    private Set<BlockingStateChangeListener> stateChangeListeners = new CopyOnWriteArraySet<>();
    public SitemapResource( //
            final @Reference ItemUIRegistry itemUIRegistry, //
            final @Reference SitemapRegistry sitemapRegistry, //
            final @Reference TimeZoneProvider timeZoneProvider, //
            final @Reference SitemapSubscriptionService subscriptions) {
        this.subscriptions = subscriptions;
        broadcaster = new SseBroadcaster<>();
        broadcaster.addListener(this);
        // The clean SSE subscriptions job sends an ALIVE event to all subscribers. This will trigger
        // an exception when the subscriber is dead, leading to the release of the SSE subscription
        // on server side.
        // In practice, the exception occurs only after the sending of a second ALIVE event. So this
        // will require two runs of the job to release an SSE subscription.
        // The clean SSE subscriptions job is run every 5 minutes.
        cleanSubscriptionsJob = scheduler.scheduleAtFixedRate(() -> {
            logger.debug("Run clean SSE subscriptions job");
            subscriptions.checkAliveClients();
        }, 1, 2, TimeUnit.MINUTES);
        ScheduledFuture<?> job = cleanSubscriptionsJob;
        if (job != null && !job.isCancelled()) {
            logger.debug("Cancel clean SSE subscriptions job");
            job.cancel(true);
            cleanSubscriptionsJob = null;
        broadcaster.removeListener(this);
        broadcaster.close();
    @Operation(operationId = "getSitemaps", summary = "Get all available sitemaps.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = SitemapDTO.class)))) })
    public Response getSitemaps() {
        logger.debug("Received HTTP GET request from IP {} at '{}'", request.getRemoteAddr(), uriInfo.getPath());
        Collection<SitemapDTO> responseObject = getSitemapBeans(uriInfo.getAbsolutePathBuilder().build());
    @Path("/{sitemapname: [a-zA-Z_0-9]+}")
    @Operation(operationId = "getSitemapByName", summary = "Get sitemap by name.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = SitemapDTO.class))) })
    public Response getSitemapData(@Context HttpHeaders headers,
            @PathParam("sitemapname") @Parameter(description = "sitemap name") String sitemapname,
            @QueryParam("type") String type, @QueryParam("jsoncallback") @DefaultValue("callback") String callback,
            @QueryParam("includeHidden") @Parameter(description = "include hidden widgets") boolean includeHiddenWidgets) {
        logger.debug("Received HTTP GET request from IP {} at '{}' for media type '{}'.", request.getRemoteAddr(),
                uriInfo.getPath(), type);
        URI uri = uriInfo.getBaseUriBuilder().build();
        SitemapDTO responseObject = getSitemapBean(sitemapname, uri, locale, includeHiddenWidgets, false);
     * Subscribe to a whole sitemap (all pages at once).
     * This is not a recommended option as it incurs high traffic
     * and might pose a high load on the server, depending on the sitemap size.
     * No built-in openhab UI should use this functionality.
    @Path("/{sitemapname: [a-zA-Z_0-9]+}/*")
    @Operation(operationId = "pollDataForSitemap", summary = "Polls the data for a whole sitemap. Not recommended due to potentially high traffic.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = SitemapDTO.class))),
            @ApiResponse(responseCode = "404", description = "Sitemap with requested name does not exist"),
            @ApiResponse(responseCode = "400", description = "Invalid subscription id has been provided.") })
            @QueryParam("subscriptionid") @Parameter(description = "subscriptionid") @Nullable String subscriptionId,
        if (subscriptionId != null) {
                subscriptions.updateSubscriptionLocation(subscriptionId, sitemapname, null);
        boolean timeout = false;
        if (headers.getRequestHeader("X-Atmosphere-Transport") != null) {
            timeout = blockUntilChangeOccurs(sitemapname, null);
        SitemapDTO responseObject = getSitemapBean(sitemapname, uriInfo.getBaseUriBuilder().build(), locale,
                includeHiddenWidgets, timeout);
    @Path("/{sitemapname: [a-zA-Z_0-9]+}/{pageid: [a-zA-Z_0-9]+}")
    @Operation(operationId = "pollDataForPage", summary = "Polls the data for one page of a sitemap.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = PageDTO.class))),
            @ApiResponse(responseCode = "404", description = "Sitemap with requested name does not exist or page does not exist, or page refers to a non-linkable widget"),
    public Response getPageData(@Context HttpHeaders headers,
            @PathParam("pageid") @Parameter(description = "page id") String pageId,
                subscriptions.updateSubscriptionLocation(subscriptionId, sitemapname, pageId);
            // Make the REST-API pseudo-compatible with openHAB 1.x
            // The client asks Atmosphere for server push functionality,
            // so we do a simply listening for changes on the appropriate items
            // The blocking has a timeout of 30 seconds. If this timeout is reached,
            // we notice this information in the response object.
            timeout = blockUntilChangeOccurs(sitemapname, pageId);
        PageDTO responseObject = getPageBean(sitemapname, pageId, uriInfo.getBaseUriBuilder().build(), locale, timeout,
                includeHiddenWidgets);
     * Creates a subscription for the stream of sitemap events.
     * @return a subscription id
    @Path(SEGMENT_EVENTS + "/subscribe")
    @Operation(operationId = "createSitemapEventSubscription", summary = "Creates a sitemap event subscription.", responses = {
            @ApiResponse(responseCode = "201", description = "Subscription created."),
            @ApiResponse(responseCode = "503", description = "Subscriptions limit reached.") })
    public Object createEventSubscription() {
        logger.debug("Received HTTP POST request from IP {} at '{}'", request.getRemoteAddr(), uriInfo.getPath());
        String subscriptionId = subscriptions.createSubscription(this);
        if (subscriptionId == null) {
            return JSONResponse.createResponse(Status.SERVICE_UNAVAILABLE, null,
                    "Max number of subscriptions is reached.");
        final SseSinkInfo sinkInfo = new SseSinkInfo(subscriptionId, subscriptions);
        knownSubscriptions.put(subscriptionId, sinkInfo);
        URI uri = uriInfo.getBaseUriBuilder().path(PATH_SITEMAPS).path(SEGMENT_EVENTS).path(subscriptionId).build();
        logger.debug("Client from IP {} requested new subscription => got id {}.", request.getRemoteAddr(),
                subscriptionId);
        // See https://github.com/openhab/openhab-core/issues/1216
        // return Response.created(uri).build();
        return JerseyResponseBuilderUtils.created(uri.toASCIIString());
     * This is not a recommended option as it incurs high SSE traffic
    @Path(SEGMENT_EVENTS + "/{subscriptionid: [a-zA-Z_0-9-]+}/*")
    @Produces(MediaType.SERVER_SENT_EVENTS)
    @Operation(operationId = "getSitemapEvents", summary = "Get sitemap events for a whole sitemap. Not recommended due to potentially high traffic.", responses = {
            @ApiResponse(responseCode = "400", description = "Missing sitemap parameter, or sitemap not linked successfully to the subscription."),
            @ApiResponse(responseCode = "404", description = "Subscription not found.") })
    public void getSitemapEvents(@Context final SseEventSink sseEventSink, @Context final HttpServletResponse response,
            @PathParam("subscriptionid") @Parameter(description = "subscription id") String subscriptionId,
            @QueryParam("sitemap") @Parameter(description = "sitemap name") @Nullable String sitemapname) {
        getSitemapEvents(sseEventSink, response, subscriptionId, sitemapname, null, true);
     * Subscribes the connecting client to the stream of sitemap events.
    @Path(SEGMENT_EVENTS + "/{subscriptionid: [a-zA-Z_0-9-]+}")
    @Operation(operationId = "getSitemapEvents", summary = "Get sitemap events.", responses = {
            @ApiResponse(responseCode = "400", description = "Missing sitemap or page parameter, or page not linked successfully to the subscription."),
            @QueryParam("sitemap") @Parameter(description = "sitemap name") @Nullable String sitemapname,
            @QueryParam("pageid") @Parameter(description = "page id") @Nullable String pageId) {
        getSitemapEvents(sseEventSink, response, subscriptionId, sitemapname, pageId, false);
        logger.debug("Received HTTP GET request from IP {} at '{}' for sitemap {} and page {}", request.getRemoteAddr(),
                uriInfo.getPath(), sitemapname, pageId);
    private void getSitemapEvents(SseEventSink sseEventSink, HttpServletResponse response, String subscriptionId,
            @Nullable String sitemapname, @Nullable String pageId, boolean subscribeToWholeSitemap) {
        // Clean up stale subscriptions
        final SseSinkInfo sinkInfo = knownSubscriptions.get(subscriptionId);
        if (sinkInfo == null) {
            logger.debug("Subscription id {} does not exist.", subscriptionId);
            response.setStatus(Status.NOT_FOUND.getStatusCode());
        if (sitemapname != null && (subscribeToWholeSitemap || pageId != null)) {
        if (subscriptions.getSitemapName(subscriptionId) == null
                || (subscriptions.getPageId(subscriptionId) == null && !subscribeToWholeSitemap)) {
            logger.debug("Subscription id {} is not yet linked to a sitemap (or sitemap and page).", subscriptionId);
            response.setStatus(Status.BAD_REQUEST.getStatusCode());
        logger.debug("Client from IP {} requested sitemap event stream for subscription {}.", request.getRemoteAddr(),
        // Disables proxy buffering when using an nginx http server proxy for this response.
        // This allows you to not disable proxy buffering in nginx and still have working sse
        response.addHeader(X_ACCEL_BUFFERING_HEADER, "no");
        broadcaster.add(sseEventSink, sinkInfo);
    private PageDTO getPageBean(String sitemapName, String pageId, URI uri, Locale locale, boolean timeout,
            boolean includeHidden) {
        if (sitemap != null) {
            if (pageId.equals(sitemap.getName())) {
                List<Widget> children = itemUIRegistry.getChildren(sitemap);
                return createPageBean(sitemapName, sitemap.getLabel(), sitemap.getIcon(), sitemap.getName(), children,
                        false, isLeaf(children), uri, locale, timeout, includeHidden);
                    List<Widget> children = itemUIRegistry.getChildren(widget);
                    PageDTO pageBean = createPageBean(sitemapName, itemUIRegistry.getLabel(pageWidget),
                            itemUIRegistry.getCategory(pageWidget), pageId, children, false, isLeaf(children), uri,
                            locale, timeout, includeHidden);
                    Parent parentPage = widget.getParent();
                    while (parentPage instanceof Frame frameParent) {
                        parentPage = frameParent.getParent();
                    if (parentPage instanceof Widget parentPageWidget) {
                        String parentId = itemUIRegistry.getWidgetId(parentPageWidget);
                        pageBean.parent = getPageBean(sitemapName, parentId, uri, locale, timeout, includeHidden);
                        pageBean.parent.widgets = null;
                        pageBean.parent.parent = null;
                    } else if (parentPage instanceof Sitemap) {
                        pageBean.parent = getPageBean(sitemapName, sitemap.getName(), uri, locale, timeout,
                                includeHidden);
                    return pageBean;
                        if (pageWidget == null) {
                            logger.debug("Received HTTP GET request at '{}' for the unknown page id '{}'.", uri,
                                    pageId);
                            logger.debug("Received HTTP GET request at '{}' for the page id '{}'. "
                                    + "This id refers to a non-linkable widget and is therefore no valid page id.", uri,
                    throw new WebApplicationException(404);
            logger.info("Received HTTP GET request at '{}' for the unknown sitemap '{}'.", uri, sitemapName);
    public Collection<SitemapDTO> getSitemapBeans(URI uri) {
        Collection<SitemapDTO> beans = new LinkedList<>();
        logger.debug("Received HTTP GET request at '{}'.", UriBuilder.fromUri(uri).build().toASCIIString());
        for (Sitemap sitemap : sitemapRegistry.getAll()) {
            SitemapDTO bean = new SitemapDTO();
            bean.name = sitemap.getName();
            bean.icon = sitemap.getIcon();
            bean.label = sitemap.getLabel();
            bean.link = UriBuilder.fromUri(uri).path(bean.name).build().toASCIIString();
            bean.homepage = new PageDTO();
            bean.homepage.link = bean.link + "/" + sitemap.getName();
            beans.add(bean);
        return beans;
    private SitemapDTO getSitemapBean(String sitemapname, URI uri, Locale locale, boolean includeHiddenWidgets,
            boolean timeout) {
        Sitemap sitemap = getSitemap(sitemapname);
            return createSitemapBean(sitemapname, sitemap, uri, locale, includeHiddenWidgets, timeout);
            logger.info("Received HTTP GET request at '{}' for the unknown sitemap '{}'.", uriInfo.getPath(),
                    sitemapname);
    private SitemapDTO createSitemapBean(String sitemapName, Sitemap sitemap, URI uri, Locale locale,
            boolean includeHiddenWidgets, boolean timeout) {
        bean.name = sitemapName;
        bean.link = UriBuilder.fromUri(uri).path(SitemapResource.PATH_SITEMAPS).path(bean.name).build().toASCIIString();
        bean.homepage = createPageBean(sitemap.getName(), sitemap.getLabel(), sitemap.getIcon(), sitemap.getName(),
                itemUIRegistry.getChildren(sitemap), true, false, uri, locale, timeout, includeHiddenWidgets);
        return bean;
    private PageDTO createPageBean(String sitemapName, @Nullable String title, @Nullable String icon, String pageId,
            @Nullable List<Widget> children, boolean drillDown, boolean isLeaf, URI uri, Locale locale, boolean timeout,
            boolean includeHiddenWidgets) {
        PageDTO bean = new PageDTO();
        bean.timeout = timeout;
        bean.id = pageId;
        bean.title = title;
        bean.icon = icon;
        bean.leaf = isLeaf;
        bean.link = UriBuilder.fromUri(uri).path(PATH_SITEMAPS).path(sitemapName).path(pageId).build().toASCIIString();
        if (children != null) {
            for (Widget widget : children) {
                String widgetId = itemUIRegistry.getWidgetId(widget);
                WidgetDTO subWidget = createWidgetBean(sitemapName, widget, drillDown, uri, widgetId, locale,
                if (subWidget != null) {
                    bean.widgets.add(subWidget);
            bean.widgets = null;
    private @Nullable WidgetDTO createWidgetBean(String sitemapName, Widget widget, boolean drillDown, URI uri,
            String widgetId, Locale locale, boolean evenIfHidden) {
        // Test visibility
        if (!evenIfHidden && !itemUIRegistry.getVisiblity(widget)) {
        WidgetDTO bean = new WidgetDTO();
        State itemState = null;
        String itemName = widget.getItem();
                Item item = itemUIRegistry.getItem(itemName);
                itemState = item.getState();
                String widgetTypeName = widget.getWidgetType();
                boolean isMapview = "mapview".equalsIgnoreCase(widgetTypeName);
                Predicate<Item> itemFilter = (i -> CoreItemFactory.LOCATION.equals(i.getType()));
                bean.item = EnrichedItemDTOMapper.map(item, isMapview, itemFilter,
                        UriBuilder.fromUri(uri).path("items/{itemName}"), locale, timeZoneProvider.getTimeZone());
                State widgetState = itemUIRegistry.getState(widget);
                bean.state = widgetState != null ? widgetState.toFullString() : null;
                // In case the widget state is identical to the item state, its value is set to null.
                if (bean.state != null && bean.state.equals(bean.item.state)) {
                    bean.state = null;
                logger.debug("{}", e.getMessage());
        bean.widgetId = widgetId;
        bean.icon = itemUIRegistry.getCategory(widget);
        bean.staticIcon = widget.isStaticIcon() || !widget.getIconRules().isEmpty();
        bean.labelcolor = convertItemValueColor(itemUIRegistry.getLabelColor(widget), itemState);
        bean.valuecolor = convertItemValueColor(itemUIRegistry.getValueColor(widget), itemState);
        bean.iconcolor = convertItemValueColor(itemUIRegistry.getIconColor(widget), itemState);
        bean.label = itemUIRegistry.getLabel(widget);
        bean.labelSource = itemUIRegistry.getLabelSource(widget).toString();
        bean.pattern = itemUIRegistry.getFormatPattern(widget);
        bean.unit = itemUIRegistry.getUnitForWidget(widget);
        bean.type = widget.getWidgetType();
        bean.visibility = itemUIRegistry.getVisiblity(widget);
        if (widget instanceof LinkableWidget linkableWidget) {
            List<Widget> children = itemUIRegistry.getChildren(linkableWidget);
            if (widget instanceof Frame || widget instanceof Buttongrid) {
                for (Widget child : children) {
                    String wID = itemUIRegistry.getWidgetId(child);
                    WidgetDTO subWidget = createWidgetBean(sitemapName, child, drillDown, uri, wID, locale,
                            evenIfHidden);
            } else if (!children.isEmpty()) {
                String pageName = itemUIRegistry.getWidgetId(linkableWidget);
                bean.linkedPage = createPageBean(sitemapName, itemUIRegistry.getLabel(widget),
                        itemUIRegistry.getCategory(widget), pageName, drillDown ? children : null, drillDown,
                        isLeaf(children), uri, locale, false, evenIfHidden);
        if (widget instanceof Switch switchWidget) {
            for (Mapping mapping : switchWidget.getMappings()) {
                MappingDTO mappingBean = new MappingDTO();
                mappingBean.command = mapping.getCmd();
                mappingBean.releaseCommand = mapping.getReleaseCmd();
                mappingBean.label = mapping.getLabel();
                mappingBean.icon = mapping.getIcon();
                bean.mappings.add(mappingBean);
        if (widget instanceof Selection selectionWidget) {
            for (Mapping mapping : selectionWidget.getMappings()) {
        if (widget instanceof Input inputWidget) {
            bean.inputHint = inputWidget.getInputHint();
        if (widget instanceof Slider sliderWidget) {
            bean.switchSupport = sliderWidget.isSwitchEnabled();
            bean.releaseOnly = sliderWidget.isReleaseOnly();
            bean.minValue = sliderWidget.getMinValue();
            bean.maxValue = sliderWidget.getMaxValue();
            bean.step = sliderWidget.getStep();
        if (widget instanceof Image imageWidget) {
            bean.url = buildProxyUrl(sitemapName, widget, uri);
            if (imageWidget.getRefresh() > 0) {
                bean.refresh = imageWidget.getRefresh();
        if (widget instanceof Video videoWidget) {
            if (videoWidget.getEncoding() != null) {
                bean.encoding = videoWidget.getEncoding();
            String encoding = videoWidget.getEncoding();
            if (encoding != null && encoding.toLowerCase().contains("hls")) {
                bean.url = videoWidget.getUrl();
                bean.url = buildProxyUrl(sitemapName, videoWidget, uri);
        if (widget instanceof Webview webViewWidget) {
            bean.url = webViewWidget.getUrl();
            bean.height = webViewWidget.getHeight();
        if (widget instanceof Mapview mapViewWidget) {
            bean.height = mapViewWidget.getHeight();
        if (widget instanceof Chart chartWidget) {
            bean.service = chartWidget.getService();
            bean.period = chartWidget.getPeriod();
            bean.legend = chartWidget.hasLegend();
            bean.forceAsItem = chartWidget.forceAsItem();
            bean.yAxisDecimalPattern = chartWidget.getYAxisDecimalPattern();
            bean.interpolation = chartWidget.getInterpolation();
            if (chartWidget.getRefresh() > 0) {
                bean.refresh = chartWidget.getRefresh();
        if (widget instanceof Setpoint setpointWidget) {
            bean.minValue = setpointWidget.getMinValue();
            bean.maxValue = setpointWidget.getMaxValue();
            bean.step = setpointWidget.getStep();
        if (widget instanceof Colortemperaturepicker colortemperaturepickerWidget) {
            bean.minValue = colortemperaturepickerWidget.getMinValue();
            bean.maxValue = colortemperaturepickerWidget.getMaxValue();
        if (widget instanceof Buttongrid buttonGridWidget) {
            for (ButtonDefinition button : buttonGridWidget.getButtons()) {
                mappingBean.row = button.getRow();
                mappingBean.column = button.getColumn();
                mappingBean.command = button.getCmd();
                mappingBean.label = button.getLabel();
                mappingBean.icon = button.getIcon();
        if (widget instanceof Button buttonWidget) {
            // Get the icon from the widget only
            if (widget.getIcon() == null && widget.getIconRules().isEmpty()) {
                bean.icon = null;
                bean.staticIcon = null;
            // Get the label from the widget only and fail back to the command if not set
            bean.label = widget.getLabel() != null ? widget.getLabel() : buttonWidget.getCmd();
            bean.labelSource = WidgetLabelSource.SITEMAP_WIDGET.toString();
            bean.pattern = null;
            bean.unit = null;
            bean.row = buttonWidget.getRow();
            bean.column = buttonWidget.getColumn();
            bean.command = buttonWidget.getCmd();
            bean.releaseCommand = buttonWidget.getReleaseCmd();
            bean.stateless = buttonWidget.isStateless();
    public static @Nullable String convertItemValueColor(@Nullable String color, @Nullable State itemState) {
        if ("itemValue".equals(color)) {
            if (itemState instanceof HSBType hsbState) {
                return "#" + Integer.toHexString(ColorUtil.hsbTosRgb(hsbState)).substring(2);
    private String buildProxyUrl(String sitemapName, Widget widget, URI uri) {
        String wId = itemUIRegistry.getWidgetId(widget);
        sb.append(uri.getScheme()).append("://").append(uri.getHost());
        if (uri.getPort() >= 0) {
            sb.append(":").append(uri.getPort());
        sb.append("/proxy?sitemap=").append(sitemapName).append("&widgetId=").append(wId);
    private boolean isLeaf(List<Widget> children) {
        for (Widget w : children) {
            if (w instanceof Frame frame) {
                if (isLeaf(frame.getWidgets())) {
            } else if (w instanceof Buttongrid grid) {
                if (isLeaf(grid.getWidgets())) {
            } else if (w instanceof LinkableWidget linkableWidget) {
                if (!itemUIRegistry.getChildren(linkableWidget).isEmpty()) {
    private @Nullable Sitemap getSitemap(String sitemapname) {
        return sitemapRegistry.get(sitemapname);
    private boolean blockUntilChangeOccurs(String sitemapname, @Nullable String pageId) {
        List<Widget> widgets = subscriptions.collectWidgets(sitemapname, pageId);
        if (widgets.isEmpty()) {
        return waitForChanges(widgets);
     * This method only returns when a change has occurred to any item on the
     * page to display or if the timeout is reached
     * @param widgets
     *            the widgets of the page to observe
     * @return true if the timeout is reached
    private boolean waitForChanges(List<Widget> widgets) {
        long startTime = (new Date()).getTime();
        Set<String> items = getAllItems(widgets).stream().map(Item::getName).collect(Collectors.toSet());
        BlockingStateChangeListener listener = new BlockingStateChangeListener(items);
        stateChangeListeners.add(listener);
        logger.debug("Waiting for changes on {} items from {} widgets", items.size(), widgets.size());
        while (!listener.hasChanged() && !timeout) {
            timeout = (new Date()).getTime() - startTime > TIMEOUT_IN_MS;
                Thread.sleep(300);
                timeout = true;
        stateChangeListeners.remove(listener);
     * Collects all items that are represented by a given list of widgets
     *            the widget list to get the items for added to all bundles containing REST resources
     * @return all items that are represented by the list of widgets
    private Set<GenericItem> getAllItems(List<Widget> widgets) {
        Set<GenericItem> items = new HashSet<>();
        for (Widget widget : widgets) {
            // We skip the chart widgets having a refresh argument
            boolean skipWidget = false;
                skipWidget = chartWidget.getRefresh() > 0;
            if (!skipWidget && itemName != null) {
                    if (item instanceof GenericItem genericItem) {
                        items.add(genericItem);
            // Consider all items inside the frame
            if (widget instanceof Frame frame) {
                items.addAll(getAllItems(frame.getWidgets()));
            } else if (widget instanceof Buttongrid grid) {
                items.addAll(getAllItems(grid.getWidgets()));
            // Consider items involved in any icon condition
            items.addAll(getItemsInRuleConditions(widget.getIconRules()));
            // Consider items involved in any visibility, labelcolor, valuecolor and iconcolor condition
            items.addAll(getItemsInRuleConditions(widget.getVisibility()));
            items.addAll(getItemsInRuleConditions(widget.getLabelColor()));
            items.addAll(getItemsInRuleConditions(widget.getValueColor()));
            items.addAll(getItemsInRuleConditions(widget.getIconColor()));
    private Set<GenericItem> getItemsInRuleConditions(List<Rule> ruleList) {
        for (Rule rule : ruleList) {
            getItemsInConditions(rule.getConditions(), items);
    private void getItemsInConditions(@Nullable List<Condition> conditions, Set<GenericItem> items) {
                String itemName = condition.getItem();
        return Set.of(ItemStateChangedEvent.TYPE, GroupItemStateChangedEvent.TYPE);
        if (event instanceof ItemEvent itemEvent) {
            String itemName = itemEvent.getItemName();
            stateChangeListeners.forEach(l -> l.itemChanged(itemName));
    public void onEvent(SitemapEvent event) {
        final Sse sse = this.sse;
        if (sse == null) {
            logger.trace("broadcast skipped (no one listened since activation)");
        final OutboundSseEvent outboundSseEvent = sse.newEventBuilder().name("event")
                .mediaType(MediaType.APPLICATION_JSON_TYPE).data(event).build();
        broadcaster.sendIf(outboundSseEvent, info -> {
            String sitemapName = event.sitemapName;
            String pageId = event.pageId;
            if (sitemapName != null && sitemapName.equals(subscriptions.getSitemapName(info.subscriptionId))
                    && Objects.equals(pageId, subscriptions.getPageId(info.subscriptionId))) {
                    if (event instanceof SitemapWidgetEvent widgetEvent) {
                        logger.debug("Sent sitemap event for widget {} to subscription {}.", widgetEvent.widgetId,
                                info.subscriptionId);
                    } else if (event instanceof ServerAliveEvent) {
                        logger.debug("Sent alive event to subscription {}.", info.subscriptionId);
    public void onRelease(String subscriptionId) {
        logger.debug("SSE connection for subscription {} has been released.", subscriptionId);
        broadcaster.closeAndRemoveIf(info -> info.subscriptionId.equals(subscriptionId));
        knownSubscriptions.remove(subscriptionId);
    public void sseEventSinkRemoved(SseEventSink sink, SseSinkInfo info) {
        logger.debug("SSE connection for subscription {} has been closed.", info.subscriptionId);
        subscriptions.removeSubscription(info.subscriptionId);
        knownSubscriptions.remove(info.subscriptionId);
     * This is a replacement implementation for Google Guava <code>new MapMaker().weakValues().makeMap()</code>, to
     * avoid pulling in a Guava dependency in this class.
     * @param <K> key
     * @param <V> value
    private class WeakValueConcurrentHashMap<K, V> {
        // Map from key → WeakReference to value
        private final Map<K, WeakValueRef<K, V>> backingMap = new ConcurrentHashMap<>();
        private final ReferenceQueue<V> refQueue = new ReferenceQueue<>();
        // Custom WeakReference that remembers its key
        private static class WeakValueRef<K, V> extends WeakReference<V> {
            final K key;
            WeakValueRef(K key, V value, ReferenceQueue<V> queue) {
                super(value, queue);
            processQueue();
            backingMap.put(key, new WeakValueRef<>(key, value, refQueue));
        public @Nullable V get(K key) {
            WeakValueRef<K, V> ref = backingMap.get(key);
            return ref != null ? ref.get() : null;
        public void remove(K key) {
            backingMap.remove(key);
        private void processQueue() {
            WeakValueRef<K, V> ref;
            while ((ref = (WeakValueRef<K, V>) refQueue.poll()) != null) {
                backingMap.remove(ref.key, ref); // remove only if still mapped
    private static class BlockingStateChangeListener {
        private final Set<String> items;
        private boolean changed = false;
        public BlockingStateChangeListener(Set<String> items) {
            this.items = items;
        public void itemChanged(String item) {
            if (items.contains(item)) {
        public boolean hasChanged() {
            return changed;
