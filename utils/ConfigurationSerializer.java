import com.google.gson.JsonSerializationContext;
 * This class serializes elements of Configuration object into json as configuration object (not as
 * configuration.properties object).
 * @author Ana Dimova - provide serialization of multiple configuration values.
 * @author Sami Salonen - property names are sorted for serialization for minimal diffs
public class ConfigurationSerializer implements JsonSerializer<Configuration> {
    public JsonElement serialize(Configuration src, Type typeOfSrc, JsonSerializationContext context) {
        JsonObject result = new JsonObject();
        src.keySet().stream().sorted().forEachOrdered((String propName) -> {
            Object value = src.get(propName);
            if (value instanceof List list) {
                JsonArray array = new JsonArray();
                    array.add(serializePrimitive(element));
                result.add(propName, array);
                result.add(propName, serializePrimitive(value));
    private JsonPrimitive serializePrimitive(Object primitive) {
        if (primitive instanceof String string) {
            return new JsonPrimitive(string);
        } else if (primitive instanceof Number number) {
            return new JsonPrimitive(number);
        } else if (primitive instanceof Boolean boolean1) {
            return new JsonPrimitive(boolean1);
