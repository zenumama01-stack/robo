 * Deserializes the internal data structure of the {@link JsonStorage})
 * The contained entities remain JSON objects and won't be deserialized to their corresponding types at this point.
public class StorageEntryMapDeserializer implements JsonDeserializer<Map<String, StorageEntry>> {
     * Finds out whether the given object is the outer JSON storage map or not.
     * It must be
     * <li>a Map of Maps
     * <li>with 2 entries each
     * <li>with {@link JsonStorage#CLASS} and {@link JsonStorage#VALUE} being their keys
     * @param obj the object to be analyzed
     * @return {@code true} if it is the outer storage map
    private boolean isOuterMap(JsonObject obj) {
        for (Map.Entry<String, JsonElement> me : obj.entrySet()) {
            JsonElement v = me.getValue();
            if (!v.isJsonObject()) {
            Set<Entry<String, JsonElement>> entrySet = ((JsonObject) v).entrySet();
            if (entrySet.size() != 2) {
            Set<String> keys = entrySet.stream().map(Entry::getKey).collect(Collectors.toSet());
            if (!keys.contains(JsonStorage.CLASS) || !keys.contains(JsonStorage.VALUE)) {
    public @Nullable Map<String, StorageEntry> deserialize(JsonElement json, Type typeOfT,
            JsonDeserializationContext context) throws JsonParseException {
        JsonObject obj = json.getAsJsonObject();
        if (!isOuterMap(obj)) {
            throw new IllegalArgumentException("Object {} is not an outer map: " + obj);
        return readOuterMap(obj, context);
    private Map<String, StorageEntry> readOuterMap(JsonObject obj, JsonDeserializationContext context) {
        Map<String, StorageEntry> map = new ConcurrentHashMap<>();
            String key = me.getKey();
            JsonObject value = me.getValue().getAsJsonObject();
            StorageEntry innerMap = new StorageEntry(value.get(JsonStorage.CLASS).getAsString(),
                    value.get(JsonStorage.VALUE));
            map.put(key, innerMap);
