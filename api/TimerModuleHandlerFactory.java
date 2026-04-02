 * This HandlerFactory creates TimerTriggerHandlers to control items within the
 * RuleManager.
 * @author Kai Kreuzer - added new module types
@Component(immediate = true, service = ModuleHandlerFactory.class)
public class TimerModuleHandlerFactory extends BaseModuleHandlerFactory {
    private final Logger logger = LoggerFactory.getLogger(TimerModuleHandlerFactory.class);
    public static final String THREADPOOLNAME = "ruletimer";
    private static final Collection<String> TYPES = Arrays.asList(GenericCronTriggerHandler.MODULE_TYPE_ID,
            TimeOfDayTriggerHandler.MODULE_TYPE_ID, TimeOfDayConditionHandler.MODULE_TYPE_ID,
            DayOfWeekConditionHandler.MODULE_TYPE_ID, DateTimeTriggerHandler.MODULE_TYPE_ID,
            IntervalConditionHandler.MODULE_TYPE_ID);
    public TimerModuleHandlerFactory(final @Reference CronScheduler scheduler,
            final @Reference ItemRegistry itemRegistry, final BundleContext bundleContext) {
                case GenericCronTriggerHandler.MODULE_TYPE_ID:
                    return new GenericCronTriggerHandler(trigger, scheduler);
                case TimeOfDayTriggerHandler.MODULE_TYPE_ID:
                    return new TimeOfDayTriggerHandler(trigger, scheduler);
                case DateTimeTriggerHandler.MODULE_TYPE_ID:
                    return new DateTimeTriggerHandler(trigger, scheduler, itemRegistry, bundleContext);
                case TimeOfDayConditionHandler.MODULE_TYPE_ID:
                    return new TimeOfDayConditionHandler(condition);
                case DayOfWeekConditionHandler.MODULE_TYPE_ID:
                    return new DayOfWeekConditionHandler(condition);
                case IntervalConditionHandler.MODULE_TYPE_ID:
                    return new IntervalConditionHandler(condition);
        logger.error("The module handler type '{}' is not supported.", moduleTypeUID);
