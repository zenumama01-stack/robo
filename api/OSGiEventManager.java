import org.osgi.service.event.Event;
import org.osgi.service.event.EventHandler;
 * The {@link OSGiEventManager} provides an OSGi based default implementation of the openHAB event bus.
 * The OSGiEventHandler tracks {@link EventSubscriber}s and {@link EventFactory}s, receives OSGi events (by
 * implementing the OSGi {@link EventHandler} interface) and dispatches the received OSGi events
 * as OH {@link org.openhab.core.events.Event}s to the {@link EventSubscriber}s if the provided filter applies.
 * @author Markus Rathgeb - Return on received events as fast as possible (handle event in another thread)
@Component(immediate = true, property = { "event.topics:String=openhab" })
public class OSGiEventManager implements EventHandler {
    /** The event subscribers indexed by the event type. */
    // Use a concurrent hash map because the map is written and read by different threads!
    private final Map<String, Set<EventSubscriber>> typedEventSubscribers = new ConcurrentHashMap<>();
    private final Map<String, EventFactory> typedEventFactories = new ConcurrentHashMap<>();
    private final ThreadedEventHandler eventHandler;
    public OSGiEventManager(ComponentContext componentContext) {
        eventHandler = new ThreadedEventHandler(typedEventSubscribers, typedEventFactories);
        eventHandler.open();
        eventHandler.close();
    protected void addEventSubscriber(final EventSubscriber eventSubscriber) {
        final Set<String> subscribedEventTypes = eventSubscriber.getSubscribedEventTypes();
        for (final String subscribedEventType : subscribedEventTypes) {
            final Set<EventSubscriber> entries = typedEventSubscribers.get(subscribedEventType);
            if (entries == null) {
                // Use a copy on write array set because the set is written and read by different threads!
                typedEventSubscribers.put(subscribedEventType, new CopyOnWriteArraySet<>(Set.of(eventSubscriber)));
                entries.add(eventSubscriber);
    protected void removeEventSubscriber(EventSubscriber eventSubscriber) {
                entries.remove(eventSubscriber);
                if (entries.isEmpty()) {
                    typedEventSubscribers.remove(subscribedEventType);
    protected void addEventFactory(EventFactory eventFactory) {
        Set<String> supportedEventTypes = eventFactory.getSupportedEventTypes();
        for (String supportedEventType : supportedEventTypes) {
                if (!typedEventFactories.containsKey(supportedEventType)) {
                    typedEventFactories.put(supportedEventType, eventFactory);
    protected void removeEventFactory(EventFactory eventFactory) {
            typedEventFactories.remove(supportedEventType);
    public void handleEvent(@Nullable Event osgiEvent) {
        if (osgiEvent != null) {
            eventHandler.handleEvent(osgiEvent);
