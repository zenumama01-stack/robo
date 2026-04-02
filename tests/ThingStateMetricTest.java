import static org.mockito.Mockito.doReturn;
import org.openhab.core.thing.internal.ThingImpl;
import io.micrometer.core.instrument.simple.SimpleMeterRegistry;
 * Tests for ThingStateMetric class
 * @author Scott Hraban - Initial contribution
public class ThingStateMetricTest {
    public void testThingUidAlwaysUsedToCreateMeter() {
        final String strThingTypeUid = "sonos:Amp";
        final String strThingUid = strThingTypeUid + ":RINCON_347E5C0D150501400";
        ThingUID thingUid = new ThingUID(strThingUid);
        Thing thing = new ThingImpl(new ThingTypeUID(strThingTypeUid), thingUid);
        final String strThingUid2 = strThingTypeUid + ":foo";
        ThingUID thingUid2 = new ThingUID(strThingUid2);
        ThingRegistry thingRegistry = mock(ThingRegistry.class);
        SimpleMeterRegistry meterRegistry = new SimpleMeterRegistry();
        ThingStateMetric thingStateMetric = new ThingStateMetric(mock(BundleContext.class), thingRegistry, Set.of());
        // Only one meter registered at bind time
        doReturn(List.of(thing)).when(thingRegistry).getAll();
        thingStateMetric.bindTo(meterRegistry);
        List<Meter> meters = meterRegistry.getMeters();
        assertEquals(1, meters.size());
        assertEquals(strThingUid, meters.getFirst().getId().getTag("thing"));
        // Still only one meter registered after receiving an event
        ThingStatusInfo thingStatusInfo = new ThingStatusInfo(ThingStatus.ONLINE, ThingStatusDetail.NONE, null);
        thingStateMetric.receive(ThingEventFactory.createStatusInfoEvent(thingUid, thingStatusInfo));
        meters = meterRegistry.getMeters();
        // Now another one is added
        thingStateMetric.receive(ThingEventFactory.createStatusInfoEvent(thingUid2, thingStatusInfo));
        assertEquals(2, meters.size());
