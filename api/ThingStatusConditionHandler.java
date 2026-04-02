import org.openhab.core.thing.events.ThingAddedEvent;
import org.openhab.core.thing.events.ThingRemovedEvent;
 * ConditionHandler implementation to check the thing status
 * @author Jörg Sautter - Initial contribution based on the ItemStateConditionHandler
public class ThingStatusConditionHandler extends BaseConditionModuleHandler implements EventSubscriber {
     * Constants for Config-Parameters corresponding to Definition in ThingConditions.json
    public static final String CFG_THING_UID = "thingUID";
    public static final String CFG_OPERATOR = "operator";
    public static final String CFG_STATUS = "status";
    private final Logger logger = LoggerFactory.getLogger(ThingStatusConditionHandler.class);
    public static final String THING_STATUS_CONDITION = "core.ThingStatusCondition";
    private final String thingUID;
    public ThingStatusConditionHandler(Condition condition, String ruleUID, BundleContext bundleContext,
            ThingRegistry thingRegistry) {
        this.thingUID = (String) module.getConfiguration().get(CFG_THING_UID);
        this.eventFilter = new TopicPrefixEventFilter("openhab/things/" + thingUID + "/");
        this.types = Set.of(ThingAddedEvent.TYPE, ThingRemovedEvent.TYPE);
        if (thingUID == null || thingRegistry.get(new ThingUID(thingUID)) == null) {
            logger.warn("Thing '{}' needed for rule '{}' is missing. Condition '{}' will not work.", thingUID, ruleUID,
        if ((event instanceof ThingAddedEvent addedEvent) && thingUID.equals(addedEvent.getThing().UID)) {
            logger.info("Thing '{}' needed for rule '{}' added. Condition '{}' will now work.", thingUID, ruleUID,
        } else if ((event instanceof ThingRemovedEvent removedEvent) && thingUID.equals(removedEvent.getThing().UID)) {
            logger.warn("Thing '{}' needed for rule '{}' removed. Condition '{}' will no longer work.", thingUID,
        String rawStatus = (String) module.getConfiguration().get(CFG_STATUS);
        ThingStatus status;
            status = ThingStatus.valueOf(rawStatus);
            status = null;
        String operator = (String) module.getConfiguration().get(CFG_OPERATOR);
        if (operator == null || status == null || thingUID == null) {
            logger.error("Module is not well configured: thingUID={}  operator={}  status = {} for rule {}", thingUID,
                    operator, rawStatus, ruleUID);
        logger.debug("ThingStatusCondition '{}' checking if {} {} {} for rule {}", module.getId(), thingUID, operator,
                status, ruleUID);
        Thing thing = thingRegistry.get(new ThingUID(thingUID));
        if (thing == null) {
            logger.error("Thing with UID {} not found in ThingRegistry for condition of rule {}.", thingUID, ruleUID);
                    return thing.getStatus().equals(status);
                    return !thing.getStatus().equals(status);
                    logger.error("Thing status condition operator {} is not known of rule {}", operator, ruleUID);
