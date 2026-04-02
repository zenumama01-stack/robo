 * The {@link TimeSeriesProfile} extends the {@link StateProfile} to support {@link TimeSeries} updates
public interface TimeSeriesProfile extends StateProfile {
     * If a binding sends a time-series to a channel, this method will be called for each linked item.
     * @param timeSeries the time-series
    void onTimeSeriesFromHandler(TimeSeries timeSeries);
