import static org.openhab.core.thing.binding.builder.ThingStatusInfoBuilder.create;
import static org.openhab.core.thing.util.ThingHandlerHelper.isHandlerInitialized;
 * Test for the ThingHandlerHelper
public class ThingHandlerHelperTest {
    private @Mock @NonNullByDefault({}) ThingHandler thingHandlerMock;
        thing = ThingBuilder.create(new ThingTypeUID("test:test"), new ThingUID("test:test:test")).build();
    public void assertIsHandlerInitializedWorksCorrectlyForAThingStatus() {
        assertThat(isHandlerInitialized(ThingStatus.UNINITIALIZED), is(false));
        assertThat(isHandlerInitialized(ThingStatus.INITIALIZING), is(false));
        assertThat(isHandlerInitialized(ThingStatus.REMOVING), is(false));
        assertThat(isHandlerInitialized(ThingStatus.REMOVED), is(false));
        assertThat(isHandlerInitialized(ThingStatus.UNKNOWN), is(true));
        assertThat(isHandlerInitialized(ThingStatus.ONLINE), is(true));
        assertThat(isHandlerInitialized(ThingStatus.OFFLINE), is(true));
    public void assertIsHandlerInitializedWorksCorrectlyForAThing() {
        thing.setStatusInfo(create(ThingStatus.UNINITIALIZED).build());
        assertThat(isHandlerInitialized(thing), is(false));
        thing.setStatusInfo(create(ThingStatus.INITIALIZING).build());
        thing.setStatusInfo(create(ThingStatus.REMOVING).build());
        thing.setStatusInfo(create(ThingStatus.REMOVED).build());
        thing.setStatusInfo(create(ThingStatus.UNKNOWN).build());
        assertThat(isHandlerInitialized(thing), is(true));
        thing.setStatusInfo(create(ThingStatus.ONLINE).build());
        thing.setStatusInfo(create(ThingStatus.OFFLINE).build());
    public void assertIsHandlerInitializedWorksCorrectlyForAThingHandler() {
        when(thingHandlerMock.getThing()).thenReturn(thing);
        assertThat(isHandlerInitialized(thingHandlerMock), is(false));
        assertThat(isHandlerInitialized(thingHandlerMock), is(true));
