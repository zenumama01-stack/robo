 * This is a ConditionHandler implementation, which checks the current day of the week against a specified list.
public class DayOfWeekConditionHandler extends BaseConditionModuleHandler implements TimeBasedConditionHandler {
    public static final String MODULE_TYPE_ID = "timer.DayOfWeekCondition";
    public static final String MODULE_CONTEXT_NAME = "MODULE";
    public static final String CFG_DAYS = "days";
    private final Logger logger = LoggerFactory.getLogger(DayOfWeekConditionHandler.class);
    private final Set<DayOfWeek> days;
    public DayOfWeekConditionHandler(Condition module) {
            days = new HashSet<>();
            for (String day : (Iterable<String>) module.getConfiguration().get(CFG_DAYS)) {
                switch (day.toUpperCase()) {
                    case "SUN":
                        days.add(DayOfWeek.SUNDAY);
                    case "MON":
                        days.add(DayOfWeek.MONDAY);
                    case "TUE":
                        days.add(DayOfWeek.TUESDAY);
                    case "WED":
                        days.add(DayOfWeek.WEDNESDAY);
                    case "THU":
                        days.add(DayOfWeek.THURSDAY);
                    case "FRI":
                        days.add(DayOfWeek.FRIDAY);
                    case "SAT":
                        days.add(DayOfWeek.SATURDAY);
                        logger.warn("Ignoring illegal weekday '{}'", day);
            throw new IllegalArgumentException("'days' parameter must be an array of strings.");
        return isSatisfiedAt(ZonedDateTime.now());
    public boolean isSatisfiedAt(ZonedDateTime time) {
        DayOfWeek dow = time.getDayOfWeek();
        return days.contains(dow);
