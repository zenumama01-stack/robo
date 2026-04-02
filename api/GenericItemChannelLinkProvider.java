 * {@link GenericItemChannelLinkProvider} link items to channel by reading bindings with type "channel".
 * @author Alex Tugarev - Added parsing of multiple Channel UIDs
 * @author Laurent Garnier - Store channel links per context (model) + do not notify the registry for isolated models
@Component(immediate = true, service = { GenericItemChannelLinkProvider.class, ItemChannelLinkProvider.class,
        BindingConfigReader.class })
public class GenericItemChannelLinkProvider extends AbstractProvider<ItemChannelLink>
        implements BindingConfigReader, ItemChannelLinkProvider {
    private final Logger logger = LoggerFactory.getLogger(GenericItemChannelLinkProvider.class);
    /** caches binding configurations. maps context to a map mapping itemNames to {@link ItemChannelLink}s */
    protected Map<String, Map<String, Map<ChannelUID, ItemChannelLink>>> itemChannelLinkMap = new ConcurrentHashMap<>();
    private Map<String, Set<ChannelUID>> addedItemChannels = new ConcurrentHashMap<>();
     * stores information about the context of items. The map has this content
     * structure: context -> Set of Item names
    protected Map<String, Set<String>> contextMap = new ConcurrentHashMap<>();
    private @Nullable Set<String> previousItemNames;
    public String getBindingType() {
        return "channel";
    public void validateItemType(String itemType, String bindingConfig) throws BindingConfigParseException {
        // all item types are allowed
    public void processBindingConfiguration(String context, String itemType, String itemName, String bindingConfig,
            Configuration configuration) throws BindingConfigParseException {
        String[] uids = bindingConfig.split(",");
        if (uids.length == 0) {
            throw new BindingConfigParseException(
                    "At least one Channel UID should be provided: <bindingID>.<thingTypeId>.<thingId>.<channelId>");
            createItemChannelLink(context, itemName, uid.trim(), configuration);
    private void createItemChannelLink(String context, String itemName, String channelUID, Configuration configuration)
            throws BindingConfigParseException {
        ChannelUID channelUIDObject;
            channelUIDObject = new ChannelUID(channelUID);
            throw new BindingConfigParseException(e.getMessage());
        // Fix the configuration in case a profile is defined without any scope
        if (configuration.containsKey("profile") && configuration.get("profile") instanceof String profile
                && profile.indexOf(":") == -1) {
            String fullProfile = ProfileTypeUID.SYSTEM_SCOPE + ":" + profile;
            configuration.put("profile", fullProfile);
                    "Profile '{}' for channel '{}' is missing the scope prefix, assuming the correct UID is '{}'. Check your configuration.",
                    profile, channelUID, fullProfile);
        ItemChannelLink itemChannelLink = new ItemChannelLink(itemName, channelUIDObject, configuration);
        Set<String> itemNames = Objects.requireNonNull(contextMap.computeIfAbsent(context, k -> new HashSet<>()));
        itemNames.add(itemName);
        if (previousItemNames != null) {
            previousItemNames.remove(itemName);
        Map<String, Map<ChannelUID, ItemChannelLink>> channelLinkMap = Objects
                .requireNonNull(itemChannelLinkMap.computeIfAbsent(context, k -> new ConcurrentHashMap<>()));
        // Create a HashMap with an initial capacity of 2 (the default is 16) to save memory because most items have
        // only one channel. A capacity of 2 is enough to avoid resizing the HashMap in most cases, whereas 1 would
        // trigger a resize as soon as one element is added.
        Map<ChannelUID, ItemChannelLink> links = Objects
                .requireNonNull(channelLinkMap.computeIfAbsent(itemName, k -> new HashMap<>(2)));
        ItemChannelLink oldLink = links.put(channelUIDObject, itemChannelLink);
        if (isValidContextForListeners(context)) {
                notifyListenersAboutUpdatedElement(oldLink, itemChannelLink);
        addedItemChannels.computeIfAbsent(itemName, k -> new HashSet<>(2)).add(channelUIDObject);
    public void startConfigurationUpdate(String context) {
            logger.warn("There already is an update transaction for generic item channel links. Continuing anyway.");
        Set<String> previous = contextMap.get(context);
        previousItemNames = previous != null ? new HashSet<>(previous) : new HashSet<>();
    public void stopConfigurationUpdate(String context) {
        final Set<String> previousItemNames = this.previousItemNames;
        this.previousItemNames = null;
        if (previousItemNames == null) {
        Map<String, Map<ChannelUID, ItemChannelLink>> channelLinkMap = (itemChannelLinkMap.getOrDefault(context,
                new HashMap<>()));
        for (String itemName : previousItemNames) {
            // we remove all binding configurations that were not processed
            Map<ChannelUID, ItemChannelLink> links = channelLinkMap.remove(itemName);
            if (links != null && isValidContextForListeners(context)) {
                links.values().forEach(this::notifyListenersAboutRemovedElement);
        Optional.ofNullable(contextMap.get(context)).ifPresent(ctx -> ctx.removeAll(previousItemNames));
        addedItemChannels.forEach((itemName, addedChannelUIDs) -> {
            Map<ChannelUID, ItemChannelLink> links = channelLinkMap.getOrDefault(itemName, Map.of());
            Set<ChannelUID> removedChannelUIDs = new HashSet<>(links.keySet());
            removedChannelUIDs.removeAll(addedChannelUIDs);
            removedChannelUIDs.forEach(removedChannelUID -> {
                ItemChannelLink link = links.remove(removedChannelUID);
                if (link != null && isValidContextForListeners(context)) {
                    notifyListenersAboutRemovedElement(link);
        addedItemChannels.clear();
        if (channelLinkMap.isEmpty()) {
            itemChannelLinkMap.remove(context);
        return itemChannelLinkMap.keySet().stream().filter(context -> isValidContextForListeners(context))
                .map(name -> itemChannelLinkMap.getOrDefault(name, Map.of())).flatMap(m -> m.values().stream())
                .flatMap(m -> m.values().stream()).toList();
    public Collection<ItemChannelLink> getAllFromContext(String context) {
        return itemChannelLinkMap.getOrDefault(context, Map.of()).values().stream().flatMap(m -> m.values().stream())
    private boolean isValidContextForListeners(String context) {
        return !isIsolatedModel(context);
