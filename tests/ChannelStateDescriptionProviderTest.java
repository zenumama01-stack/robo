 * @author Maksym Krasovskyi - Initial contribution
public class ChannelStateDescriptionProviderTest {
    private @NonNullByDefault({}) ChannelStateDescriptionProvider channelStateDescriptionProvider;
    private @Mock @NonNullByDefault({}) DynamicStateDescriptionProvider dynamicStateDescriptionProvider;
    private @Mock @NonNullByDefault({}) ItemChannelLinkRegistry itemChannelLinkRegistry;
    private @Mock @NonNullByDefault({}) ThingRegistry thingRegistry;
    private static final ChannelUID CHANNEL_UID_1 = new ChannelUID("channel:f:g:1");
    private static final ChannelUID CHANNEL_UID_2 = new ChannelUID("channel:f:g:2");
    private static final String ITEM_1 = "item1";
        channelStateDescriptionProvider = new ChannelStateDescriptionProvider(itemChannelLinkRegistry,
                thingTypeRegistry, thingRegistry);
    @CsvSource({ "true, true, true", "true, false, false", "false, true, false", "false, false, false" })
    public void testStateDescriptionFromMultipleChannels(Boolean channel1State, Boolean channel2State,
            Boolean expectedItemState) {
        when(itemChannelLinkRegistry.getBoundChannels(ITEM_1))
                .thenReturn(new HashSet<>(Arrays.asList(CHANNEL_UID_1, CHANNEL_UID_2)));
        // Setup channel 1
        Channel channel1 = ChannelBuilder.create(CHANNEL_UID_1).build();
        when(thingRegistry.getChannel(CHANNEL_UID_1)).thenReturn(channel1);
        StateDescription stateDescription1 = StateDescriptionFragmentBuilder.create().withMinimum(BigDecimal.ZERO)
                .withMaximum(new BigDecimal(100)).withStep(BigDecimal.ONE).withReadOnly(channel1State).withPattern("%s")
                .build().toStateDescription();
        ChannelType channelType1 = Mockito.mock(ChannelType.class);
        assertNotNull(stateDescription1);
        when(channelType1.getState()).thenReturn(stateDescription1);
        when(thingTypeRegistry.getChannelType(channel1, Locale.ENGLISH)).thenReturn(channelType1);
        when(dynamicStateDescriptionProvider.getStateDescription(channel1, stateDescription1, Locale.ENGLISH))
                .thenReturn(StateDescriptionFragmentBuilder.create(stateDescription1).build().toStateDescription());
        // Setup channel 2
        Channel channel2 = ChannelBuilder.create(CHANNEL_UID_2).build();
        when(thingRegistry.getChannel(CHANNEL_UID_2)).thenReturn(channel2);
        StateDescription stateDescription2 = StateDescriptionFragmentBuilder.create().withMinimum(BigDecimal.ZERO)
                .withMaximum(new BigDecimal(100)).withStep(BigDecimal.ONE).withReadOnly(channel2State).withPattern("%s")
        ChannelType channelType2 = Mockito.mock(ChannelType.class);
        assertNotNull(stateDescription2);
        when(channelType2.getState()).thenReturn(stateDescription2);
        when(thingTypeRegistry.getChannelType(channel2, Locale.ENGLISH)).thenReturn(channelType2);
        when(dynamicStateDescriptionProvider.getStateDescription(channel2, stateDescription2, Locale.ENGLISH))
                .thenReturn(StateDescriptionFragmentBuilder.create(stateDescription2).build().toStateDescription());
        channelStateDescriptionProvider.addDynamicStateDescriptionProvider(dynamicStateDescriptionProvider);
        StateDescriptionFragment stateDescriptionResult = channelStateDescriptionProvider
                .getStateDescriptionFragment(ITEM_1, Locale.ENGLISH);
        assertNotNull(stateDescriptionResult);
        assertEquals(expectedItemState, stateDescriptionResult.isReadOnly());
    public void testStateDescriptionWithSingleReadOnlyChannel() {
        when(itemChannelLinkRegistry.getBoundChannels(ITEM_1)).thenReturn(new HashSet<>(Arrays.asList(CHANNEL_UID_1)));
                .withMaximum(new BigDecimal(100)).withStep(BigDecimal.ONE).withReadOnly(Boolean.TRUE).withPattern("%s")
        ChannelType channelType = Mockito.mock(ChannelType.class);
        when(channelType.getState()).thenReturn(stateDescription1);
        when(thingTypeRegistry.getChannelType(channel1, Locale.ENGLISH)).thenReturn(channelType);
        assertTrue(stateDescriptionResult.isReadOnly());
    public void testStateDescriptionWithSingleWriteOnlyChannel() {
                .withMaximum(new BigDecimal(100)).withStep(BigDecimal.ONE).withReadOnly(Boolean.FALSE).withPattern("%s")
        assertFalse(stateDescriptionResult.isReadOnly());
