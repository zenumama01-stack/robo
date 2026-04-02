import org.openhab.core.automation.module.script.defaultscope.ScriptThingActions;
import org.openhab.core.thing.binding.ThingActionsScope;
import org.openhab.core.thing.binding.ThingHandler;
 * Note: This class is a copy from the {@link org.openhab.core.model.script.internal.engine.action.ThingActionService}
 * @author Jan N. Klug - Moved implementation to internal class
public class ScriptThingActionsImpl implements ScriptThingActions {
    private static final Map<String, ThingActions> THING_ACTIONS_MAP = new HashMap<>();
    private @Nullable ThingRegistry thingRegistry;
    ScriptThingActionsImpl(ThingRegistry thingRegistry) {
        this.thingRegistry = null;
    public @Nullable ThingActions get(@Nullable String scope, @Nullable String thingUid) {
        ThingRegistry thingRegistry = this.thingRegistry;
        if (thingUid != null && scope != null && thingRegistry != null) {
            ThingUID uid = new ThingUID(thingUid);
            Thing thing = thingRegistry.get(uid);
                ThingHandler handler = thing.getHandler();
                if (handler != null) {
                    return THING_ACTIONS_MAP.get(getKey(scope, thingUid));
    void addThingActions(ThingActions thingActions) {
        String key = getKey(thingActions);
        if (key != null) {
            THING_ACTIONS_MAP.put(key, thingActions);
    void removeThingActions(ThingActions thingActions) {
        THING_ACTIONS_MAP.remove(key);
    private static @Nullable String getKey(ThingActions thingActions) {
        String scope = getScope(thingActions);
        String thingUID = getThingUID(thingActions);
        if (thingUID == null) {
            return getKey(scope, thingUID);
    private static String getKey(String scope, String thingUID) {
        return scope + "-" + thingUID;
    private static @Nullable String getThingUID(ThingActions actions) {
        ThingHandler thingHandler = actions.getThingHandler();
        if (thingHandler == null) {
        return thingHandler.getThing().getUID().getAsString();
    private static String getScope(ThingActions actions) {
        ThingActionsScope scopeAnnotation = actions.getClass().getAnnotation(ThingActionsScope.class);
        return scopeAnnotation.name();
