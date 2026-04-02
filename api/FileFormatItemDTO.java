import org.openhab.core.items.dto.GroupFunctionDTO;
import org.openhab.core.items.dto.GroupItemDTO;
import org.openhab.core.items.dto.ItemDTO;
import org.openhab.core.items.dto.MetadataDTO;
 * This is a data transfer object to serialize an item contained in a file format.
@Schema(name = "FileFormatItem")
public class FileFormatItemDTO extends ItemDTO {
    public String groupType;
    public GroupFunctionDTO function;
    public String format;
    public Map<String, MetadataDTO> metadata;
    public List<FileFormatChannelLinkDTO> channelLinks;
    public FileFormatItemDTO(ItemDTO itemDTO, boolean isGroup) {
        this.type = itemDTO.type;
        this.name = itemDTO.name;
        this.label = itemDTO.label;
        this.category = itemDTO.category;
        this.tags = itemDTO.tags;
        this.groupNames = itemDTO.groupNames;
        if (isGroup) {
            this.groupType = ((GroupItemDTO) itemDTO).groupType;
            this.function = ((GroupItemDTO) itemDTO).function;
