 * Serializes map data by ordering the keys
 * @author Sami Salonen - Initial contribution
public class OrderingMapSerializer implements JsonSerializer<Map<@Nullable Object, @Nullable Object>> {
    @SuppressWarnings({ "rawtypes", "unchecked", "null" })
    public JsonElement serialize(Map<@Nullable Object, @Nullable Object> src, Type typeOfSrc,
            JsonSerializationContext context) {
        JsonObject ordered = new JsonObject();
        final Stream<Entry<@Nullable Object, @Nullable Object>> possiblySortedStream;
        if (OrderingSetSerializer.allSameClassAndComparable(src.keySet())) {
            // Map keys are comparable as verified above so casting to plain Comparator is safe
            possiblySortedStream = src.entrySet().stream().sorted((Comparator) Map.Entry.comparingByKey());
            possiblySortedStream = src.entrySet().stream();
        possiblySortedStream.forEachOrdered(entry -> {
            Object key = entry.getKey();
                ordered.add(string, context.serialize(entry.getValue()));
                JsonElement serialized = context.serialize(key);
                ordered.add(serialized.isJsonPrimitive() ? serialized.getAsString() : serialized.toString(),
                        context.serialize(entry.getValue()));
