import static org.eclipse.jdt.annotation.Checks.requireNonNull;
 * Tests {@link ConfigDescriptionRegistry}.
public class ConfigDescriptionRegistryTest extends JavaTest {
    private @NonNullByDefault({}) URI uriDummy;
    private @NonNullByDefault({}) URI uriDummy1;
    private @NonNullByDefault({}) URI uriAliases;
    private @NonNullByDefault({}) ConfigDescriptionRegistry configDescriptionRegistry;
    private @NonNullByDefault({}) ConfigDescription configDescription;
    private @NonNullByDefault({}) ConfigDescription configDescription1;
    private @NonNullByDefault({}) ConfigDescription configDescription2;
    private @NonNullByDefault({}) ConfigDescription configDescriptionAliased;
    private @Mock @NonNullByDefault({}) ConfigDescriptionProvider configDescriptionProviderMock;
    private @Mock @NonNullByDefault({}) ConfigDescriptionProvider configDescriptionProviderMock1;
    private @Mock @NonNullByDefault({}) ConfigDescriptionProvider configDescriptionProviderMock2;
    private @Mock @NonNullByDefault({}) ConfigDescriptionProvider configDescriptionProviderAliased;
    private @Mock @NonNullByDefault({}) ConfigDescriptionAliasProvider aliasProvider;
    private @Mock @NonNullByDefault({}) ConfigOptionProvider configOptionsProviderMockAliased;
    private @Mock @NonNullByDefault({}) ConfigOptionProvider configOptionsProviderMock;
    public void setUp() throws Exception {
        uriDummy = new URI("config:Dummy");
        uriDummy1 = new URI("config:Dummy1");
        uriAliases = new URI("config:Aliased");
        configDescriptionRegistry = new ConfigDescriptionRegistry();
        ConfigDescriptionParameter param1 = ConfigDescriptionParameterBuilder
                .create("param1", ConfigDescriptionParameter.Type.INTEGER).build();
        configDescription = ConfigDescriptionBuilder.create(uriDummy).withParameter(param1).build();
        when(configDescriptionProviderMock.getConfigDescriptions(any())).thenReturn(Set.of(configDescription));
        when(configDescriptionProviderMock.getConfigDescription(eq(uriDummy), any())).thenReturn(configDescription);
        configDescription1 = ConfigDescriptionBuilder.create(uriDummy1).build();
        when(configDescriptionProviderMock1.getConfigDescriptions(any())).thenReturn(Set.of(configDescription1));
        when(configDescriptionProviderMock1.getConfigDescription(eq(uriDummy1), any())).thenReturn(configDescription1);
        configDescriptionAliased = ConfigDescriptionBuilder.create(uriAliases).withParameter(
                ConfigDescriptionParameterBuilder.create("instanceId", ConfigDescriptionParameter.Type.INTEGER).build())
        when(configDescriptionProviderAliased.getConfigDescriptions(any()))
                .thenReturn(Set.of(configDescriptionAliased));
        when(configDescriptionProviderAliased.getConfigDescription(eq(uriAliases), any()))
                .thenReturn(configDescriptionAliased);
        ConfigDescriptionParameter param2 = ConfigDescriptionParameterBuilder
                .create("param2", ConfigDescriptionParameter.Type.INTEGER).build();
        configDescription2 = ConfigDescriptionBuilder.create(uriDummy).withParameter(param2).build();
        when(configDescriptionProviderMock2.getConfigDescriptions(any())).thenReturn(Set.of(configDescription2));
        when(configDescriptionProviderMock2.getConfigDescription(eq(uriDummy), any())).thenReturn(configDescription2);
        when(aliasProvider.getAlias(eq(uriAliases))).thenReturn(uriDummy);
        when(configOptionsProviderMockAliased.getParameterOptions(eq(uriAliases), anyString(), any(), any()))
                .thenReturn(List.of(new ParameterOption("Option", "Aliased")));
        when(configOptionsProviderMockAliased.getParameterOptions(eq(uriDummy), anyString(), any(), any()))
                .thenReturn(null);
        when(configOptionsProviderMock.getParameterOptions(eq(uriDummy), anyString(), any(), any()))
                .thenReturn(List.of(new ParameterOption("Option", "Original")));
        when(configOptionsProviderMock.getParameterOptions(eq(uriAliases), anyString(), any(), any())).thenReturn(null);
    public void testGetConfigDescription() throws Exception {
        configDescriptionRegistry.addConfigDescriptionProvider(configDescriptionProviderMock);
        ConfigDescription configDescription = requireNonNull(configDescriptionRegistry.getConfigDescription(uriDummy));
        assertThat(configDescription.getUID(), is(equalTo(uriDummy)));
    public void testGetConfigDescriptions() throws Exception {
        assertThat(configDescriptionRegistry.getConfigDescriptions().size(), is(0));
        assertThat(configDescriptionRegistry.getConfigDescriptions().size(), is(1));
        List<ConfigDescription> configDescriptions = new ArrayList<>(configDescriptionRegistry.getConfigDescriptions());
        assertThat(configDescriptions.getFirst().getUID(), is(equalTo(uriDummy)));
        assertThat(configDescriptions.getFirst().toParametersMap().size(), is(1));
        assertThat(configDescriptions.getFirst().toParametersMap().get("param1"), notNullValue());
        configDescriptionRegistry.addConfigDescriptionProvider(configDescriptionProviderMock1);
        assertThat(configDescriptionRegistry.getConfigDescriptions().size(), is(2));
        configDescriptionRegistry.removeConfigDescriptionProvider(configDescriptionProviderMock);
        configDescriptionRegistry.removeConfigDescriptionProvider(configDescriptionProviderMock1);
    public void testGetConfigDescriptionsOptions() throws Exception {
        configDescriptionRegistry.addConfigDescriptionProvider(configDescriptionProviderMock2);
        assertThat(configDescriptions.getFirst().getParameters().size(), is(2));
        assertThat(configDescriptions.getFirst().getParameters().getFirst().getName(), is(equalTo("param1")));
        assertThat(configDescriptions.getFirst().getParameters().get(1).getName(), is(equalTo("param2")));
        configDescriptionRegistry.removeConfigDescriptionProvider(configDescriptionProviderMock2);
    public void testGetConfigDescriptionsAliasedOptions() throws Exception {
        configDescriptionRegistry.addConfigDescriptionAliasProvider(aliasProvider);
        configDescriptionRegistry.addConfigOptionProvider(configOptionsProviderMockAliased);
        ConfigDescription res = requireNonNull(configDescriptionRegistry.getConfigDescription(uriAliases));
        assertThat(res.getParameters().getFirst().getOptions().size(), is(1));
        assertThat(res.getParameters().getFirst().getOptions().getFirst().getLabel(), is("Aliased"));
        assertThat(res.getUID(), is(uriAliases));
    public void testGetConfigDescriptionsAliasedOptionsOriginalWins() throws Exception {
        configDescriptionRegistry.addConfigOptionProvider(configOptionsProviderMock);
    public void testGetConfigDescriptionsNonAliasOptions() throws Exception {
        assertThat(res.getParameters().getFirst().getOptions().getFirst().getLabel(), is("Original"));
    public void testGetConfigDescriptionsAliasedMixes() throws Exception {
        ConfigDescription res1 = requireNonNull(configDescriptionRegistry.getConfigDescription(uriAliases));
        assertThat(res1.getParameters().size(), is(1));
        configDescriptionRegistry.addConfigDescriptionProvider(configDescriptionProviderAliased);
        ConfigDescription res2 = requireNonNull(configDescriptionRegistry.getConfigDescription(uriAliases));
        assertThat(res2.getParameters().size(), is(2));
        configDescriptionRegistry.removeConfigDescriptionProvider(configDescriptionProviderAliased);
