 * Tests for {@link ConfigDescriptionParameterGroupBuilder) class.
public class ConfigDescriptionParameterGroupBuilderTest {
    private @NonNullByDefault({}) ConfigDescriptionParameterGroupBuilder builder;
        builder = ConfigDescriptionParameterGroupBuilder.create("test") //
                .withContext("My Context") //
                .withAdvanced(Boolean.TRUE) //
                .withLabel("My Label") //
                .withDescription("My Description");
    public void testConfigDescriptionParameterGroupBuilder() {
        ConfigDescriptionParameterGroup group = builder.build();
        assertThat(group.getName(), is("test"));
        assertThat(group.getContext(), is("My Context"));
        assertThat(group.isAdvanced(), is(true));
        assertThat(group.getLabel(), is("My Label"));
        assertThat(group.getDescription(), is("My Description"));
    public void subsequentBuildsCreateIndependentConfigDescriptionParameterGroups() {
        ConfigDescriptionParameterGroup otherGroup = builder.withContext("My Second Context") //
                .withAdvanced(Boolean.FALSE) //
                .withLabel("My Second Label") //
                .withDescription("My Second Description") //
        assertThat(otherGroup.getName(), is(group.getName()));
        assertThat(otherGroup.getContext(), is(not(group.getContext())));
        assertThat(otherGroup.isAdvanced(), is(not(group.isAdvanced())));
        assertThat(otherGroup.getLabel(), is(not(group.getLabel())));
        assertThat(otherGroup.getDescription(), is(not(group.getDescription())));
