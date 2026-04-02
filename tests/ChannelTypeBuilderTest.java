 * Tests the {@link ChannelTypeBuilder}.
public class ChannelTypeBuilderTest {
    private static final String TAG = "tag";
    private static final List<String> TAGS = List.of("TAG1", "TAG2");
    private static final URI CONFIG_DESCRIPTION_URL = URI.create("config:dummy");
    private static final ChannelTypeUID CHANNEL_TYPE_UID = new ChannelTypeUID("bindingId", "channelId");
    private static final StateDescriptionFragment STATE_DESCRIPTION_FRAGMENT = StateDescriptionFragmentBuilder.create()
            .withMinimum(BigDecimal.ZERO).withMaximum(new BigDecimal(100)).withStep(BigDecimal.ONE).withPattern("%s")
    private static final StateDescription STATE_DESCRIPTION = Objects
            .requireNonNull(STATE_DESCRIPTION_FRAGMENT.toStateDescription());
    private static final EventDescription EVENT_DESCRIPTION = new EventDescription(
            List.of(new EventOption(CommonTriggerEvents.DIR1_PRESSED, null),
                    new EventOption(CommonTriggerEvents.DIR1_RELEASED, null)));
    private @NonNullByDefault({}) StateChannelTypeBuilder stateBuilder;
    private @NonNullByDefault({}) TriggerChannelTypeBuilder triggerBuilder;
        // set up a valid basic ChannelTypeBuilder
        stateBuilder = ChannelTypeBuilder.state(CHANNEL_TYPE_UID, LABEL, ITEM_TYPE);
        triggerBuilder = ChannelTypeBuilder.trigger(CHANNEL_TYPE_UID, LABEL);
        assertThrows(IllegalArgumentException.class, () -> ChannelTypeBuilder.state(CHANNEL_TYPE_UID, "", ITEM_TYPE));
    public void whenItemTypeIsBlankForStateShouldFail() {
        assertThrows(IllegalArgumentException.class, () -> ChannelTypeBuilder.state(CHANNEL_TYPE_UID, LABEL, ""));
    public void whenLabelIsBlankForTriggerShouldFail() {
        assertThrows(IllegalArgumentException.class, () -> ChannelTypeBuilder.trigger(CHANNEL_TYPE_UID, ""));
    public void withLabelAndChannelTypeUIDShouldCreateChannelType() {
        ChannelType channelType = stateBuilder.build();
        assertThat(channelType.getUID(), is(CHANNEL_TYPE_UID));
        assertThat(channelType.getItemType(), is(ITEM_TYPE));
        assertThat(channelType.getLabel(), is(LABEL));
        assertThat(channelType.getKind(), is(ChannelKind.STATE));
    public void withDefaultAdvancedIsFalse() {
        assertThat(channelType.isAdvanced(), is(false));
    public void isAdvancedShouldSetAdvanced() {
        ChannelType channelType = stateBuilder.isAdvanced(true).build();
        assertThat(channelType.isAdvanced(), is(true));
        ChannelType channelType = stateBuilder.withDescription(DESCRIPTION).build();
        assertThat(channelType.getDescription(), is(DESCRIPTION));
        ChannelType channelType = stateBuilder.withCategory(CATEGORY).build();
        assertThat(channelType.getCategory(), is(CATEGORY));
    public void withConfigDescriptionURIShouldSetConfigDescriptionURI() {
        ChannelType channelType = stateBuilder.withConfigDescriptionURI(CONFIG_DESCRIPTION_URL).build();
        assertThat(channelType.getConfigDescriptionURI(), is(CONFIG_DESCRIPTION_URL));
    public void withTagsShouldSetTag() {
        ChannelType channelType = stateBuilder.withTag(TAG).build();
        assertThat(channelType.getTags(), is(hasSize(1)));
    public void withTagsShouldSetTags() {
        ChannelType channelType = stateBuilder.withTags(TAGS).build();
        assertThat(channelType.getTags(), is(hasSize(2)));
    public void withStateDescriptionFragmentShouldSetStateDescription() {
        ChannelType channelType = stateBuilder.withStateDescriptionFragment(STATE_DESCRIPTION_FRAGMENT).build();
        assertThat(channelType.getState(), is(STATE_DESCRIPTION));
    public void withEventDescriptionShouldSetEventDescription() {
        ChannelType channelType = triggerBuilder.withEventDescription(EVENT_DESCRIPTION).build();
        assertThat(channelType.getEvent(), is(EVENT_DESCRIPTION));
        assertThat(channelType.getKind(), is(ChannelKind.TRIGGER));
