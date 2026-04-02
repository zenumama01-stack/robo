import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonToken;
import com.fasterxml.jackson.core.ObjectCodec;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;
 * Custom deserializer for {@link YamlMetadataDTO} that converts any YAML scalar
 * (string, integer, boolean, or float) into a metadata String {@code value} with an empty config.
class YamlMetadataDTODeserializer extends JsonDeserializer<YamlMetadataDTO> {
    public YamlMetadataDTO deserialize(JsonParser p, DeserializationContext ctxt) throws IOException {
        JsonToken token = p.currentToken();
        if (token == JsonToken.VALUE_STRING || token == JsonToken.VALUE_NUMBER_INT
                || token == JsonToken.VALUE_NUMBER_FLOAT || token == JsonToken.VALUE_TRUE
                || token == JsonToken.VALUE_FALSE) {
            YamlMetadataDTO dto = new YamlMetadataDTO();
            dto.value = p.getValueAsString("");
        if (token == JsonToken.START_OBJECT) {
            ObjectCodec codec = p.getCodec();
            JsonNode node = codec.readTree(p);
            JsonNode valueNode = node.get("value");
            if (valueNode != null && !valueNode.isNull()) {
                dto.value = valueNode.asText();
            JsonNode configNode = node.get("config");
            if (configNode != null && !configNode.isNull()) {
                dto.config = codec.treeToValue(configNode, Map.class);
        return (YamlMetadataDTO) ctxt.handleUnexpectedToken(YamlMetadataDTO.class, p);
    public YamlMetadataDTO getNullValue(DeserializationContext ctxt) {
        return new YamlMetadataDTO();
