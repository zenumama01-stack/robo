import org.openhab.core.common.registry.AbstractProvider;
import org.openhab.core.common.registry.ManagedProvider;
import org.openhab.core.thing.link.ItemChannelLinkProvider;
 * This {@link ItemChannelLinkProvider} keeps {@link ItemChannelLink}s provided by scripts during runtime.
 * This ensures that {@link ItemChannelLink}s are not kept on reboot, but have to be provided by the scripts again.
@Component(immediate = true, service = { ScriptedItemChannelLinkProvider.class, ItemChannelLinkProvider.class })
public class ScriptedItemChannelLinkProvider extends AbstractProvider<ItemChannelLink>
        implements ItemChannelLinkProvider, ManagedProvider<ItemChannelLink, String> {
    private final Logger logger = LoggerFactory.getLogger(ScriptedItemChannelLinkProvider.class);
    private final Map<String, ItemChannelLink> itemChannelLinks = new HashMap<>();
        return itemChannelLinks.values();
        return itemChannelLinks.get(key);
    public void add(ItemChannelLink itemChannelLink) {
        if (get(itemChannelLink.getUID()) != null) {
                    "Cannot add item->channel link, because an item->channel link with same UID ("
                            + itemChannelLink.getUID() + ") already exists.");
        itemChannelLinks.put(itemChannelLink.getUID(), itemChannelLink);
        notifyListenersAboutAddedElement(itemChannelLink);
    public @Nullable ItemChannelLink update(ItemChannelLink itemChannelLink) {
        ItemChannelLink oldItemChannelLink = itemChannelLinks.get(itemChannelLink.getUID());
        if (oldItemChannelLink != null) {
            notifyListenersAboutUpdatedElement(oldItemChannelLink, itemChannelLink);
            logger.warn("Cannot update item->channel link with UID '{}', because it does not exist.",
                    itemChannelLink.getUID());
        return oldItemChannelLink;
        ItemChannelLink itemChannelLink = itemChannelLinks.remove(key);
        if (itemChannelLink != null) {
            notifyListenersAboutRemovedElement(itemChannelLink);
        return itemChannelLink;
