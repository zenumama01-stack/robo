 * An {@link EventFilter} can be provided by an {@link EventSubscriber} in order
 * to receive specific {@link Event}s by an {@link EventPublisher} if the filter applies.
public interface EventFilter {
     * Apply the filter on an event.
     * This method is called for each subscribed {@link Event} of an
     * {@link EventSubscriber}. If the filter applies, the event will be dispatched to the
     * {@link EventSubscriber#receive(Event)} method.
     * @param event the event (not null)
     * @return true if the filter criterion applies
    boolean apply(Event event);
