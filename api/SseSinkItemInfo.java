 * The specific information we need to hold for a SSE sink which tracks item state updates.
public class SseSinkItemInfo {
    private final String connectionId = UUID.randomUUID().toString();
    private final Set<String> trackedItems = new CopyOnWriteArraySet<>();
     * Gets the connection identifier of this {@link SseSinkItemInfo}
     * @return the connection id
    public String getConnectionId() {
        return connectionId;
     * Updates the list of tracked items for a connection
     * @param itemNames the item names to track
    public void updateTrackedItems(Set<String> itemNames) {
        trackedItems.clear();
        trackedItems.addAll(itemNames);
    public static Predicate<SseSinkItemInfo> hasConnectionId(String connectionId) {
        return info -> info.connectionId.equals(connectionId);
    public static Predicate<SseSinkItemInfo> tracksItem(String itemName) {
        return info -> info.trackedItems.contains(itemName);
