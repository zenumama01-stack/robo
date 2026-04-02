import static org.openhab.core.service.StartLevelService.STARTLEVEL_COMPLETE;
import org.openhab.core.thing.link.events.LinkEventFactory;
 * {@link ItemChannelLinkRegistry} tracks all {@link ItemChannelLinkProvider}s
 * and aggregates all {@link ItemChannelLink}s.
 * @author Markus Rathgeb - Linked items returns only existing items
 * @author Markus Rathgeb - Rewrite collection handling to improve performance
 * @author Andrew Fiddian-Green - Apply channel default tags to items
@Component(immediate = true, service = ItemChannelLinkRegistry.class, configurationPid = "org.openhab.ItemChannelLinkRegistry")
public class ItemChannelLinkRegistry extends AbstractLinkRegistry<ItemChannelLink, ItemChannelLinkProvider>
        implements RegistryChangeListener<Item>, EventSubscriber {
    public static final String USE_TAGS = "useTags";
    private final Logger logger = LoggerFactory.getLogger(ItemChannelLinkRegistry.class);
    private final ServiceRegistration<?> eventRegistration;
    private boolean useTagsGlobally = false;
    private int startlevel = 0;
    private boolean oneShotLoggingDone = false;
    public ItemChannelLinkRegistry(final @Nullable Map<String, @Nullable Object> configuration,
            final @Reference ItemBuilderFactory itemBuilderFactory, BundleContext bundleContext) {
        super(ItemChannelLinkProvider.class);
        this.eventRegistration = bundleContext.registerService(EventSubscriber.class.getName(), this, null);
        this.oneShotLoggingDone = false;
    protected void modified(@Nullable Map<String, @Nullable Object> configuration) {
        Object entry = configuration != null ? configuration.get(USE_TAGS) : null;
        useTagsGlobally = entry != null ? Boolean.parseBoolean(entry.toString()) : false;
        oneShotLoggingDone = false;
        eventRegistration.unregister();
     * Returns a set of bound channels for the given item name.
     * @return an unmodifiable set of bound channels for the given item name
    public Set<ChannelUID> getBoundChannels(final String itemName) {
        return getLinks(itemName).stream().map(link -> link.getLinkedUID()).collect(Collectors.toSet());
        return super.getLinkedItemNames(uid).stream().filter(itemName -> itemRegistry.get(itemName) != null)
     * Returns a set of bound items for the given channel UID.
     * @param uid channel UID
     * @return an unmodifiable set of bound items for the given channel UID
    public Set<Item> getLinkedItems(final UID uid) {
        return super.getLinkedItemNames(uid).stream().map(itemName -> itemRegistry.get(itemName))
                .filter(Objects::nonNull).collect(Collectors.toSet());
     * Returns a set of bound things for the given item name.
     * @return an unmodifiable set of bound things for the given item name
    public Set<Thing> getBoundThings(final String itemName) {
        return getBoundChannels(itemName).stream().map(channelUID -> thingRegistry.get(channelUID.getThingUID()))
    protected void setManagedProvider(final ManagedItemChannelLinkProvider provider) {
    protected void unsetManagedProvider(final ManagedItemChannelLinkProvider provider) {
    protected void setEventPublisher(final EventPublisher eventPublisher) {
    protected void unsetEventPublisher(final EventPublisher eventPublisher) {
     * Remove all links related to a thing
     * @return the number of removed links
    public int removeLinksForThing(final ThingUID thingUID) {
        ManagedItemChannelLinkProvider managedProvider = (ManagedItemChannelLinkProvider) getManagedProvider();
        if (managedProvider == null) {
            throw new IllegalStateException("ManagedProvider is not available");
        return managedProvider.removeLinksForThing(thingUID);
     * Remove all links related to an item
    public int removeLinksForItem(final String itemName) {
        return managedProvider.removeLinksForItem(itemName);
     * Remove all orphaned (item or channel missing) links that are editable
     * @see #getOrphanLinks()
        ManagedProvider<ItemChannelLink, String> managedProvider = getManagedProvider();
        List<String> toRemove = getOrphanLinks().keySet().stream().map(ItemChannelLink::getUID)
                .filter(i -> managedProvider.get(i) != null).toList();
        toRemove.forEach(managedProvider::remove);
        return toRemove.size();
     * Get all orphan links (item or channel missing)
     * @see #purge()
     * @return a map with orphan links as key and reason they are broken as value
    public Map<ItemChannelLink, ItemChannelLinkProblem> getOrphanLinks() {
        Collection<Item> items = itemRegistry.getItems();
        Map<ItemChannelLink, ItemChannelLinkProblem> results = new HashMap<>();
                .map(Channel::getUID).collect(Collectors.toSet());
        Collection<String> itemNames = items.stream().map(Item::getName).collect(Collectors.toSet());
        getAll().forEach(itemChannelLink -> {
                if (!itemNames.contains(itemChannelLink.getItemName())) {
                    results.put(itemChannelLink, ItemChannelLinkProblem.ITEM_AND_THING_CHANNEL_MISSING);
                    results.put(itemChannelLink, ItemChannelLinkProblem.THING_CHANNEL_MISSING);
                results.put(itemChannelLink, ItemChannelLinkProblem.ITEM_MISSING);
    protected void notifyListenersAboutAddedElement(final ItemChannelLink element) {
        assignChannelDefaultTags(element);
        postEvent(LinkEventFactory.createItemChannelLinkAddedEvent(element));
    protected void notifyListenersAboutRemovedElement(final ItemChannelLink element) {
        removeChannelDefaultTags(element);
        postEvent(LinkEventFactory.createItemChannelLinkRemovedEvent(element));
    protected void notifyListenersAboutUpdatedElement(final ItemChannelLink oldElement, final ItemChannelLink element) {
        // it is not needed to send an event, because links can not be updated
    public enum ItemChannelLinkProblem {
        THING_CHANNEL_MISSING,
        ITEM_MISSING,
        ITEM_AND_THING_CHANNEL_MISSING
     * If the item does not already have a Point and/or a Property tag and if the linked channel has
     * 'useTags=true' then assign the default tags from that channel to the respective item. By contrast
     * if the item does already have a Point and/or a Property tag then we write a warning message in
     * the log. The warning is also logged if the item has more than one linked channel with 'useTags=true'.
     * And in any case if the item has native custom tags then those tags remain.
    private void assignChannelDefaultTags(ItemChannelLink link, ActiveItem activeItem) {
        boolean alreadyHasPointOrPropertySemanticTag = false;
        for (String tag : activeItem.getTags()) {
            Class<? extends Tag> type = SemanticTags.getById(tag);
            if ((type != null) && (Point.class.isAssignableFrom(type) || Property.class.isAssignableFrom(type))) {
                alreadyHasPointOrPropertySemanticTag = true;
        Set<String> channelDefaultTags = getChannelDefaultTags(link);
        if (!channelDefaultTags.isEmpty()) {
            if (alreadyHasPointOrPropertySemanticTag) {
                logger.debug("Item '{}' already tagged; ignoring tags supplied by channel '{}'.", activeItem.getName(),
                        link.getLinkedUID());
                Set<String> newTags = new HashSet<>(activeItem.getTags());
                newTags.addAll(channelDefaultTags);
                logTagsAdded(activeItem.getName(), channelDefaultTags, link.getLinkedUID());
                link.setTagsLinked(true);
                updateExistingRegistryItemTagsAndNotifyRegistryListeners(activeItem.getName(), newTags);
    private void assignChannelDefaultTags(ItemChannelLink link) {
            if (itemRegistry.getItem(link.getItemName()) instanceof ActiveItem activeItem) {
                assignChannelDefaultTags(link, activeItem);
            // we don't expect this exception but log it anyway
            logger.debug("Item '{}' not found when assigning channel default tags.", link.getItemName());
     * if the linked channel is the actual source of the item's tags then remove those tags from
     * the item. If the item had any native custom tags they shall NOT be removed. Finally iterate
     * over any other linked channels so they may eventually provide new tags.
    private void removeChannelDefaultTags(ItemChannelLink oldLink, ActiveItem activeItem) {
        if (oldLink.tagsLinked()) {
            // remove old link's tags
            Set<String> oldLinkTags = getChannelDefaultTags(oldLink);
            newTags.removeAll(oldLinkTags);
            // on openHAB shutdown tagsLinked may be true but oldLinkTags is already empty so do not log
            if (startlevel >= STARTLEVEL_COMPLETE) {
                logger.debug("Item '{}' removing tags '{}' supplied by channel '{}'.", activeItem.getName(),
                        oldLinkTags, oldLink.getLinkedUID());
            // iterate over other links in case one may assign new tags
            boolean alreadyHasPointOrPropertyTag = false;
            for (ItemChannelLink otherLink : getLinks(activeItem.getName())) {
                if (!otherLink.getUID().equals(oldLink.getUID())) {
                    Set<String> otherLinkTags = getChannelDefaultTags(otherLink);
                    if (!otherLinkTags.isEmpty()) {
                        if (alreadyHasPointOrPropertyTag) {
                            logger.debug("Item '{}' already tagged; ignoring tags supplied by channel '{}'.",
                                    activeItem.getName(), otherLink.getLinkedUID());
                            alreadyHasPointOrPropertyTag = true;
                            newTags.addAll(otherLinkTags);
                            logTagsAdded(activeItem.getName(), otherLinkTags, otherLink.getLinkedUID());
                            otherLink.setTagsLinked(true);
            oldLink.setTagsLinked(false);
    private void removeChannelDefaultTags(ItemChannelLink link) {
                removeChannelDefaultTags(link, activeItem);
     * Determine if we shall get the default tags from the linked channel. This depends on the global setting 'useTags'
     * and the per-link setting 'useTags'.
     * @param link the ItemChannelLink to be used
     * @return the default tags from the linked channel or an empty set
    private Set<String> getChannelDefaultTags(ItemChannelLink link) {
        Configuration configuration = link.getConfiguration();
        Object entry = configuration.get(USE_TAGS);
        Boolean useTagsPerLink = entry != null ? Boolean.parseBoolean(entry.toString()) : null;
        boolean getChannelTags = useTagsGlobally ? !Boolean.FALSE.equals(useTagsPerLink)
                : Boolean.TRUE.equals(useTagsPerLink);
        if (getChannelTags) {
                Channel channel = thing.getChannel(channelUID.getId());
                    return channel.getDefaultTags();
     * Update the tags on the item instance in item registry and notify the item registry listeners about
     * the change. For items provisioned by an .items file, the item registry is read only so instead of
     * trying to replace the item with a new instance, we just modify the tags on the existing item instance.
     * However since the notification method requires both old and new items so we create a 'fake old item'
     * instance having the prior tags.
     * @param itemName the name of the item in the registry that shall be be updated.
     * @param newTags the new tags that shall be applied to that item instance.
    private void updateExistingRegistryItemTagsAndNotifyRegistryListeners(String itemName, Set<String> newTags) {
            if (itemRegistry.getItem(itemName) instanceof ActiveItem item) {
                Item fakeOldItem = itemBuilderFactory.newItemBuilder(item).build();
                item.removeAllTags();
                item.addTags(newTags);
                itemRegistry.notifyListenersAboutItemExternalUpdate(fakeOldItem, item);
            logger.debug("Item '{}' not found when assigning channel default tags.", itemName);
     * If a new item is added to the item registry and we already have prior "pre- orphaned"
     * links to it, then update the tags from the default tags of the prior linked channels.
            getLinks(activeItem.getName()).forEach(link -> assignChannelDefaultTags(link, activeItem));
     * If the item has gone then clear the 'tagsLinked' flag.
            getLinks(activeItem.getName()).forEach(link -> link.setTagsLinked(false));
     * Either this class applied channel default tags to the item, or something else updated the
     * item (including possibly updating its tags), so in any case do NOT try to re-apply the
     * channel default tags. Since that might cause an infinite loop.
     * Subscribe to system start level events.
        return Set.of(StartlevelEvent.TYPE);
     * Re-initialize (one off) all the channel default tags when startup is complete.
        if (event instanceof StartlevelEvent startlevelEvent) {
            int newStartlevel = startlevelEvent.getStartlevel();
            if (newStartlevel != startlevel && newStartlevel >= STARTLEVEL_COMPLETE) {
                for (ItemChannelLink link : getAll()) {
                    assignChannelDefaultTags(link);
            startlevel = newStartlevel;
    private void logTagsAdded(String item, Set<String> tags, ChannelUID channel) {
            logger.debug("Item '{}' adding tags '{}' supplied by channel '{}'.", item, tags, channel);
        } else if (!oneShotLoggingDone) {
            oneShotLoggingDone = true;
            logger.info("At least one Item added tags supplied by a channel. => Enable debug logging to see details.");
