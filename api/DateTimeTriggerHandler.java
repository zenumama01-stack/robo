import java.time.format.DateTimeFormatter;
import org.openhab.core.automation.events.TimerEvent;
import org.openhab.core.events.TopicPrefixEventFilter;
import org.openhab.core.items.events.ItemStateChangedEvent;
import org.openhab.core.scheduler.CronAdjuster;
import org.openhab.core.scheduler.CronScheduler;
 * This is a ModuleHandler implementation for Triggers which trigger the rule
 * based on a {@link org.openhab.core.library.types.DateTimeType} stored in an item
 * @author Jimmy Tanagra - Add offset support
public class DateTimeTriggerHandler extends BaseTriggerModuleHandler
        implements SchedulerRunnable, TimeBasedTriggerHandler, EventSubscriber {
    public static final String MODULE_TYPE_ID = "timer.DateTimeTrigger";
    public static final String CONFIG_ITEM_NAME = "itemName";
    public static final String CONFIG_TIME_ONLY = "timeOnly";
    public static final String CONFIG_OFFSET = "offset";
    private static final DateTimeFormatter CRON_FORMATTER = DateTimeFormatter.ofPattern("s m H d M * uuuu");
    private static final DateTimeFormatter CRON_TIMEONLY_FORMATTER = DateTimeFormatter.ofPattern("s m H * * * *");
    private final Logger logger = LoggerFactory.getLogger(DateTimeTriggerHandler.class);
    private final CronScheduler scheduler;
    private final String itemName;
    private final @Nullable EventFilter eventFilter;
    private String cronExpression = CronAdjuster.REBOOT;
    private Boolean timeOnly = false;
    private Long offset = 0L;
    private @Nullable ScheduledCompletableFuture<?> schedule;
    private @Nullable ServiceRegistration<?> eventSubscriberRegistration;
    public DateTimeTriggerHandler(Trigger module, CronScheduler scheduler, ItemRegistry itemRegistry,
            BundleContext bundleContext) {
        this.itemName = ConfigParser.valueAsOrElse(module.getConfiguration().get(CONFIG_ITEM_NAME), String.class, "");
        if (this.itemName.isBlank()) {
            logger.warn("itemName is blank in module '{}', trigger will not work", module.getId());
            eventFilter = null;
        this.eventFilter = new TopicPrefixEventFilter("openhab/items/" + itemName + "/");
        this.timeOnly = ConfigParser.valueAsOrElse(module.getConfiguration().get(CONFIG_TIME_ONLY), Boolean.class,
        this.offset = ConfigParser.valueAsOrElse(module.getConfiguration().get(CONFIG_OFFSET), Long.class, 0L);
        eventSubscriberRegistration = bundleContext.registerService(EventSubscriber.class.getName(), this, null);
            process(itemRegistry.getItem(itemName).getState());
            logger.info("Could not determine initial state for item '{}' in trigger '{}', waiting for event", itemName,
                    module.getId());
        ServiceRegistration<?> eventSubscriberRegistration = this.eventSubscriberRegistration;
        if (eventSubscriberRegistration != null) {
            this.eventSubscriberRegistration = null;
        cancelScheduler();
    public synchronized void setCallback(ModuleHandlerCallback callback) {
        super.setCallback(callback);
        startScheduler();
        ModuleHandlerCallback callback = this.callback;
        if (callback instanceof TriggerHandlerCallback triggerHandlerCallback) {
            TimerEvent event = AutomationEventFactory.createTimerEvent(module.getTypeUID(),
                    Objects.requireNonNullElse(module.getLabel(), module.getId()),
                    Map.of(CONFIG_ITEM_NAME, itemName, CONFIG_TIME_ONLY, timeOnly, CONFIG_OFFSET, offset));
            triggerHandlerCallback.triggered(module, Map.of("event", event));
            logger.debug("Tried to trigger, but callback isn't available!");
    public CronAdjuster getTemporalAdjuster() {
        return new CronAdjuster(cronExpression);
        return Set.of(ItemStateChangedEvent.TYPE);
        return eventFilter;
        if (event instanceof ItemStateChangedEvent itemStateChangedEvent
                && (itemStateChangedEvent.getItemName().equals(itemName))) {
            process(itemStateChangedEvent.getItemState());
    private synchronized void startScheduler() {
        if (!CronAdjuster.REBOOT.equals(cronExpression)) {
            schedule = scheduler.schedule(this, cronExpression);
            logger.debug("Scheduled cron job '{}' from item '{}' for trigger '{}'.", cronExpression,
                    module.getConfiguration().get(CONFIG_ITEM_NAME), module.getId());
    private synchronized void cancelScheduler() {
        ScheduledCompletableFuture<?> schedule = this.schedule;
        if (schedule != null) {
            schedule.cancel(true);
            this.schedule = null;
            logger.debug("Cancelled job for trigger '{}'.", module.getId());
    private void process(Type value) {
        if (value instanceof UnDefType) {
            cronExpression = CronAdjuster.REBOOT;
        } else if (value instanceof DateTimeType dateTimeType) {
            boolean itemIsTimeOnly = dateTimeType.toString().startsWith("1970-01-01T");
            cronExpression = dateTimeType.getZonedDateTime(ZoneId.systemDefault()).plusSeconds(offset.longValue())
                    .format(timeOnly || itemIsTimeOnly ? CRON_TIMEONLY_FORMATTER : CRON_FORMATTER);
            logger.warn("Received {} which is not an accepted value for trigger of type '{}", value, MODULE_TYPE_ID);
