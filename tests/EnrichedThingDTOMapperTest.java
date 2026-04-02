import static org.hamcrest.collection.IsEmptyCollection.empty;
import org.hamcrest.CoreMatchers;
import org.openhab.core.thing.binding.builder.ChannelBuilder;
public class EnrichedThingDTOMapperTest {
    private static final String ITEM_TYPE = "itemType";
    private static final String THING_TYPE_UID = "thing:type:uid";
    private static final String UID = "thing:uid:1";
    private static final String THING_LABEL = "label";
    private static final String LOCATION = "location";
    private @Mock @NonNullByDefault({}) ThingStatusInfo thingStatusInfoMock;
    private @Mock @NonNullByDefault({}) FirmwareStatusDTO firmwareStatusMock;
    private @Mock @NonNullByDefault({}) Map<String, Set<String>> linkedItemsMapMock;
    private @Mock @NonNullByDefault({}) Configuration configurationMock;
    private @Mock @NonNullByDefault({}) Map<String, String> propertiesMock;
        when(thingMock.getThingTypeUID()).thenReturn(new ThingTypeUID(THING_TYPE_UID));
        when(thingMock.getUID()).thenReturn(new ThingUID(UID));
        when(thingMock.getLabel()).thenReturn(THING_LABEL);
        when(thingMock.getChannels()).thenReturn(mockChannels());
        when(thingMock.getConfiguration()).thenReturn(configurationMock);
        when(thingMock.getProperties()).thenReturn(propertiesMock);
        when(thingMock.getLocation()).thenReturn(LOCATION);
    public void shouldMapEnrichedThingDTO() {
        when(linkedItemsMapMock.get("1")).thenReturn(Set.of("linkedItem1", "linkedItem2"));
        EnrichedThingDTO enrichedThingDTO = EnrichedThingDTOMapper.map(thingMock, thingStatusInfoMock,
                firmwareStatusMock, linkedItemsMapMock, true);
        assertThat(enrichedThingDTO.editable, is(true));
        assertThat(enrichedThingDTO.firmwareStatus, is(equalTo(firmwareStatusMock)));
        assertThat(enrichedThingDTO.statusInfo, is(equalTo(thingStatusInfoMock)));
        assertThat(enrichedThingDTO.thingTypeUID, is(equalTo(thingMock.getThingTypeUID().getAsString())));
        assertThat(enrichedThingDTO.label, is(equalTo(THING_LABEL)));
        assertThat(enrichedThingDTO.bridgeUID, is(CoreMatchers.nullValue()));
        assertChannels(enrichedThingDTO);
        assertThat(enrichedThingDTO.configuration.values(), is(empty()));
        assertThat(enrichedThingDTO.properties, is(equalTo(propertiesMock)));
        assertThat(enrichedThingDTO.location, is(equalTo(LOCATION)));
    private void assertChannels(EnrichedThingDTO enrichedThingDTO) {
        assertThat(enrichedThingDTO.channels, hasSize(2));
        assertThat(enrichedThingDTO.channels.getFirst(), is(instanceOf(EnrichedChannelDTO.class)));
        EnrichedChannelDTO channel1 = enrichedThingDTO.channels.getFirst();
        assertThat(channel1.linkedItems, hasSize(2));
    private List<Channel> mockChannels() {
        channels.add(ChannelBuilder.create(new ChannelUID(THING_TYPE_UID + ":" + UID + ":1"), ITEM_TYPE).build());
        channels.add(ChannelBuilder.create(new ChannelUID(THING_TYPE_UID + ":" + UID + ":2"), ITEM_TYPE).build());
