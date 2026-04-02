 * The {@link ItemTimeSeriesEvent} can be used to report item time series updates through the openHAB event bus.
 * Time series events must be created with the {@link ItemEventFactory}.
public class ItemTimeSeriesEvent extends ItemEvent {
    public static final String TYPE = ItemTimeSeriesEvent.class.getSimpleName();
    protected final TimeSeries timeSeries;
     * Constructs a new item time series event.
    protected ItemTimeSeriesEvent(String topic, String payload, String itemName, TimeSeries timeSeries,
        this.timeSeries = timeSeries;
     * Gets the item time series.
     * @return the item time series
        return String.format("Item '%s' shall process time series with %d values.", itemName, timeSeries.size());
