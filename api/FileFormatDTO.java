import org.openhab.core.thing.dto.ThingDTO;
 * in a file format (items, things, ...).
@Schema(name = "FileFormat")
public class FileFormatDTO {
    public List<FileFormatItemDTO> items;
    public List<ThingDTO> things;
