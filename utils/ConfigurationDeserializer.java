import com.google.gson.JsonArray;
import com.google.gson.JsonDeserializationContext;
 * Deserializes a {@link Configuration} object.
 * As opposed to Gson's default behavior, it ensures that all numbers are represented as {@link BigDecimal}s.
 * @author Ana Dimova - added a deserializer for the configuration, conforming to the automation json format
public class ConfigurationDeserializer implements JsonDeserializer<Configuration> {
    public Configuration deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context)
            throws JsonParseException {
        JsonObject configurationObject = json.getAsJsonObject();
        if (configurationObject.get("properties") != null) {
            return deserialize(configurationObject.get("properties").getAsJsonObject());
            return deserialize(configurationObject);
    private Configuration deserialize(JsonObject propertiesObject) {
        for (Entry<String, JsonElement> entry : propertiesObject.entrySet()) {
            JsonElement value = entry.getValue();
            if (value.isJsonPrimitive()) {
                JsonPrimitive primitive = value.getAsJsonPrimitive();
                configuration.put(key, deserialize(primitive));
            } else if (value.isJsonArray()) {
                JsonArray array = value.getAsJsonArray();
                configuration.put(key, deserialize(array));
                        "Configuration parameters must be primitives or arrays of primities only but was " + value);
    private Object deserialize(JsonPrimitive primitive) {
        if (primitive.isString()) {
            return primitive.getAsString();
        } else if (primitive.isNumber()) {
            return primitive.getAsBigDecimal();
        } else if (primitive.isBoolean()) {
            return primitive.getAsBoolean();
            throw new IllegalArgumentException("Unsupported primitive: " + primitive);
    private Object deserialize(JsonArray array) {
        List<Object> list = new LinkedList<>();
        for (JsonElement element : array) {
            if (element.isJsonPrimitive()) {
                JsonPrimitive primitive = element.getAsJsonPrimitive();
                list.add(deserialize(primitive));
                throw new IllegalArgumentException("Multiples must only contain primitives but was " + element);
