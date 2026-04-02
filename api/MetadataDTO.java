 * This is a data transfer object that is used to serialize metadata for a certain namespace and item.
@Schema(name = "Metadata")
public class MetadataDTO {
    public @Nullable String value;
    public @Nullable Map<String, Object> config;
    public @Nullable Boolean editable;
