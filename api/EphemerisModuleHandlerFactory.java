import org.openhab.core.automation.internal.module.handler.EphemerisConditionHandler;
import org.openhab.core.ephemeris.EphemerisManager;
 * This HandlerFactory creates ModuleHandlers to control items within the RuleManager. It contains basic Ephemeris
 * Conditions.
public class EphemerisModuleHandlerFactory extends BaseModuleHandlerFactory implements ModuleHandlerFactory {
    private final Logger logger = LoggerFactory.getLogger(EphemerisModuleHandlerFactory.class);
    private static final Collection<String> TYPES = List.of(EphemerisConditionHandler.HOLIDAY_MODULE_TYPE_ID,
            EphemerisConditionHandler.NOT_HOLIDAY_MODULE_TYPE_ID, EphemerisConditionHandler.WEEKEND_MODULE_TYPE_ID,
            EphemerisConditionHandler.WEEKDAY_MODULE_TYPE_ID, EphemerisConditionHandler.DAYSET_MODULE_TYPE_ID);
    private final EphemerisManager ephemerisManager;
    public EphemerisModuleHandlerFactory(final @Reference EphemerisManager ephemerisManager) {
        this.ephemerisManager = ephemerisManager;
    protected @Nullable ModuleHandler internalCreate(final Module module, final String ruleUID) {
        logger.trace("create {} -> {} : {}", module.getId(), moduleTypeUID, ruleUID);
            switch (moduleTypeUID) {
                case EphemerisConditionHandler.HOLIDAY_MODULE_TYPE_ID:
                case EphemerisConditionHandler.NOT_HOLIDAY_MODULE_TYPE_ID:
                case EphemerisConditionHandler.WEEKEND_MODULE_TYPE_ID:
                case EphemerisConditionHandler.WEEKDAY_MODULE_TYPE_ID:
                case EphemerisConditionHandler.DAYSET_MODULE_TYPE_ID:
                    return new EphemerisConditionHandler(condition, ephemerisManager);
