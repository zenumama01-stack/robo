package org.openhab.core.config.discovery.inbox;
 * The {@link Inbox} is a service interface providing a container for discovered {@code Thing}s
 * (e.g. found by a {@link org.openhab.core.config.discovery.DiscoveryService}) as {@link DiscoveryResult}s.
 * A {@link DiscoveryResult} entry in this container is not a full configured {@code Thing} and therefore no
 * {@code Thing} exists for it. A {@link DiscoveryResult} can be marked to be ignored, so that a specific {@code Thing}
 * is not considered to get part of the system.
 * This container offers a listener registry for {@link InboxListener}s which are notified if a {@link DiscoveryResult}
 * is added, updated or removed.
 * @author Laurent Garnier - Added parameter newThingId to method approve
 * @see InboxListener
public interface Inbox {
     * Adds the specified {@link DiscoveryResult} to this {@link Inbox} and sends an <i>ADDED</i>
     * event to any registered {@link InboxListener}.
     * If there is already a {@link DiscoveryResult} with the same {@code Thing} ID in this {@link Inbox}, the specified
     * {@link DiscoveryResult} is synchronized with the existing one, while keeping the {@link DiscoveryResultFlag} and
     * overriding the specific properties. In that case an <i>UPDATED</i> event is sent to any registered
     * {@link InboxListener}.
     * This method returns silently, if the specified {@link DiscoveryResult} is {@code null}.
     * @param result the discovery result to be added to this inbox (could be null)
     * @return {@link CompletableFuture} future that completes to <code>true</code> if the specified discovery result
     *         could be added or updated, otherwise to <code>false</code>
    CompletableFuture<Boolean> add(@Nullable DiscoveryResult result);
     * Removes the {@link DiscoveryResult} associated with the specified {@code Thing} ID from
     * this {@link Inbox} and sends a <i>REMOVED</i> event to any registered {@link InboxListener}.
     * This method returns silently, if the specified {@code Thing} ID is {@code null}, empty, invalid, or no associated
     * {@link DiscoveryResult} exists in this {@link Inbox}.
     * @param thingUID the Thing UID pointing to the discovery result to be removed from this inbox
     *            (could be null or invalid)
     * @return true if the specified discovery result could be removed, otherwise false
    boolean remove(@Nullable ThingUID thingUID);
     * Returns all {@link DiscoveryResult}s in this {@link Inbox}.
     * @return all discovery results in this inbox (not null, could be empty)
    List<DiscoveryResult> getAll();
     * Returns a stream of all {@link DiscoveryResult}s in this {@link Inbox}.
     * @return stream of all discovery results in this inbox
    Stream<DiscoveryResult> stream();
     * Sets the flag for a given thingUID result.<br>
     * If the specified flag is {@code null}, {@link DiscoveryResultFlag#NEW} is set by default.
     * @param flag the flag of the given thingUID result to be set (could be null)
    void setFlag(ThingUID thingUID, @Nullable DiscoveryResultFlag flag);
     * Adds an {@link InboxListener} to the listeners' registry.
     * When a {@link DiscoveryResult} is <i>ADDED</i>, <i>UPDATED</i> or <i>REMOVED</i>, the specified listener is
     * notified.
     * This method returns silently if the specified listener is {@code null} or has already been registered before.
    void addInboxListener(@Nullable InboxListener listener);
     * Removes an {@link InboxListener} from the listeners' registry.
     * When this method returns, the specified listener is no longer notified about an <i>ADDED</i>, <i>UPDATED</i> or
     * <i>REMOVED</i> {@link DiscoveryResult}.
    void removeInboxListener(@Nullable InboxListener listener);
     * Creates new {@link Thing} and adds it to the {@link org.openhab.core.thing.ThingRegistry}.
     * @param thingUID the UID of the Thing
     * @param label the label of the Thing
     * @param newThingId the thing ID of the Thing to be created; if not null, it will replace the thing ID from
     *            parameter thingUID
     * @return the approved Thing
    Thing approve(ThingUID thingUID, @Nullable String label, @Nullable String newThingId);
