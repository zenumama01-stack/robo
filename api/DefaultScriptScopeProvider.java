package org.openhab.core.automation.module.script.internal.defaultscope;
import java.nio.file.Paths;
import java.time.DayOfWeek;
import java.time.Month;
import org.openhab.core.library.types.DecimalType;
import org.openhab.core.library.types.HSBType;
import org.openhab.core.library.types.IncreaseDecreaseType;
import org.openhab.core.library.types.NextPreviousType;
import org.openhab.core.library.types.OnOffType;
import org.openhab.core.library.types.OpenClosedType;
import org.openhab.core.library.types.PlayPauseType;
import org.openhab.core.library.types.PointType;
import org.openhab.core.library.types.QuantityType;
import org.openhab.core.library.types.RawType;
import org.openhab.core.library.types.RewindFastforwardType;
import org.openhab.core.library.types.StopMoveType;
import org.openhab.core.library.types.StringListType;
import org.openhab.core.library.types.StringType;
import org.openhab.core.library.types.UpDownType;
import org.openhab.core.library.unit.BinaryPrefix;
import org.openhab.core.library.unit.ImperialUnits;
import org.openhab.core.library.unit.MetricPrefix;
import org.openhab.core.library.unit.SIUnits;
import org.openhab.core.library.unit.Units;
import org.openhab.core.types.RefreshType;
import org.openhab.core.types.UnDefType;
 * This is a default scope provider for stuff that is of general interest in an OH-based solution.
 * Nonetheless, solutions are free to remove it and have more specific scope providers for their own purposes.
 * @author Simon Merschjohann - Refactored to be a {@link ScriptExtensionProvider}
public class DefaultScriptScopeProvider implements ScriptExtensionProvider {
    private static final String PRESET_DEFAULT = "default";
    private final Map<String, Object> elements = new ConcurrentHashMap<>();
    private final ScriptThingActionsImpl thingActions;
    public DefaultScriptScopeProvider(final @Reference ItemRegistry itemRegistry,
            final @Reference ThingRegistry thingRegistry, final @Reference RuleRegistry ruleRegistry,
            final @Reference BusEvent busEvent) {
        this.thingActions = new ScriptThingActionsImpl(thingRegistry);
        elements.put("State", State.class);
        elements.put("Command", Command.class);
        elements.put("URLEncoder", URLEncoder.class);
        elements.put("File", File.class);
        elements.put("Files", Files.class);
        elements.put("Path", Path.class);
        elements.put("Paths", Paths.class);
        // types
        elements.put("IncreaseDecreaseType", IncreaseDecreaseType.class);
        elements.put("DECREASE", IncreaseDecreaseType.DECREASE);
        elements.put("INCREASE", IncreaseDecreaseType.INCREASE);
        elements.put("OnOffType", OnOffType.class);
        elements.put("ON", OnOffType.ON);
        elements.put("OFF", OnOffType.OFF);
        elements.put("OpenClosedType", OpenClosedType.class);
        elements.put("CLOSED", OpenClosedType.CLOSED);
        elements.put("OPEN", OpenClosedType.OPEN);
        elements.put("StopMoveType", StopMoveType.class);
        elements.put("MOVE", StopMoveType.MOVE);
        elements.put("STOP", StopMoveType.STOP);
        elements.put("UpDownType", UpDownType.class);
        elements.put("DOWN", UpDownType.DOWN);
        elements.put("UP", UpDownType.UP);
        elements.put("UnDefType", UnDefType.class);
        elements.put("NULL", UnDefType.NULL);
        elements.put("UNDEF", UnDefType.UNDEF);
        elements.put("RefreshType", RefreshType.class);
        elements.put("REFRESH", RefreshType.REFRESH);
        elements.put("NextPreviousType", NextPreviousType.class);
        elements.put("NEXT", NextPreviousType.NEXT);
        elements.put("PREVIOUS", NextPreviousType.PREVIOUS);
        elements.put("PlayPauseType", PlayPauseType.class);
        elements.put("PLAY", PlayPauseType.PLAY);
        elements.put("PAUSE", PlayPauseType.PAUSE);
        elements.put("RewindFastforwardType", RewindFastforwardType.class);
        elements.put("REWIND", RewindFastforwardType.REWIND);
        elements.put("FASTFORWARD", RewindFastforwardType.FASTFORWARD);
        elements.put("QuantityType", QuantityType.class);
        elements.put("StringListType", StringListType.class);
        elements.put("RawType", RawType.class);
        elements.put("DateTimeType", DateTimeType.class);
        elements.put("DecimalType", DecimalType.class);
        elements.put("HSBType", HSBType.class);
        elements.put("PercentType", PercentType.class);
        elements.put("PointType", PointType.class);
        elements.put("StringType", StringType.class);
        elements.put("ImperialUnits", ImperialUnits.class);
        elements.put("MetricPrefix", MetricPrefix.class);
        elements.put("SIUnits", SIUnits.class);
        elements.put("Units", Units.class);
        elements.put("BinaryPrefix", BinaryPrefix.class);
        // date time static functions
        elements.put("ChronoUnit", ChronoUnit.class);
        elements.put("DayOfWeek", DayOfWeek.class);
        elements.put("Duration", Duration.class);
        elements.put("Month", Month.class);
        elements.put("ZoneId", ZoneId.class);
        elements.put("ZonedDateTime", ZonedDateTime.class);
        // services
        elements.put("items", new ItemRegistryDelegate(itemRegistry));
        elements.put("ir", itemRegistry);
        elements.put("itemRegistry", itemRegistry);
        elements.put("things", thingRegistry);
        elements.put("rules", ruleRegistry);
        elements.put("events", busEvent);
        elements.put("actions", thingActions);
    @Reference(policy = ReferencePolicy.DYNAMIC, cardinality = ReferenceCardinality.MULTIPLE)
    synchronized void addThingActions(ThingActions thingActions) {
        this.thingActions.addThingActions(thingActions);
        elements.put(thingActions.getClass().getSimpleName(), thingActions.getClass());
    protected void removeThingActions(ThingActions thingActions) {
        elements.remove(thingActions.getClass().getSimpleName());
        this.thingActions.removeThingActions(thingActions);
        thingActions.dispose();
        elements.clear();
        return Set.of(PRESET_DEFAULT);
        if (PRESET_DEFAULT.equals(preset)) {
            return Collections.unmodifiableMap(elements);
        // nothing todo
