 * An {@link ItemChannelLinkRemovedEvent} notifies subscribers that an item channel link has been removed.
public class ItemChannelLinkRemovedEvent extends AbstractItemChannelLinkRegistryEvent {
     * The link removed event type.
    public static final String TYPE = ItemChannelLinkRemovedEvent.class.getSimpleName();
    public ItemChannelLinkRemovedEvent(String topic, String payload, ItemChannelLinkDTO link) {
        return "Link '" + link.itemName + " => " + link.channelUID + "' has been removed.";
