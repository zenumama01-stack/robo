import com.google.gson.TypeAdapter;
import com.google.gson.TypeAdapterFactory;
 * A {@link TypeAdapterFactory} that decorates the default {@link AccessTokenResponse} adapter in order to capture
 * additional fields returned by an OAuth 2.0 authorization server that are not part of the standard RFC 6749
 * specification. All unknown JSON properties are collected into a map and exposed via {@code extraFields} on the
 * {@link AccessTokenResponse}.
public final class AccessTokenResponseExtraFieldsAdapterFactory implements TypeAdapterFactory {
    private static final Set<String> KNOWN_FIELDS = Set.of("access_token", "token_type", "expires_in", "refresh_token",
            "scope", "state", "created_on", "extra_fields");
    public <T> @Nullable TypeAdapter<T> create(@Nullable Gson gson, @Nullable TypeToken<T> type) {
        if (type == null || !AccessTokenResponse.class.isAssignableFrom(type.getRawType())) {
        if (gson == null) {
        final TypeAdapter<T> delegate = gson.getDelegateAdapter(this, type);
        final TypeAdapter<JsonElement> elementAdapter = gson.getAdapter(JsonElement.class);
        return new TypeAdapter<>() {
            public void write(JsonWriter out, T value) throws IOException {
                delegate.write(out, value);
            public T read(JsonReader in) throws IOException {
                JsonElement tree = elementAdapter.read(in);
                if (tree == null) {
                if (!tree.isJsonObject()) {
                    return delegate.fromJsonTree(tree);
                JsonObject obj = tree.getAsJsonObject();
                T parsed = delegate.fromJsonTree(tree);
                AccessTokenResponse response = (AccessTokenResponse) parsed;
                Map<String, String> extras = new HashMap<>();
                for (Map.Entry<String, JsonElement> entry : obj.entrySet()) {
                    if (KNOWN_FIELDS.contains(key)) {
                    extras.put(key, toStringValue(gson, entry.getValue()));
                    response.setExtraFields(extras);
    private static String toStringValue(Gson gson, JsonElement el) {
        if (el.isJsonNull()) {
        if (el.isJsonPrimitive()) {
            JsonPrimitive p = el.getAsJsonPrimitive();
            return p.getAsString();
        return gson.toJson(el);
