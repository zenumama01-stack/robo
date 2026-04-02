 * Tests the {@link ThingStatusInfoBuilder}.
public class ThingStatusInfoBuilderTest {
    private @NonNullByDefault({}) ThingStatusInfoBuilder builder;
        builder = ThingStatusInfoBuilder.create(ThingStatus.ONLINE);
    public void testThingStatusInfoBuilderStatus() {
        ThingStatusInfo thingStatusInfo = builder.build();
        assertThat(thingStatusInfo.getStatus(), is(ThingStatus.ONLINE));
        assertThat(thingStatusInfo.getStatusDetail(), is(ThingStatusDetail.NONE));
        assertThat(thingStatusInfo.getDescription(), is(nullValue()));
    public void testThingStatusInfoBuilderStatusDetails() {
        ThingStatusInfo thingStatusInfo = builder.withStatusDetail(ThingStatusDetail.DISABLED).build();
        assertThat(thingStatusInfo.getStatusDetail(), is(ThingStatusDetail.DISABLED));
    public void testThingStatusInfoBuilderStatusDescription() {
        ThingStatusInfo thingStatusInfo = builder.withDescription("My test description").build();
        assertThat(thingStatusInfo.getDescription(), is("My test description"));
    public void subsequentBuildsCreateIndependentThingStatusInfos() {
        ThingStatusInfo thingStatusInfo1 = builder.build();
        ThingStatusInfo thingStatusInfo2 = builder.withStatusDetail(ThingStatusDetail.DISABLED)
                .withDescription("My test description").build();
        assertThat(thingStatusInfo2.getStatus(), is(thingStatusInfo1.getStatus()));
        assertThat(thingStatusInfo2.getStatusDetail(), is(not(thingStatusInfo1.getStatusDetail())));
        assertThat(thingStatusInfo2.getDescription(), is(not(thingStatusInfo1.getDescription())));
