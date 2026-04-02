import com.fasterxml.jackson.databind.JsonSerializer;
import com.fasterxml.jackson.databind.SerializerProvider;
 * Custom serializer for {@link YamlMetadataDTO} that writes the namespace as a scalar
 * when its config is empty, otherwise writes as an object with value and config fields.
public class YamlMetadataDTOSerializer extends JsonSerializer<YamlMetadataDTO> {
    public void serialize(YamlMetadataDTO value, JsonGenerator gen, SerializerProvider serializers) throws IOException {
        Map<?, ?> config = value.config;
        boolean configIsEmpty = (config == null || config.isEmpty());
        if (configIsEmpty) {
            gen.writeString(value.getValue());
            gen.writeStartObject();
            gen.writeStringField("value", value.getValue());
            gen.writeObjectField("config", value.config);
            gen.writeEndObject();
