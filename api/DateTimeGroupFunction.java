 * This interface is a container for group functions that require {@link DateTimeType}s for its calculations.
 * @author Robert Michalak - Initial contribution
public interface DateTimeGroupFunction extends GroupFunction {
     * This calculates the maximum value of all item states of DateType type.
    class Latest implements GroupFunction {
        public Latest() {
                Instant max = null;
                    DateTimeType itemState = item.getStateAs(DateTimeType.class);
                        if (max == null || max.isBefore(itemState.getInstant())) {
                            max = itemState.getInstant();
                    return new DateTimeType(max);
     * This calculates the minimum value of all item states of DateType type.
    class Earliest implements GroupFunction {
        public Earliest() {
                        if (max == null || max.isAfter(itemState.getInstant())) {
