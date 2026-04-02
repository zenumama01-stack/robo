import org.openhab.core.model.script.actions.Things;
 * This class provides methods for interacting with Things in scripts.
public class ThingActionService implements ActionService {
    private static @Nullable ThingRegistry thingRegistry;
    public ThingActionService(final @Reference ThingRegistry thingRegistry) {
        ThingActionService.thingRegistry = thingRegistry;
        return Things.class;
    public static @Nullable ThingStatusInfo getThingStatusInfo(String thingUid) {
            return thing.getStatusInfo();
    public static @Nullable ThingActions getActions(String scope, String thingUid) {
    private static String getKey(ThingActions thingActions) {
    private static String getThingUID(ThingActions actions) {
        return actions.getThingHandler().getThing().getUID().getAsString();
