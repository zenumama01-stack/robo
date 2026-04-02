package org.openhab.core.model.yaml.internal.items;
import static org.openhab.core.model.yaml.YamlModelUtils.isIsolatedModel;
 * This class serves as a provider for all item channel links that is found within YAML files.
 * It is filled with content by the {@link YamlItemProvider}, which cannot itself implement the
 * {@link ItemChannelLinkProvider} interface as it already implements {@link ItemProvider},
 * which would lead to duplicate methods.
@Component(immediate = true, service = { ItemChannelLinkProvider.class, YamlChannelLinkProvider.class })
public class YamlChannelLinkProvider extends AbstractProvider<ItemChannelLink> implements ItemChannelLinkProvider {
    private final Logger logger = LoggerFactory.getLogger(YamlChannelLinkProvider.class);
    // Map the channel links to each channel UID and then to each item name and finally to each model name
    private Map<String, Map<String, Map<ChannelUID, ItemChannelLink>>> itemsChannelLinksMap = new ConcurrentHashMap<>();
        return itemsChannelLinksMap.keySet().stream().filter(name -> !isIsolatedModel(name))
                .map(name -> itemsChannelLinksMap.getOrDefault(name, Map.of())).flatMap(m -> m.values().stream())
    public Collection<ItemChannelLink> getAllFromModel(String modelName) {
        return itemsChannelLinksMap.getOrDefault(modelName, Map.of()).values().stream()
    public void updateItemChannelLinks(String modelName, String itemName, Map<String, Configuration> channelLinks) {
        Map<String, Map<ChannelUID, ItemChannelLink>> channelLinksMap = Objects
                .requireNonNull(itemsChannelLinksMap.computeIfAbsent(modelName, k -> new ConcurrentHashMap<>()));
                .requireNonNull(channelLinksMap.computeIfAbsent(itemName, k -> new ConcurrentHashMap<>(2)));
        Set<ChannelUID> linksToBeRemoved = new HashSet<>(links.keySet());
        for (Map.Entry<String, Configuration> entry : channelLinks.entrySet()) {
            String channelUID = entry.getKey();
            Configuration configuration = entry.getValue();
                logger.warn("Invalid channel UID '{}' in channel link for item '{}'!", channelUID, itemName, e);
            linksToBeRemoved.remove(channelUIDObject);
            ItemChannelLink oldLink = links.get(channelUIDObject);
                links.put(channelUIDObject, itemChannelLink);
                logger.debug("model {} added channel link {}", modelName, itemChannelLink.getUID());
            } else if (!Objects.equals(configuration.getProperties(), oldLink.getConfiguration().getProperties())) {
                logger.debug("model {} updated channel link {}", modelName, itemChannelLink.getUID());
        linksToBeRemoved.forEach(uid -> {
            ItemChannelLink link = links.remove(uid);
            if (link != null) {
                logger.debug("model {} removed channel link {}", modelName, link.getUID());
        if (links.isEmpty()) {
            channelLinksMap.remove(itemName);
        if (channelLinksMap.isEmpty()) {
            itemsChannelLinksMap.remove(modelName);
