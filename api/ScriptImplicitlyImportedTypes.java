import org.eclipse.xtext.xbase.scoping.batch.ImplicitlyImportedFeatures;
import org.openhab.core.model.script.actions.BusEvent;
import org.openhab.core.model.script.actions.CoreUtil;
import org.openhab.core.model.script.actions.Exec;
import org.openhab.core.model.script.actions.HTTP;
import org.openhab.core.model.script.actions.Log;
import org.openhab.core.model.script.actions.Ping;
import org.openhab.core.model.script.actions.ScriptExecution;
import org.openhab.core.model.script.actions.Transformation;
import org.openhab.core.model.script.lib.NumberExtensions;
 * This class registers all statically available functions as well as the
 * extensions for specific jvm types, which should only be available in rules,
 * but not in scripts
 * @author Oliver Libutzki - Xtext 2.5.0 migration
public class ScriptImplicitlyImportedTypes extends ImplicitlyImportedFeatures {
    private List<Class<?>> actionClasses = null;
    IActionServiceProvider actionServiceProvider;
    IThingActionsProvider thingActionsProvider;
    protected List<Class<?>> getExtensionClasses() {
        List<Class<?>> result = super.getExtensionClasses();
        result.add(NumberExtensions.class);
        result.add(BusEvent.class);
        result.add(HTTP.class);
        result.add(Log.class);
        result.add(Ping.class);
        result.add(Transformation.class);
        result.add(ScriptExecution.class);
        result.add(URLEncoder.class);
        result.addAll(getActionClasses());
    protected List<Class<?>> getStaticImportClasses() {
        List<Class<?>> result = super.getStaticImportClasses();
        result.add(Exec.class);
        result.add(CoreUtil.class);
        result.add(ImperialUnits.class);
        result.add(MetricPrefix.class);
        result.add(SIUnits.class);
        result.add(Units.class);
        result.add(BinaryPrefix.class);
        result.add(ChronoUnit.class);
        result.add(DayOfWeek.class);
        result.add(Duration.class);
        result.add(Month.class);
        result.add(ZoneId.class);
        result.add(ZonedDateTime.class);
    protected List<Class<?>> getActionClasses() {
        List<Class<?>> localActionClasses = new ArrayList<>();
        List<ActionService> services = actionServiceProvider.get();
        if (services != null) {
            for (ActionService actionService : services) {
                localActionClasses.add(actionService.getActionClass());
        List<ThingActions> actions = thingActionsProvider.get();
            for (ThingActions thingActions : actions) {
                localActionClasses.add(thingActions.getClass());
        actionClasses = localActionClasses;
        return actionClasses;
