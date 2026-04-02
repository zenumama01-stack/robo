 * {@link ThingWebClientUtil} provides an utility method to create a valid consumer name for web clients.
public class ThingWebClientUtil {
     * Build a valid consumer name for HTTP or WebSocket client.
     * @param uid thing UID for which to associate HTTP or WebSocket client
     * @param prefix a prefix to consider for the name; can be null
     * @return a valid consumer name
    public static String buildWebClientConsumerName(ThingUID uid, @Nullable String prefix) {
        String pref = prefix == null ? "" : prefix;
        String name = pref + uid.getAsString().replace(':', '-');
        if (name.length() > MAX_CONSUMER_NAME_LENGTH) {
            // Try to use only prefix + binding ID + thing ID
            name = pref + uid.getBindingId();
            if (name.length() > (MAX_CONSUMER_NAME_LENGTH / 2)) {
                // Truncate the binding ID to keep enough place for thing ID
                name = name.substring(0, MAX_CONSUMER_NAME_LENGTH / 2);
            // Add the thing ID
            int maxIdLength = MAX_CONSUMER_NAME_LENGTH - 1 - name.length();
            if (id.length() > maxIdLength) {
                // If thing ID is too big, use a hash code of the thing UID instead of thing id
                // and truncate it if necessary
                id = buildHashCode(uid, maxIdLength);
            name += "-" + id;
    // Make the method public just to be able to call it inside the tests
    static String buildHashCode(ThingUID uid, int maxLength) {
        String result = Integer.toHexString(uid.hashCode());
        if (result.length() > maxLength) {
            result = result.substring(result.length() - maxLength);
