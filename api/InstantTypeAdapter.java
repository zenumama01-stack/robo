package org.openhab.core.storage.json.internal;
 * The {@link InstantTypeAdapter} implements serialization and deserialization of {@link Instant}.
 * as formatted UTC strings.
 * Deserialization supports milliseconds since epoch as well.
public class InstantTypeAdapter implements JsonSerializer<Instant>, JsonDeserializer<Instant> {
     * Converts an {@link Instant} to a formatted UTC string.
    public JsonElement serialize(Instant instant, Type typeOfSrc, JsonSerializationContext context) {
        return new JsonPrimitive(instant.toString());
     * Converts a formatted UTC string to {@link Instant}.
     * As fallback, milliseconds since epoch is supported as well.
    public @Nullable Instant deserialize(JsonElement element, Type arg1, JsonDeserializationContext arg2)
            return Instant.parse(element.getAsString());
            // Fallback to milliseconds since epoch for backwards compatibility.
            return Instant.ofEpochMilli(element.getAsLong());
