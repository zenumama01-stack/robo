 * ConditionHandler implementation for time based conditions.
 * @author Dominik Schlierf - Initial contribution
public class TimeOfDayConditionHandler extends BaseConditionModuleHandler implements TimeBasedConditionHandler {
    public static final String MODULE_TYPE_ID = "core.TimeOfDayCondition";
     * TimeOfDayConditionHandler.json
    public static final String CFG_START_TIME = "startTime";
    public static final String CFG_END_TIME = "endTime";
    private final Logger logger = LoggerFactory.getLogger(TimeOfDayConditionHandler.class);
     * The start time of the user configured time span.
    private final @Nullable LocalTime startTime;
     * The end time of the user configured time span.
    private final @Nullable LocalTime endTime;
    public TimeOfDayConditionHandler(Condition condition) {
        String startTimeConfig = (String) configuration.get(CFG_START_TIME);
        String endTimeConfig = (String) configuration.get(CFG_END_TIME);
        startTime = startTimeConfig == null ? null : LocalTime.parse(startTimeConfig).truncatedTo(ChronoUnit.MINUTES);
        endTime = endTimeConfig == null ? null : LocalTime.parse(endTimeConfig).truncatedTo(ChronoUnit.MINUTES);
        LocalTime startTime = this.startTime;
        LocalTime endTime = this.endTime;
        if (startTime == null || endTime == null) {
            logger.warn("Time condition with id {} is not well configured: startTime={}  endTime = {}", module.getId(),
                    startTime, endTime);
        LocalTime currentTime = time.toLocalTime().truncatedTo(ChronoUnit.MINUTES);
        // If the current time equals the start time, the condition is always true.
        if (currentTime.equals(startTime)) {
            logger.debug("Time condition with id {} evaluated, that the current time {} equals the start time: {}",
                    module.getId(), currentTime, startTime);
        // If the start time is before the end time, the condition will evaluate as true,
        // if the current time is between the start time and the end time.
        if (startTime.isBefore(endTime)) {
            if (currentTime.isAfter(startTime) && currentTime.isBefore(endTime)) {
                logger.debug("Time condition with id {} evaluated, that {} is between {} and {}.", module.getId(),
                        currentTime, startTime, endTime);
        // If the start time is set after the end time, the time values wrap around the midnight mark.
        // So if the start time is 19:00 and the end time is 07:00, the condition will be true from
        // 19:00 to 23:59 and 00:00 to 07:00.
        else if (currentTime.isAfter(LocalTime.MIDNIGHT) && currentTime.isBefore(endTime)
                || currentTime.isAfter(startTime) && currentTime.isBefore(LocalTime.MAX)) {
            logger.debug("Time condition with id {} evaluated, that {} is between {} and {}, or between {} and {}.",
                    module.getId(), currentTime, LocalTime.MIDNIGHT, endTime, startTime,
                    LocalTime.MAX.truncatedTo(ChronoUnit.MINUTES));
