 * Tests the {@link ChannelGroupTypeBuilder}.
public class ChannelGroupTypeBuilderTest {
    private static final String DESCRIPTION = "description";
    private static final String CATEGORY = "category";
    private static final String LABEL = "label";
    private static final ChannelGroupTypeUID CHANNEL_GROUP_TYPE_UID = new ChannelGroupTypeUID("bindingId",
            "channelGroupId");
    private @NonNullByDefault({}) ChannelGroupTypeBuilder builder;
        // set up a valid basic ChannelGroupTypeBuilder
        builder = ChannelGroupTypeBuilder.instance(CHANNEL_GROUP_TYPE_UID, LABEL);
    public void whenLabelIsBlankForStateShouldFail() {
                () -> ChannelGroupTypeBuilder.instance(CHANNEL_GROUP_TYPE_UID, ""));
    public void withLabelAndChannelGroupTypeUIDShouldCreateChannelGroupType() {
        ChannelGroupType channelGroupType = builder.build();
        assertEquals(CHANNEL_GROUP_TYPE_UID, channelGroupType.getUID());
        assertEquals(LABEL, channelGroupType.getLabel());
    public void withDescriptionShouldSetDescription() {
        ChannelGroupType channelGroupType = builder.withDescription(DESCRIPTION).build();
        assertEquals(DESCRIPTION, channelGroupType.getDescription());
    public void withCategoryShouldSetCategory() {
        ChannelGroupType channelGroupType = builder.withCategory(CATEGORY).build();
        assertEquals(CATEGORY, channelGroupType.getCategory());
    public void withChannelDefinitionsShouldSetUnmodifiableChannelDefinitions() {
        ChannelGroupType channelGroupType = builder.withChannelDefinitions(mockList(ChannelDefinition.class, 2))
        assertEquals(2, channelGroupType.getChannelDefinitions().size());
            channelGroupType.getChannelDefinitions().add(mock(ChannelDefinition.class));
            fail();
            // expected
    private <T> List<T> mockList(Class<T> entityClass, int size) {
        List<T> result = new ArrayList<>(size);
        for (int i = 0; i < size; i++) {
            result.add(mock(entityClass));
