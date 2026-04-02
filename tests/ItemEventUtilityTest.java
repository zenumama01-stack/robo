import org.openhab.core.io.websocket.event.EventProcessingException;
import org.openhab.core.items.events.ItemTimeSeriesEvent;
import org.openhab.core.library.items.StringItem;
 * The {@link ItemEventUtilityTest} contains tests for the {@link ItemEventUtility} class.
public class ItemEventUtilityTest {
    private static final String EXISTING_ITEM_NAME = "existingItem";
    private static final String NON_EXISTING_ITEM_NAME = "nonExistingItem";
    private static final StringType ITEM_STATE = new StringType("foo");
    private StringItem existingItem = new StringItem(EXISTING_ITEM_NAME);
    public void setUp() throws ItemNotFoundException {
        when(itemRegistry.getItem(eq(EXISTING_ITEM_NAME))).thenReturn(existingItem);
        when(itemRegistry.getItem(eq(NON_EXISTING_ITEM_NAME)))
                .thenThrow(new ItemNotFoundException(NON_EXISTING_ITEM_NAME));
    public void validStateEvent() throws EventProcessingException {
        ItemEvent event = ItemEventFactory.createStateEvent(EXISTING_ITEM_NAME, ITEM_STATE);
        Event itemEvent = itemEventUtility.createStateEvent(eventDTO);
        assertThat(itemEvent, is(event));
    public void validStateEventWithMissingItem() {
        ItemEvent event = ItemEventFactory.createStateEvent(NON_EXISTING_ITEM_NAME, ITEM_STATE);
        EventProcessingException e = assertThrows(EventProcessingException.class,
                () -> itemEventUtility.createStateEvent(eventDTO));
        assertThat(e.getMessage(), is("Could not find item '" + NON_EXISTING_ITEM_NAME + "' in registry."));
    public void validStateEventWithInvalidState() {
        ItemEvent event = ItemEventFactory.createStateEvent(EXISTING_ITEM_NAME, DecimalType.ZERO);
        assertThat(e.getMessage(), is("Incompatible datatype, rejected."));
    public void invalidStateEventTopic() {
        ItemEvent event = ItemEventFactory.createCommandEvent(EXISTING_ITEM_NAME, HSBType.BLACK);
        assertThat(e.getMessage(), is("Topic does not match event type."));
    public void invalidStateEventPayload() {
        ItemEvent event = ItemEventFactory.createStateEvent(EXISTING_ITEM_NAME, HSBType.BLACK);
        eventDTO.payload = "invalidNoJson";
        assertThat(e.getMessage(), is("Failed to deserialize payload 'invalidNoJson'."));
    public void validCommandEvent() throws EventProcessingException {
        ItemEvent event = ItemEventFactory.createCommandEvent(EXISTING_ITEM_NAME, ITEM_STATE);
        Event itemEvent = itemEventUtility.createCommandEvent(eventDTO);
    public void validCommandEventWithMissingItem() {
    public void validCommandEventWithInvalidState() {
                () -> itemEventUtility.createCommandEvent(eventDTO));
    public void invalidCommandEvent() {
    public void invalidCommandEventPayload() {
    public void validTimeSeriesEvent() throws EventProcessingException {
        TimeSeries timeSeries = new TimeSeries(TimeSeries.Policy.REPLACE);
        timeSeries.add(Instant.now(), OnOffType.ON);
        timeSeries.add(Instant.now().plusSeconds(5), OnOffType.OFF);
        ItemTimeSeriesEvent event = ItemEventFactory.createTimeSeriesEvent(EXISTING_ITEM_NAME, timeSeries, null);
        Event itemEvent = itemEventUtility.createTimeSeriesEvent(eventDTO);
