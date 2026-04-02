 * The {@link ItemTimeSeriesUpdatedEvent} can be used to report item time series updates through the openHAB event bus.
public class ItemTimeSeriesUpdatedEvent extends ItemEvent {
    public static final String TYPE = ItemTimeSeriesUpdatedEvent.class.getSimpleName();
     * Constructs a new item time series updated event.
    protected ItemTimeSeriesUpdatedEvent(String topic, String payload, String itemName, TimeSeries timeSeries,
        return String.format("Item '%s' updated time series with %d values.", itemName, timeSeries.size());
