import static org.openhab.core.magic.binding.MagicBindingConstants.CHANNEL_FORECAST;
import static org.openhab.core.types.TimeSeries.Policy.ADD;
 * The {@link MagicTimeSeriesHandler} is capable of providing a series of different forecasts
public class MagicTimeSeriesHandler extends BaseThingHandler {
    private Configuration configuration = new Configuration();
    public MagicTimeSeriesHandler(Thing thing) {
        configuration = getConfigAs(Configuration.class);
                TimeSeries timeSeries = new TimeSeries(ADD);
                Duration stepSize = Duration.ofSeconds(configuration.interval / configuration.count);
                double range = configuration.max - configuration.min;
                for (int i = 1; i <= configuration.count; i++) {
                    double value = switch (configuration.type) {
                        case RND -> ThreadLocalRandom.current().nextDouble() * range + configuration.min;
                        case ASC -> (range / configuration.count) * i + configuration.min;
                        case DESC -> configuration.max + (range / configuration.count) * i;
                    timeSeries.add(now.plus(stepSize.multipliedBy(i)), new DecimalType(value));
                sendTimeSeries(CHANNEL_FORECAST, timeSeries);
            }, 0, configuration.interval, TimeUnit.SECONDS);
    public static class Configuration {
        public int interval = 600;
        public Type type = Type.RND;
        public double min = 0.0;
        public double max = 100.0;
        public int count = 10;
        RND,
        ASC,
        DESC
