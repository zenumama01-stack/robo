 * The {@link EnrichedItemChannelLinkDTOMapper} is a utility class to map item channel links into enriched item channel
 * link data transform objects (DTOs).
public class EnrichedItemChannelLinkDTOMapper {
     * Maps an item channel link into an enriched item channel link DTO object.
     * @param link the item channel link
     * @return item channel link DTO object
    public static EnrichedItemChannelLinkDTO map(ItemChannelLink link, boolean editable) {
        return new EnrichedItemChannelLinkDTO(link.getItemName(), link.getLinkedUID().toString(),
                link.getConfiguration().getProperties(), editable);
