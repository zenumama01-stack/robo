import static org.hamcrest.Matchers.instanceOf;
 * The {@link ItemChannelLinkResourceTest} tests the {@link ItemChannelLinkResource}
public class ItemChannelLinkResourceTest {
    private static final int EXPECTED_REMOVED_LINKS = 5;
    private @Mock @NonNullByDefault({}) ItemChannelLinkRegistry itemChannelLinkRegistryMock;
    private @Mock @NonNullByDefault({}) ManagedItemChannelLinkProvider managedItemChannelLinkProviderMock;
    private @NonNullByDefault({}) ItemChannelLinkResource itemChannelLinkResource;
        itemChannelLinkResource = new ItemChannelLinkResource(itemRegistryMock, thingRegistryMock,
                channelTypeRegistryMock, profileTypeRegistryMock, itemChannelLinkRegistryMock,
                managedItemChannelLinkProviderMock);
        when(itemChannelLinkRegistryMock.removeLinksForItem(any())).thenReturn(EXPECTED_REMOVED_LINKS);
        when(itemChannelLinkRegistryMock.removeLinksForThing(any())).thenReturn(EXPECTED_REMOVED_LINKS);
    public void testRemoveAllLinksForItem() {
        try (Response response = itemChannelLinkResource.removeAllLinksForObject("testItem")) {
            Object responseEntity = response.getEntity();
            assertThat(responseEntity, instanceOf(Map.class));
            assertThat(((Map<?, ?>) responseEntity).get("count"), is(EXPECTED_REMOVED_LINKS));
        verify(itemChannelLinkRegistryMock).removeLinksForItem(eq("testItem"));
    public void testRemoveAllLinksForThing() {
        try (Response response = itemChannelLinkResource.removeAllLinksForObject("binding:type:thing")) {
        verify(itemChannelLinkRegistryMock).removeLinksForThing(eq(new ThingUID("binding:type:thing")));
