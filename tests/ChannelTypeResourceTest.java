import org.hamcrest.core.IsIterableContaining;
import org.openhab.core.io.rest.LocaleServiceImpl;
import org.openhab.core.library.CoreItemFactory;
import org.openhab.core.thing.type.ChannelTypeBuilder;
public class ChannelTypeResourceTest {
    private @NonNullByDefault({}) ChannelTypeResource channelTypeResource;
    private @Mock @NonNullByDefault({}) ChannelTypeRegistry channelTypeRegistryMock;
    private @Mock @NonNullByDefault({}) LocaleServiceImpl localeServiceMock;
    private @Mock @NonNullByDefault({}) ProfileTypeRegistry profileTypeRegistryMock;
        channelTypeResource = new ChannelTypeResource(channelTypeRegistryMock, configDescriptionRegistryMock,
                localeServiceMock, profileTypeRegistryMock);
    public void getAllShouldRetrieveAllChannelTypes() throws Exception {
        when(localeServiceMock.getLocale(null)).thenReturn(Locale.ENGLISH);
        channelTypeResource.getAll(null, null);
        verify(channelTypeRegistryMock).getChannelTypes(Locale.ENGLISH);
    public void returnLinkableItemTypesForTriggerChannelType() throws IOException {
        ChannelTypeUID channelTypeUID = new ChannelTypeUID("binding", "ct");
        ChannelType channelType = ChannelTypeBuilder.trigger(channelTypeUID, "Label").build();
        when(channelTypeRegistryMock.getChannelType(channelTypeUID)).thenReturn(channelType);
        TriggerProfileType profileType = mock(TriggerProfileType.class);
        when(profileType.getSupportedChannelTypeUIDs()).thenReturn(List.of(channelTypeUID));
        when(profileType.getSupportedItemTypes()).thenReturn(List.of(CoreItemFactory.SWITCH, CoreItemFactory.DIMMER));
        when(profileTypeRegistryMock.getProfileTypes()).thenReturn(List.of(profileType));
        Response response = channelTypeResource.getLinkableItemTypes(channelTypeUID.getAsString());
        verify(channelTypeRegistryMock).getChannelType(channelTypeUID);
        verify(profileTypeRegistryMock).getProfileTypes();
        assertThat(response.getStatus(), is(200));
        assertThat((Set<String>) response.getEntity(),
                IsIterableContaining.hasItems(CoreItemFactory.SWITCH, CoreItemFactory.DIMMER));
