import org.openhab.core.automation.internal.module.config.EphemerisConditionConfig;
public class EphemerisConditionHandler extends BaseModuleHandler<Condition> implements TimeBasedConditionHandler {
    public static final String HOLIDAY_MODULE_TYPE_ID = "ephemeris.HolidayCondition";
    public static final String NOT_HOLIDAY_MODULE_TYPE_ID = "ephemeris.NotHolidayCondition";
    public static final String WEEKEND_MODULE_TYPE_ID = "ephemeris.WeekendCondition";
    public static final String WEEKDAY_MODULE_TYPE_ID = "ephemeris.WeekdayCondition";
    public static final String DAYSET_MODULE_TYPE_ID = "ephemeris.DaysetCondition";
    private final @Nullable String dayset;
    private final int offset;
    public EphemerisConditionHandler(Condition condition, EphemerisManager ephemerisManager) {
        EphemerisConditionConfig config = getConfigAs(EphemerisConditionConfig.class);
        offset = config.offset;
        dayset = DAYSET_MODULE_TYPE_ID.equals(module.getTypeUID())
                ? getValidStringConfigParameter(config.dayset, module.getId())
    private static String getValidStringConfigParameter(@Nullable String value, String moduleId) {
        if (value != null && !value.trim().isEmpty()) {
            throw new IllegalArgumentException(String
                    .format("Config parameter 'dayset' is missing in the configuration of module '%s'.", moduleId));
        ZonedDateTime offsetTime = time.plusDays(offset); // Apply offset to time
            case HOLIDAY_MODULE_TYPE_ID:
                return ephemerisManager.isBankHoliday(offsetTime);
            case NOT_HOLIDAY_MODULE_TYPE_ID:
                return !ephemerisManager.isBankHoliday(offsetTime);
            case WEEKEND_MODULE_TYPE_ID:
                return ephemerisManager.isWeekend(offsetTime);
            case WEEKDAY_MODULE_TYPE_ID:
                return !ephemerisManager.isWeekend(offsetTime);
            case DAYSET_MODULE_TYPE_ID:
                final String dayset = this.dayset;
                if (dayset != null) {
                    return ephemerisManager.isInDayset(dayset, offsetTime);
        // If none of these conditions apply false is returned.
        final ZonedDateTime target = ZonedDateTime.now();
        return this.isSatisfiedAt(target);
