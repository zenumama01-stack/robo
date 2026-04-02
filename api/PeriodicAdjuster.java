import java.time.temporal.Temporal;
 * This is a Temporal Adjuster that takes a list of delays.
 * The given delays are used sequentially.
 * If no more values are present, the last value is re-used.
 * This scheduler runs as a fixed rate scheduler from the first
 * time adjustInto is called.
 * @author Hilbrand Bouwkamp - implemented adjuster as a fixed rate scheduler
class PeriodicAdjuster implements SchedulerTemporalAdjuster {
    private final Iterator<Duration> iterator;
    private @Nullable Duration current;
    private @Nullable Temporal timeDone;
    PeriodicAdjuster(Duration... delays) {
        iterator = Arrays.stream(delays).iterator();
    public boolean isDone(Temporal temporal) {
    public Temporal adjustInto(@Nullable Temporal temporal) {
        if (timeDone == null) {
            timeDone = temporal;
            current = iterator.next();
        Temporal nextTime = timeDone.plus(current);
        timeDone = nextTime;
        return nextTime;
