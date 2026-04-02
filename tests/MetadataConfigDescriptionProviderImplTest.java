import static org.openhab.core.config.core.internal.metadata.MetadataConfigDescriptionProviderImpl.*;
public class MetadataConfigDescriptionProviderImplTest extends JavaTest {
    private static final String LIBERAL = "liberal";
    private static final String RESTRICTED = "restricted";
    private static final URI URI_RESTRICTED = URI.create(SCHEME + SEPARATOR + RESTRICTED);
    private static final URI URI_LIBERAL = URI.create(SCHEME + SEPARATOR + LIBERAL);
    private static final URI URI_RESTRICTED_DIMMER = URI.create(SCHEME + SEPARATOR + RESTRICTED + SEPARATOR + "dimmer");
    private @Mock @NonNullByDefault({}) MetadataConfigDescriptionProvider providerRestrictedMock;
    private @Mock @NonNullByDefault({}) MetadataConfigDescriptionProvider providerLiberalMock;
    private MetadataConfigDescriptionProviderImpl service = new MetadataConfigDescriptionProviderImpl();
        service = new MetadataConfigDescriptionProviderImpl();
        when(providerRestrictedMock.getNamespace()).thenReturn(RESTRICTED);
        when(providerRestrictedMock.getDescription(any())).thenReturn("Restricted");
        when(providerRestrictedMock.getParameterOptions(any())).thenReturn(List.of( //
                new ParameterOption("dimmer", "Dimmer"), //
                new ParameterOption("switch", "Switch") //
        when(providerRestrictedMock.getParameters(eq("dimmer"), any())).thenReturn(List.of( //
                ConfigDescriptionParameterBuilder.create("width", Type.INTEGER).build(), //
                ConfigDescriptionParameterBuilder.create("height", Type.INTEGER).build() //
        when(providerLiberalMock.getNamespace()).thenReturn(LIBERAL);
        when(providerLiberalMock.getDescription(any())).thenReturn("Liberal");
        when(providerLiberalMock.getParameterOptions(any())).thenReturn(null);
    public void testGetConfigDescriptionsNoOptions() {
        service.addMetadataConfigDescriptionProvider(providerLiberalMock);
        Collection<ConfigDescription> res = service.getConfigDescriptions(Locale.ENGLISH);
        assertNotNull(res);
        assertEquals(1, res.size());
        ConfigDescription desc = res.iterator().next();
        assertEquals(URI_LIBERAL, desc.getUID());
        assertEquals(1, desc.getParameters().size());
        ConfigDescriptionParameter param = desc.getParameters().getFirst();
        assertEquals("value", param.getName());
        assertEquals("Liberal", param.getDescription());
        assertFalse(param.getLimitToOptions());
    public void testGetConfigDescriptionsWithOptions() {
        service.addMetadataConfigDescriptionProvider(providerRestrictedMock);
        assertEquals(URI_RESTRICTED, desc.getUID());
        assertEquals("Restricted", param.getDescription());
        assertEquals("dimmer", param.getOptions().getFirst().getValue());
        assertEquals("switch", param.getOptions().get(1).getValue());
    public void testGetConfigDescriptionWrongScheme() {
        assertNull(service.getConfigDescription(URI.create("some:nonsense"), null));
    public void testGetConfigDescriptionValueDescription() {
        ConfigDescription desc = service.getConfigDescription(URI_LIBERAL, null);
        assertNotNull(desc);
    public void testGetConfigDescriptionValueDescriptionNonExistingNamespace() {
        ConfigDescription desc = service.getConfigDescription(URI.create("metadata:nonsense"), null);
        assertNull(desc);
    public void testGetConfigDescriptionPropertiesDescription() {
        ConfigDescription desc = service.getConfigDescription(URI_RESTRICTED_DIMMER, null);
        assertEquals(URI_RESTRICTED_DIMMER, desc.getUID());
        assertEquals(2, desc.getParameters().size());
        ConfigDescriptionParameter paramWidth = desc.getParameters().getFirst();
        assertEquals("width", paramWidth.getName());
        ConfigDescriptionParameter paramHeight = desc.getParameters().get(1);
        assertEquals("height", paramHeight.getName());
    public void testGetConfigDescriptionPropertiesDescriptionNonExistingNamespace() {
        ConfigDescription desc = service.getConfigDescription(URI.create("metadata:nonsense:nonsense"), null);
    public void testGetConfigDescriptionPropertiesDescriptionNonExistingValue() {
        ConfigDescription desc = service.getConfigDescription(URI.create("metadata:foo:nonsense"), null);
