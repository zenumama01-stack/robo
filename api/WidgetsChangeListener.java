 * This is a class that listens on item state change events and creates sitemap events for the registered widgets.
 * @author Laurent Garnier - Buttongrid as container for Button elements
public class WidgetsChangeListener implements EventSubscriber {
    private static final int REVERT_INTERVAL = 300;
    private final String sitemapName;
    private final String pageId;
    private List<Widget> widgets;
    private Set<Item> items;
    private final HashSet<String> filterItems = new HashSet<>();
    private final List<SitemapSubscriptionCallback> callbacks = Collections.synchronizedList(new ArrayList<>());
    private Set<SitemapSubscriptionCallback> distinctCallbacks = Set.of();
     * @param sitemapName the sitemap name of the page
     * @param pageId the id of the page for which events are created
     * @param itemUIRegistry the ItemUIRegistry which is needed for the functionality
     * @param widgets the list of widgets that are part of the page.
    public WidgetsChangeListener(String sitemapName, String pageId, final ItemUIRegistry itemUIRegistry,
            final TimeZoneProvider timeZoneProvider, List<Widget> widgets) {
        this.sitemapName = sitemapName;
        this.pageId = pageId;
        updateItemsAndWidgets(widgets);
    private void updateItemsAndWidgets(List<Widget> widgets) {
        this.widgets = widgets;
        items = getAllItems(widgets);
        filterItems.clear();
        filterItems.addAll(items.stream().map(Item::getName).collect(Collectors.toSet()));
    public String getSitemapName() {
        return sitemapName;
    public String getPageId() {
        return pageId;
    public void addCallback(SitemapSubscriptionCallback callback) {
        callbacks.add(callback);
        // we transform the list of callbacks to a set in order to remove duplicates
        distinctCallbacks = new HashSet<>(callbacks);
    public void removeCallback(SitemapSubscriptionCallback callback) {
        callbacks.remove(callback);
    private Set<Item> getAllItems(List<Widget> widgets) {
        Set<Item> items = new HashSet<>();
        if (itemUIRegistry != null) {
                addItemWithName(items, widget.getItem());
                // now scan icon rules
                for (Rule rule : widget.getIconRules()) {
                    addItemsFromConditions(items, rule.getConditions());
                // now scan visibility rules
                for (Rule rule : widget.getVisibility()) {
                // now scan label color rules
                for (Rule rule : widget.getLabelColor()) {
                // now scan value color rules
                for (Rule rule : widget.getValueColor()) {
                // now scan icon color rules
                for (Rule rule : widget.getIconColor()) {
    private void addItemsFromConditions(Set<Item> items, @Nullable List<Condition> conditions) {
                addItemWithName(items, condition.getItem());
    private void addItemWithName(Set<Item> items, String itemName) {
    private void constructAndSendEvents(Item item, State newState) {
        Set<SitemapEvent> events = constructSitemapEvents(item, newState, widgets);
        for (SitemapEvent event : events) {
            for (SitemapSubscriptionCallback callback : distinctCallbacks) {
                callback.onEvent(event);
    public void keepCurrentState(Item item) {
            constructAndSendEvents(item, item.getState());
        }, REVERT_INTERVAL, TimeUnit.MILLISECONDS);
    public void changeStateTo(Item item, State state) {
        constructAndSendEvents(item, state);
    private Set<SitemapEvent> constructSitemapEvents(Item item, State state, List<Widget> widgets) {
        Set<SitemapEvent> events = new HashSet<>();
        for (Widget w : widgets) {
                events.addAll(constructSitemapEvents(item, state, itemUIRegistry.getChildren(frame)));
                events.addAll(constructSitemapEvents(item, state, itemUIRegistry.getChildren(grid)));
            boolean itemBelongsToWidget = item.getName().equals(w.getItem());
            boolean skipWidget = !itemBelongsToWidget;
            if (!skipWidget && w instanceof Chart chartWidget) {
            if (!skipWidget || definesVisibilityOrColorOrIcon(w, item.getName())) {
                SitemapWidgetEvent event = constructSitemapEventForWidget(item, state, w);
                events.add(event);
    private SitemapWidgetEvent constructSitemapEventForWidget(Item item, State state, Widget widget) {
        SitemapWidgetEvent event = new SitemapWidgetEvent();
        event.sitemapName = sitemapName;
        event.pageId = pageId;
        event.label = itemUIRegistry.getLabel(widget);
        event.labelSource = itemUIRegistry.getLabelSource(widget).toString();
        event.widgetId = itemUIRegistry.getWidgetId(widget);
        event.icon = itemUIRegistry.getCategory(widget);
        event.reloadIcon = !widget.isStaticIcon();
                event.icon = null;
                event.reloadIcon = false;
            event.label = widget.getLabel() != null ? widget.getLabel() : buttonWidget.getCmd();
            event.labelSource = WidgetLabelSource.SITEMAP_WIDGET.toString();
        event.visibility = itemUIRegistry.getVisiblity(widget);
        event.descriptionChanged = false;
        // event.item contains the (potentially changed) data of the item belonging to
        // the widget including its state (in event.item.state)
        boolean itemBelongsToWidget = item.getName().equals(widget.getItem());
        final Item itemToBeSent = itemBelongsToWidget ? item : getItemForWidget(widget);
        State stateToBeSent = null;
        if (itemToBeSent != null) {
            boolean drillDown = "mapview".equalsIgnoreCase(widgetTypeName);
            event.item = EnrichedItemDTOMapper.map(itemToBeSent, drillDown, itemFilter, null, null,
            // event.state is an adjustment of the item state to the widget type.
            stateToBeSent = itemBelongsToWidget ? state : itemToBeSent.getState();
            State convertedState = itemUIRegistry.convertState(widget, itemToBeSent, stateToBeSent);
            event.state = convertedState == null ? null : convertedState.toFullString();
            // In case this state is identical to the item state, its value is set to null.
            if (event.state != null && event.state.equals(event.item.state)) {
                event.state = null;
        event.labelcolor = SitemapResource.convertItemValueColor(itemUIRegistry.getLabelColor(widget), stateToBeSent);
        event.valuecolor = SitemapResource.convertItemValueColor(itemUIRegistry.getValueColor(widget), stateToBeSent);
        event.iconcolor = SitemapResource.convertItemValueColor(itemUIRegistry.getIconColor(widget), stateToBeSent);
    private Item getItemForWidget(Widget w) {
        final String itemName = w.getItem();
                return itemUIRegistry.getItem(itemName);
                // fall through to returning null
    private boolean definesVisibilityOrColorOrIcon(Widget w, String name) {
        return w.getVisibility().stream().anyMatch(r -> conditionsDependsOnItem(r.getConditions(), name))
                || w.getLabelColor().stream().anyMatch(r -> conditionsDependsOnItem(r.getConditions(), name))
                || w.getValueColor().stream().anyMatch(r -> conditionsDependsOnItem(r.getConditions(), name))
                || w.getIconColor().stream().anyMatch(r -> conditionsDependsOnItem(r.getConditions(), name))
                || w.getIconRules().stream().anyMatch(r -> conditionsDependsOnItem(r.getConditions(), name));
    private boolean conditionsDependsOnItem(@Nullable List<Condition> conditions, String name) {
        return conditions != null && conditions.stream().anyMatch(c -> name.equals(c.getItem()));
    public void sitemapContentChanged(List<@NonNull Widget> widgets2) {
        updateItemsAndWidgets(widgets2);
        SitemapChangedEvent changeEvent = new SitemapChangedEvent();
        changeEvent.pageId = pageId;
        changeEvent.sitemapName = sitemapName;
            callback.onEvent(changeEvent);
    public void sendAliveEvent() {
        ServerAliveEvent aliveEvent = new ServerAliveEvent();
        aliveEvent.pageId = pageId;
        aliveEvent.sitemapName = sitemapName;
            callback.onEvent(aliveEvent);
    public void descriptionChanged(String itemName) {
            Set<SitemapEvent> events = constructSitemapEventsForUpdatedDescr(item, widgets);
    private Set<SitemapEvent> constructSitemapEventsForUpdatedDescr(Item item, List<Widget> widgets) {
                events.addAll(constructSitemapEventsForUpdatedDescr(item, itemUIRegistry.getChildren(frame)));
                events.addAll(constructSitemapEventsForUpdatedDescr(item, itemUIRegistry.getChildren(grid)));
            boolean itemBelongsToWidget = w.getItem() != null && w.getItem().equals(item.getName());
            if (itemBelongsToWidget) {
                SitemapWidgetEvent event = constructSitemapEventForWidget(item, item.getState(), w);
                event.descriptionChanged = true;
        return Set.of(ItemStateChangedEvent.TYPE, GroupStateUpdatedEvent.TYPE);
        if (event instanceof ItemEvent itemEvent && filterItems.contains(itemEvent.getItemName())) {
            Item item = itemUIRegistry.get(itemEvent.getItemName());
            if (event instanceof GroupStateUpdatedEvent groupStateUpdatedEvent) {
                constructAndSendEvents(item, groupStateUpdatedEvent.getItemState());
            } else if (event instanceof ItemStateChangedEvent itemStateChangedEvent) {
                constructAndSendEvents(item, itemStateChangedEvent.getItemState());
