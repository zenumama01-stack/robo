import static org.hamcrest.collection.IsCollectionWithSize.hasSize;
 * Tests for {@link ConfigDescriptionBuilder) class.
public class ConfigDescriptionBuilderTest {
    private static final URI CONFIG_URI = URI.create("system:ephemeris");
    private static final ConfigDescriptionParameter PARAM1 = ConfigDescriptionParameterBuilder
            .create("TEST PARAM 1", Type.TEXT).build();
    private static final ConfigDescriptionParameter PARAM2 = ConfigDescriptionParameterBuilder
            .create("TEST PARAM 2", Type.INTEGER).build();
    private static final ConfigDescriptionParameterGroup GROUP1 = ConfigDescriptionParameterGroupBuilder
            .create("TEST GROUP 1").withLabel("Test Group 1").build();
    private static final ConfigDescriptionParameterGroup GROUP2 = ConfigDescriptionParameterGroupBuilder
            .create("TEST GROUP 2").withAdvanced(Boolean.TRUE).withLabel("Test Group 2").build();
    private @NonNullByDefault({}) ConfigDescriptionBuilder builder;
        builder = ConfigDescriptionBuilder.create(CONFIG_URI);
    public void testWithoutParametersAndGroups() {
        ConfigDescription configDescription = builder.build();
        assertThat(configDescription.getUID(), is(CONFIG_URI));
        assertThat(configDescription.getParameterGroups(), hasSize(0));
        assertThat(configDescription.getParameters(), hasSize(0));
    public void testWithOneParameter() {
        ConfigDescription configDescription = builder.withParameter(PARAM1).build();
        assertThat(configDescription.getParameters(), hasSize(1));
        assertThat(configDescription.getParameters().getFirst(), is(PARAM1));
    public void testWithTwoParameters() {
        final List<ConfigDescriptionParameter> params = List.of(PARAM1, PARAM2);
        ConfigDescription configDescription = builder.withParameters(params).build();
        assertThat(configDescription.getParameters(), hasSize(2));
        assertThat(configDescription.getParameters(), is(params));
    public void testWithOneParameterGroup() {
        ConfigDescription configDescription = builder.withParameterGroup(GROUP1).build();
        assertThat(configDescription.getParameterGroups(), hasSize(1));
        assertThat(configDescription.getParameterGroups().getFirst(), is(GROUP1));
    public void testWithTwoParameterGroups() {
        final List<ConfigDescriptionParameterGroup> groups = List.of(GROUP1, GROUP2);
        ConfigDescription configDescription = builder.withParameterGroups(groups).build();
        assertThat(configDescription.getParameterGroups(), hasSize(2));
        assertThat(configDescription.getParameterGroups(), is(groups));
