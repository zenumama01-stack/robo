 * based on a cron expression. The cron expression can be set with the
 * configuration.
 * @author Christoph Knauf - Initial contribution
 * @author Yordan Mihaylov - Remove Quarz lib dependency
public class GenericCronTriggerHandler extends BaseTriggerModuleHandler
        implements SchedulerRunnable, TimeBasedTriggerHandler {
    public static final String MODULE_TYPE_ID = "timer.GenericCronTrigger";
    public static final String CALLBACK_CONTEXT_NAME = "CALLBACK";
    public static final String CFG_CRON_EXPRESSION = "cronExpression";
    private final Logger logger = LoggerFactory.getLogger(GenericCronTriggerHandler.class);
    private final String expression;
    public GenericCronTriggerHandler(Trigger module, CronScheduler scheduler) {
        this.expression = (String) module.getConfiguration().get(CFG_CRON_EXPRESSION);
        scheduleJob();
    private void scheduleJob() {
            schedule = scheduler.schedule(this, expression);
            logger.debug("Scheduled cron job '{}' for trigger '{}'.",
                    module.getConfiguration().get(CFG_CRON_EXPRESSION), module.getId());
        } catch (IllegalArgumentException e) { // Catch exception from CronAdjuster
            logger.warn("Failed to schedule job for trigger '{}'. {}", module.getId(), e.getMessage());
    public synchronized void dispose() {
            logger.debug("cancelled job for trigger '{}'.", module.getId());
                    Map.of(CFG_CRON_EXPRESSION, expression));
            ((TriggerHandlerCallback) callback).triggered(module, Map.of("event", event));
        return new CronAdjuster(expression);
