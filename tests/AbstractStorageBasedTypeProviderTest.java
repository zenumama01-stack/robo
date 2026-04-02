import org.openhab.core.internal.types.StateDescriptionFragmentImpl;
 * The {@link AbstractStorageBasedTypeProviderTest} contains tests for the static mapping-methods
public class AbstractStorageBasedTypeProviderTest {
    public void testStateChannelTypeProperlyMappedToEntityAndBack() {
        ChannelTypeUID channelTypeUID = new ChannelTypeUID("TestBinding:testQuantityChannelType");
        ChannelType expected = ChannelTypeBuilder.state(channelTypeUID, "testLabel", "Number:Length")
                .withDescription("testDescription").withCategory("testCategory")
                .withConfigDescriptionURI(URI.create("testBinding:testConfig"))
                .withAutoUpdatePolicy(AutoUpdatePolicy.VETO).isAdvanced(true).withTag("testTag")
                .withCommandDescription(CommandDescriptionBuilder.create().build())
                .withStateDescriptionFragment(StateDescriptionFragmentBuilder.create().build()).withUnitHint("km")
        AbstractStorageBasedTypeProvider.ChannelTypeEntity entity = AbstractStorageBasedTypeProvider
                .mapToEntity(expected);
        ChannelType actual = AbstractStorageBasedTypeProvider.mapFromEntity(entity);
        assertThat(actual.getUID(), is(expected.getUID()));
        assertThat(actual.getKind(), is(expected.getKind()));
        assertThat(actual.getLabel(), is(expected.getLabel()));
        assertThat(actual.getDescription(), is(expected.getDescription()));
        assertThat(actual.getConfigDescriptionURI(), is(expected.getConfigDescriptionURI()));
        assertThat(actual.isAdvanced(), is(expected.isAdvanced()));
        assertThat(actual.getAutoUpdatePolicy(), is(expected.getAutoUpdatePolicy()));
        assertThat(actual.getCategory(), is(expected.getCategory()));
        assertThat(actual.getEvent(), is(expected.getEvent()));
        assertThat(actual.getCommandDescription(), is(expected.getCommandDescription()));
        assertThat(actual.getState(), is(expected.getState()));
        assertThat(actual.getItemType(), is(expected.getItemType()));
        assertThat(actual.getTags(), hasItems(expected.getTags().toArray(String[]::new)));
        assertThat(actual.getUnitHint(), is(expected.getUnitHint()));
    public void testChannelGroupTypeProperlyMappedToEntityAndBack() {
        ChannelGroupTypeUID groupTypeUID = new ChannelGroupTypeUID("testBinding:testGroupType");
        ChannelDefinition channelDefinition = new ChannelDefinitionBuilder("channelName",
                new ChannelTypeUID("system:color")).withLabel("label").withDescription("description")
                .withProperties(Map.of("key", "value")).withAutoUpdatePolicy(AutoUpdatePolicy.VETO).build();
        ChannelGroupType expected = ChannelGroupTypeBuilder.instance(groupTypeUID, "testLabel")
                .withChannelDefinitions(List.of(channelDefinition)).build();
        AbstractStorageBasedTypeProvider.ChannelGroupTypeEntity entity = AbstractStorageBasedTypeProvider
        ChannelGroupType actual = AbstractStorageBasedTypeProvider.mapFromEntity(entity);
        List<ChannelDefinition> expectedChannelDefinitions = expected.getChannelDefinitions();
        List<ChannelDefinition> actualChannelDefinitions = actual.getChannelDefinitions();
        assertThat(actualChannelDefinitions.size(), is(expectedChannelDefinitions.size()));
        for (ChannelDefinition expectedChannelDefinition : expectedChannelDefinitions) {
            ChannelDefinition actualChannelDefinition = Objects.requireNonNull(actualChannelDefinitions.stream()
                    .filter(d -> d.getId().equals(expectedChannelDefinition.getId())).findFirst().orElse(null));
            assertChannelDefinition(actualChannelDefinition, expectedChannelDefinition);
    public void testThingTypeProperlyMappedToEntityAndBack() {
        ThingTypeUID thingTypeUID = new ThingTypeUID("testBinding:testThingType");
        ChannelGroupDefinition channelGroupDefinition = new ChannelGroupDefinition("groupName",
                new ChannelGroupTypeUID("testBinding:channelGroupType"), "label", "description");
        ThingType expected = ThingTypeBuilder.instance(thingTypeUID, "testLabel").withDescription("description")
                .withCategory("category").withExtensibleChannelTypeIds(List.of("ch1", "ch2"))
                .withChannelDefinitions(List.of(channelDefinition))
                .withChannelGroupDefinitions(List.of(channelGroupDefinition)).isListed(true)
                .withProperties(Map.of("key", "value")).withSupportedBridgeTypeUIDs(List.of("bridge1", "bridge2"))
        AbstractStorageBasedTypeProvider.ThingTypeEntity entity = AbstractStorageBasedTypeProvider
        ThingType actual = AbstractStorageBasedTypeProvider.mapFromEntity(entity);
        assertThat(actual.getExtensibleChannelTypeIds(),
                containsInAnyOrder(expected.getExtensibleChannelTypeIds().toArray(String[]::new)));
        assertThat(actual.getSupportedBridgeTypeUIDs(),
                containsInAnyOrder(expected.getSupportedBridgeTypeUIDs().toArray(String[]::new)));
        assertThat(actual.isListed(), is(expected.isListed()));
        assertThat(actual.getRepresentationProperty(), is(expected.getRepresentationProperty()));
        assertMap(actual.getProperties(), expected.getProperties());
        List<ChannelGroupDefinition> expectedChannelGroupDefinitions = expected.getChannelGroupDefinitions();
        List<ChannelGroupDefinition> actualChannelGroupDefinitions = actual.getChannelGroupDefinitions();
        assertThat(actualChannelGroupDefinitions.size(), is(expectedChannelGroupDefinitions.size()));
        for (ChannelGroupDefinition expectedChannelGroupDefinition : expectedChannelGroupDefinitions) {
            ChannelGroupDefinition actualChannelGroupDefinition = Objects.requireNonNull(actualChannelGroupDefinitions
                    .stream().filter(d -> d.getId().equals(expectedChannelGroupDefinition.getId())).findFirst()
                    .orElse(null));
            assertChannelGroupDefinition(actualChannelGroupDefinition, expectedChannelGroupDefinition);
    public void testMapToEntityIsComplete(Class<?> originalType, Class<?> mappedType, int allowedDelta) {
        Class<?> clazz = originalType;
        int originalTypeFieldCount = 0;
        while (clazz != Object.class) {
            originalTypeFieldCount += clazz.getDeclaredFields().length;
            clazz = clazz.getSuperclass();
        int mappedEntityFieldCount = mappedType.getDeclaredFields().length;
        assertThat(originalType.getName() + " not properly mapped", mappedEntityFieldCount,
                is(originalTypeFieldCount + allowedDelta));
    private static Stream<Arguments> testMapToEntityIsComplete() {
                // isBridge is an extra field for storage and not present in ThingType
                Arguments.of(ThingType.class, AbstractStorageBasedTypeProvider.ThingTypeEntity.class, 1),
                Arguments.of(ChannelType.class, AbstractStorageBasedTypeProvider.ChannelTypeEntity.class, 0),
                Arguments.of(ChannelDefinition.class, AbstractStorageBasedTypeProvider.ChannelDefinitionEntity.class,
                        0),
                // configDescriptionURI is not available for ChannelGroupType
                Arguments.of(ChannelGroupType.class, AbstractStorageBasedTypeProvider.ChannelGroupTypeEntity.class, -1),
                Arguments.of(ChannelGroupDefinition.class,
                        AbstractStorageBasedTypeProvider.ChannelGroupDefinitionEntity.class, 0),
                Arguments.of(StateDescriptionFragmentImpl.class,
                        AbstractStorageBasedTypeProvider.StateDescriptionFragmentEntity.class, 0));
    private void assertChannelDefinition(ChannelDefinition actual, ChannelDefinition expected) {
        assertThat(actual.getId(), is(expected.getId()));
        assertThat(actual.getChannelTypeUID(), is(expected.getChannelTypeUID()));
    private void assertChannelGroupDefinition(ChannelGroupDefinition actual, ChannelGroupDefinition expected) {
        assertThat(actual.getTypeUID(), is(expected.getTypeUID()));
    private void assertMap(Map<String, String> actual, Map<String, String> expected) {
        assertThat(actual.size(), is(expected.size()));
        for (Map.Entry<String, String> entry : expected.entrySet()) {
            assertThat(actual, hasEntry(entry.getKey(), entry.getValue()));
