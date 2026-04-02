 * ConditionHandler implementation for trigger interval limiting.
public class IntervalConditionHandler extends BaseConditionModuleHandler {
    public static final String MODULE_TYPE_ID = "timer.IntervalCondition";
     * Constants for Config-Parameters corresponding to Definition in
     * IntervalConditionHandler.json
    public static final String CFG_MIN_INTERVAL = "minInterval";
     * The minimum interval stored in nano seconds.
    private long minInterval;
    private @Nullable Long lastAcceptedTime = null;
    public IntervalConditionHandler(Condition condition) {
        this.minInterval = ((BigDecimal) configuration.get(CFG_MIN_INTERVAL)).longValue() * 1000000L;
        long currentTime = System.nanoTime();
        if (lastAcceptedTime == null || currentTime - lastAcceptedTime >= minInterval) {
            lastAcceptedTime = currentTime;
