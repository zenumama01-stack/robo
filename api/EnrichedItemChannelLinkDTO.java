 * This is an enriched data transfer object that is used to serialize items channel links with dynamic data like the
 * editable flag.
@Schema(name = "EnrichedItemChannelLink")
public class EnrichedItemChannelLinkDTO extends ItemChannelLinkDTO {
    public EnrichedItemChannelLinkDTO(String itemName, String channelUID, Map<String, Object> configuration,
            boolean editable) {
        super(itemName, channelUID, configuration);
