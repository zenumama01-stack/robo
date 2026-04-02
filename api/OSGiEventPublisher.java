import org.osgi.service.event.EventAdmin;
 * The {@link OSGiEventPublisher} provides an OSGi based default implementation of the openHAB event
 * publisher.
 * Events are send in an asynchronous way via OSGi Event Admin mechanism.
 * @author Simon Kaufmann - separated from OSGiEventManager
public class OSGiEventPublisher implements EventPublisher {
    protected static final String SOURCE = "source";
    protected static final String TOPIC = "topic";
    protected static final String PAYLOAD = "payload";
    protected static final String TYPE = "type";
    private final @Nullable EventAdmin osgiEventAdmin;
    public OSGiEventPublisher(final @Reference @Nullable EventAdmin eventAdmin) {
        this.osgiEventAdmin = eventAdmin;
    public void post(final Event event) throws IllegalArgumentException, IllegalStateException {
        EventAdmin eventAdmin = this.osgiEventAdmin;
        assertValidArgument(event);
        assertValidState(eventAdmin);
        postAsOSGiEvent(eventAdmin, event);
    private void postAsOSGiEvent(final @Nullable EventAdmin eventAdmin, final Event event)
            Dictionary<String, Object> properties = new Hashtable<>(3);
            properties.put(TYPE, event.getType());
            properties.put(PAYLOAD, event.getPayload());
            properties.put(TOPIC, event.getTopic());
            if (event.getSource() instanceof String source) {
                properties.put(SOURCE, source);
            eventAdmin.postEvent(new org.osgi.service.event.Event("openhab", properties));
            throw new IllegalStateException("Cannot post the event via the event bus. Error message: " + e.getMessage(),
    private void assertValidArgument(Event event) throws IllegalArgumentException {
        String errorMsg = "The %s of the 'event' argument must not be null or empty.";
        if ((value = event.getType()) == null || value.isEmpty()) {
            throw new IllegalArgumentException(String.format(errorMsg, "type"));
        if ((value = event.getPayload()) == null || value.isEmpty()) {
            throw new IllegalArgumentException(String.format(errorMsg, "payload"));
        if ((value = event.getTopic()) == null || value.isEmpty()) {
            throw new IllegalArgumentException(String.format(errorMsg, "topic"));
    private void assertValidState(@Nullable EventAdmin eventAdmin) throws IllegalStateException {
        if (eventAdmin == null) {
            throw new IllegalStateException("The event bus module is not available!");
