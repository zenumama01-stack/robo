 * The {@link PersistenceTimeFilter} is a filter to prevent persistence base on intervals.
 * The filter returns {@code false} if the time between now and the time of the last persisted value is less than
 * {@link #duration} {@link #unit}
public class PersistenceTimeFilter extends PersistenceFilter {
    private transient @Nullable Duration duration;
    private final transient Map<String, ZonedDateTime> nextPersistenceTimes = new HashMap<>();
    public PersistenceTimeFilter(String name, int value, @Nullable String unit) {
        this.unit = (unit == null) ? "s" : unit;
        ZonedDateTime nextPersistenceTime = nextPersistenceTimes.get(itemName);
        return nextPersistenceTime == null || !now.isBefore(nextPersistenceTime);
        Duration duration = this.duration;
        if (duration == null) {
            duration = switch (unit) {
                case "m" -> Duration.of(value, ChronoUnit.MINUTES);
                case "h" -> Duration.of(value, ChronoUnit.HOURS);
                case "d" -> Duration.of(value, ChronoUnit.DAYS);
                default -> Duration.of(value, ChronoUnit.SECONDS);
            this.duration = duration;
        nextPersistenceTimes.put(item.getName(), ZonedDateTime.now().plus(duration));
        return String.format("%s [name=%s, value=%s, unit=%s]", getClass().getSimpleName(), getName(), value, unit);
