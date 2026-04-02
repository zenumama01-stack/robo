import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
 * The {@link YamlMetadataDTO} is a data transfer object used to serialize a metadata for a particular namespace
 * @author Jimmy Tanagra - Support scalar metadata namespace
@JsonSerialize(using = YamlMetadataDTOSerializer.class)
@JsonDeserialize(using = YamlMetadataDTODeserializer.class)
public class YamlMetadataDTO {
    public Map<@NonNull String, @NonNull Object> config;
    public YamlMetadataDTO() {
    public @NonNull String getValue() {
        return Objects.hash(getValue(), config);
        YamlMetadataDTO other = (YamlMetadataDTO) obj;
        return Objects.equals(getValue(), other.getValue()) && Objects.equals(config, other.config);
