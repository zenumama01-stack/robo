import org.openhab.core.thing.ThingStatusDetail;
import org.openhab.core.thing.ThingStatusInfo;
import org.openhab.core.thing.binding.builder.ThingBuilder;
 * Basic unit tests for {@link ThingStatusConditionHandler}.
 * @author Jörg Sautter - Initial contribution based on ItemStateConditionHandlerTest
public class ThingStatusConditionHandlerTest extends JavaTest {
        public final Thing thing;
        public final String comparisonStatus;
        public final ThingStatus thingStatus;
        public ParameterSet(String comparisonStatus, ThingStatus thingStatus, boolean expectedResult) {
            thing = ThingBuilder.create(new ThingTypeUID(BINDING_UID, THING_TYPE_UID), THING_UID).build();
            thing.setStatusInfo(new ThingStatusInfo(thingStatus, ThingStatusDetail.NONE, null));
            this.comparisonStatus = comparisonStatus;
            this.thingStatus = thingStatus;
                { new ParameterSet("UNINITIALIZED", ThingStatus.UNINITIALIZED, true) }, //
                { new ParameterSet("INITIALIZING", ThingStatus.UNINITIALIZED, false) }, //
                { new ParameterSet("OFFLINE", ThingStatus.UNKNOWN, false) }, //
                { new ParameterSet("OFFLINE", ThingStatus.ONLINE, false) }, //
                { new ParameterSet("OFFLINE", ThingStatus.OFFLINE, true) }, //
                { new ParameterSet("ONLINE", ThingStatus.ONLINE, true) }, //
                { new ParameterSet("ONLINE", ThingStatus.OFFLINE, false) } });
    private static final String BINDING_UID = "binding";
    private static final String THING_TYPE_UID = "type";
    private static final String THING_UID = "myThing";
    private @NonNullByDefault({}) Thing thing;
    private @Mock @NonNullByDefault({}) ThingRegistry mockThingRegistry;
        when(mockThingRegistry.get(new ThingUID(BINDING_UID, THING_TYPE_UID, THING_UID))).thenAnswer(i -> thing);
        thing = parameterSet.thing;
        ThingStatusConditionHandler handler = initThingStatusConditionHandler("=", parameterSet.comparisonStatus);
                    parameterSet.thing + ", comparisonStatus=" + parameterSet.comparisonStatus);
        ThingStatusConditionHandler handler = initThingStatusConditionHandler("!=", parameterSet.comparisonStatus);
    private ThingStatusConditionHandler initThingStatusConditionHandler(String operator, String state) {
        configuration.put(ThingStatusConditionHandler.CFG_THING_UID,
                BINDING_UID + ":" + THING_TYPE_UID + ":" + THING_UID);
        configuration.put(ThingStatusConditionHandler.CFG_OPERATOR, operator);
        configuration.put(ThingStatusConditionHandler.CFG_STATUS, state);
                .withTypeUID(ThingStatusConditionHandler.THING_STATUS_CONDITION) //
        return new ThingStatusConditionHandler(builder.build(), "", mockBundleContext, mockThingRegistry);
    public void thingMessagesAreLogged() {
        configuration.put(ThingStatusConditionHandler.CFG_OPERATOR, "=");
        setupInterceptedLogger(ThingStatusConditionHandler.class, LogLevel.INFO);
        when(mockThingRegistry.get(new ThingUID(BINDING_UID, THING_TYPE_UID, THING_UID))).thenReturn(null);
        ThingStatusConditionHandler handler = new ThingStatusConditionHandler(condition, "foo", mockBundleContext,
                mockThingRegistry);
        assertLogMessage(ThingStatusConditionHandler.class, LogLevel.WARN,
                "Thing 'binding:type:myThing' needed for rule 'foo' is missing. Condition 'conditionId' will not work.");
        ThingAddedEvent addedEvent = ThingEventFactory.createAddedEvent(thing);
        assertLogMessage(ThingStatusConditionHandler.class, LogLevel.INFO,
                "Thing 'binding:type:myThing' needed for rule 'foo' added. Condition 'conditionId' will now work.");
        ThingRemovedEvent removedEvent = ThingEventFactory.createRemovedEvent(thing);
                "Thing 'binding:type:myThing' needed for rule 'foo' removed. Condition 'conditionId' will no longer work.");
