 * This is a data transfer object to serialize a channel link for an item contained in a file format.
@Schema(name = "FileFormatChannelLink")
public class FileFormatChannelLinkDTO {
    public String channelUID;
    public @Nullable Map<String, Object> configuration;
    public FileFormatChannelLinkDTO(String channelUID, @Nullable Map<String, Object> configuration) {
        this.channelUID = channelUID;
        this.configuration = configuration;
