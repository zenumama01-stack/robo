 * This is a data transfer object that is used to serialize links.
@Schema(name = "ItemChannelLink")
public class ItemChannelLinkDTO extends AbstractLinkDTO {
    protected ItemChannelLinkDTO() {
        this("", "", Map.of());
    public ItemChannelLinkDTO(String itemName, String channelUID, Map<String, Object> configuration) {
