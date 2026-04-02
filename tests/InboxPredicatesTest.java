 * Tests for {@link InboxPredicates}.
public class InboxPredicatesTest {
    private static final String BINDING_ID1 = "bindingId1";
    private static final String BINDING_ID2 = "bindingId2";
    private static final String THING_ID1 = "thingId1";
    private static final String THING_ID2 = "thingId2";
    private static final String THING_TYPE_ID1 = "thingTypeId1";
    private static final String THING_TYPE_ID2 = "thingTypeId2";
    private static final ThingUID THING_UID11 = new ThingUID(BINDING_ID1, THING_ID1);
    private static final ThingUID THING_UID12 = new ThingUID(BINDING_ID1, THING_ID2);
    private static final ThingUID THING_UID22 = new ThingUID(BINDING_ID2, THING_ID2);
    private static final String PROP1 = "prop1";
    private static final String PROP2 = "prop2";
    private static final String PROP_VAL1 = "propVal1";
    private static final String PROP_VAL2 = "propVal2";
    private static final ThingTypeUID THING_TYPE_UID11 = new ThingTypeUID(BINDING_ID1, THING_TYPE_ID1);
    private static final ThingTypeUID THING_TYPE_UID12 = new ThingTypeUID(BINDING_ID1, THING_TYPE_ID2);
    private static final ThingTypeUID THING_TYPE_UID21 = new ThingTypeUID(BINDING_ID2, THING_TYPE_ID1);
    private static final Map<String, Object> PROPS1 = Map.ofEntries(entry(PROP1, PROP_VAL1), entry(PROP2, PROP_VAL2));
    private static final Map<String, Object> PROPS2 = Map.of(PROP2, PROP_VAL2);
    private static final List<DiscoveryResult> RESULTS = List.of(
            DiscoveryResultBuilder.create(THING_UID11).withThingType(THING_TYPE_UID11).withProperties(PROPS1)
                    .withRepresentationProperty(PROP1).withLabel("label").build(),
            DiscoveryResultBuilder.create(THING_UID12).withThingType(THING_TYPE_UID11).withProperties(PROPS1)
                    .withLabel("label").build(),
            DiscoveryResultBuilder.create(THING_UID12).withThingType(THING_TYPE_UID12).withProperties(PROPS2)
                    .withRepresentationProperty(PROP2).withLabel("label").build(),
            DiscoveryResultBuilder.create(THING_UID22).withThingType(THING_TYPE_UID21).withProperties(PROPS2)
                    .withLabel("label").build());
        ((DiscoveryResultImpl) RESULTS.get(3)).setFlag(DiscoveryResultFlag.IGNORED);
    public void testForBinding() {
        assertThat(RESULTS.stream().filter(forBinding(BINDING_ID1)).toList().size(), is(3));
        assertThat(RESULTS.stream().filter(forBinding(BINDING_ID2)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(forBinding(BINDING_ID2)).toList().getFirst(), is(equalTo(RESULTS.get(3))));
        assertThat(RESULTS.stream().filter(forBinding(BINDING_ID2)).filter(withFlag(DiscoveryResultFlag.NEW)).toList()
                .size(), is(0));
        assertThat(RESULTS.stream().filter(forBinding(BINDING_ID2)).filter(withFlag(DiscoveryResultFlag.IGNORED))
                .toList().size(), is(1));
                .toList().getFirst(), is(equalTo(RESULTS.get(3))));
    public void testForThingTypeUID() {
        assertThat(RESULTS.stream().filter(forThingTypeUID(THING_TYPE_UID11)).toList().size(), is(2));
        assertThat(RESULTS.stream().filter(forThingTypeUID(THING_TYPE_UID12)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(forThingTypeUID(THING_TYPE_UID12)).toList().getFirst(),
                is(equalTo(RESULTS.get(2))));
    public void testForThingUID() {
        assertThat(RESULTS.stream().filter(forThingUID(THING_UID11)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(forThingUID(THING_UID11)).toList().getFirst(),
                is(equalTo(RESULTS.getFirst())));
        assertThat(RESULTS.stream().filter(forThingUID(THING_UID12)).toList().size(), is(2));
        assertThat(RESULTS.stream().filter(forThingUID(THING_UID12)).filter(forThingTypeUID(THING_TYPE_UID12)).toList()
                .size(), is(1));
                .getFirst(), is(equalTo(RESULTS.get(2))));
    public void testWithFlag() {
        assertThat(RESULTS.stream().filter(withFlag(DiscoveryResultFlag.NEW)).toList().size(), is(3));
        assertThat(RESULTS.stream().filter(withFlag(DiscoveryResultFlag.IGNORED)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(withFlag(DiscoveryResultFlag.IGNORED)).toList().getFirst(),
                is(equalTo(RESULTS.get(3))));
    public void testWithProperty() {
        assertThat(RESULTS.stream().filter(withProperty(PROP1, PROP_VAL1)).toList().size(), is(2));
        assertThat(RESULTS.stream().filter(withProperty(PROP2, PROP_VAL2)).toList().size(), is(4));
        assertThat(RESULTS.stream().filter(withProperty(PROP1, PROP_VAL2)).toList().size(), is(0));
        assertThat(RESULTS.stream().filter(withProperty(PROP2, PROP_VAL1)).toList().size(), is(0));
        assertThat(RESULTS.stream().filter(withProperty(null, PROP_VAL1)).toList().size(), is(0));
    public void testWithRepresentationProperty() {
        assertThat(RESULTS.stream().filter(withRepresentationProperty(PROP1)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(withRepresentationProperty(PROP1)).toList().getFirst(),
        assertThat(RESULTS.stream().filter(withRepresentationProperty(PROP2)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(withRepresentationProperty(PROP2)).toList().getFirst(),
    public void testWithRepresentationPropertyValue() {
        assertThat(RESULTS.stream().filter(withRepresentationPropertyValue(PROP_VAL1)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(withRepresentationPropertyValue(PROP_VAL1)).toList().getFirst(),
        assertThat(RESULTS.stream().filter(withRepresentationPropertyValue(PROP_VAL2)).toList().size(), is(1));
        assertThat(RESULTS.stream().filter(withRepresentationPropertyValue(PROP_VAL2)).toList().getFirst(),
