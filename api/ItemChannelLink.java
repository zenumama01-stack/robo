 * {@link ItemChannelLink} defines a link between an {@link Item} and a {@link org.openhab.core.thing.Channel}.
 * @author Dennis Nobel - Initial contribution, Added getIDFor method
 * @author Simon Kaufmann - added configuration
public class ItemChannelLink extends AbstractLink {
    private final @NonNullByDefault({}) ChannelUID channelUID;
    // this field tracks whether the item's current tags were applied from the linked channel's default tags
    private transient boolean tagsLinked = false;
    ItemChannelLink() {
        this.channelUID = null;
    public ItemChannelLink(String itemName, ChannelUID channelUID) {
        this(itemName, channelUID, new Configuration());
    public ItemChannelLink(String itemName, ChannelUID channelUID, Configuration configuration) {
        super(itemName);
    public ChannelUID getLinkedUID() {
        if (obj instanceof ItemChannelLink link) {
            return super.equals(obj) && configuration.equals(link.getConfiguration());
        return super.hashCode() * configuration.hashCode();
     * Check if the item's current tags were applied from the linked channel's default tags
    public boolean tagsLinked() {
        return tagsLinked;
     * Set the flag that indicates if the item's current tags were applied from the linked channel's default tags
    public void setTagsLinked(boolean value) {
        tagsLinked = value;
