 * An {@link ItemChannelLinkAddedEvent} notifies subscribers that an item channel link has been added.
 * Events must be created with the {@link LinkEventFactory}.
public class ItemChannelLinkAddedEvent extends AbstractItemChannelLinkRegistryEvent {
     * The link added event type.
    public static final String TYPE = ItemChannelLinkAddedEvent.class.getSimpleName();
    public ItemChannelLinkAddedEvent(String topic, String payload, ItemChannelLinkDTO link) {
        super(topic, payload, link);
        ItemChannelLinkDTO link = getLink();
        return "Link '" + link.itemName + "-" + link.channelUID + "' has been added.";
