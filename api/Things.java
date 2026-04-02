import org.openhab.core.model.script.internal.engine.action.ThingActionService;
 * getting thing's status info.
 * @author Maoliang Huang - Initial contribution
 * @author Kai Kreuzer - Extended for general thing access
public class Things {
     * Retrieves the status info of a Thing
     * @param thingUid The uid of the thing
     * @return <code>ThingStatusInfo</code>
    public static ThingStatusInfo getThingStatusInfo(String thingUid) {
        return ThingActionService.getThingStatusInfo(thingUid);
     * Get the actions instance for a Thing of a given scope
     * @param scope The action scope
     * @return the <code>ThingActions</code> instance
    public static ThingActions getActions(String scope, String thingUid) {
        return ThingActionService.getActions(scope, thingUid);
