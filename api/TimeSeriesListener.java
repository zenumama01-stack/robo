 * This interface must be implemented by all classes that want to be notified about |@link TimeSeries} updates of an
 * item.
public interface TimeSeriesListener {
     * This method is called, if a time series update was sent to the item.
     * @param item the item the timeseries was updated for
     * @param timeSeries the time series
    void timeSeriesUpdated(Item item, TimeSeries timeSeries);
