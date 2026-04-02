import java.text.MessageFormat;
 * at a specific time (format 'hh:mm').
public class TimeOfDayTriggerHandler extends BaseTriggerModuleHandler
    private final Logger logger = LoggerFactory.getLogger(TimeOfDayTriggerHandler.class);
    public static final String MODULE_TYPE_ID = "timer.TimeOfDayTrigger";
    public static final String CFG_TIME = "time";
    private final String time;
    public TimeOfDayTriggerHandler(Trigger module, CronScheduler scheduler) {
        this.time = module.getConfiguration().get(CFG_TIME).toString();
        this.expression = buildExpressionFromConfigurationTime(time);
     * Creates a cron-Expression from the configured time.
    private static String buildExpressionFromConfigurationTime(String time) {
            String[] parts = time.split(":");
            return MessageFormat.format("0 {1} {0} * * *", Integer.parseInt(parts[0]), Integer.parseInt(parts[1]));
        } catch (ArrayIndexOutOfBoundsException | NumberFormatException e) {
            throw new IllegalArgumentException("'time' parameter '" + time + "' is not in valid format 'hh:mm'.", e);
        logger.debug("Scheduled job for trigger '{}' at '{}' each day.", module.getId(),
                module.getConfiguration().get(CFG_TIME));
                    Objects.requireNonNullElse(module.getLabel(), module.getId()), Map.of(CFG_TIME, time));
