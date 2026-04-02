 * The {@link ManagedTransformationProviderTest} includes tests for the
 * {@link ManagedTransformationProvider}
public class ManagedTransformationProviderTest {
    private @NonNullByDefault({}) ManagedTransformationProvider provider;
        VolatileStorageService storageService = new VolatileStorageService();
        provider = new ManagedTransformationProvider(storageService);
    public void testValidConfigurationsAreAdded() {
        Transformation withoutLanguage = new Transformation("config:foo:identifier", "", "foo",
                Map.of(FUNCTION, "content"));
        provider.add(withoutLanguage);
        Transformation withLanguage = new Transformation("config:foo:identifier:de", "", "foo",
        provider.add(withLanguage);
        Mockito.verify(listenerMock).added(provider, withoutLanguage);
        Mockito.verify(listenerMock).added(provider, withLanguage);
    public void testValidConfigurationsIsUpdated() {
        Transformation configuration = new Transformation("config:foo:identifier", "", "foo",
        Transformation updatedConfiguration = new Transformation("config:foo:identifier", "", "foo",
                Map.of(FUNCTION, "updated"));
        provider.add(configuration);
        provider.update(updatedConfiguration);
        Mockito.verify(listenerMock).added(provider, configuration);
        Mockito.verify(listenerMock).updated(provider, configuration, updatedConfiguration);
    public void testUidFormatValidation() {
        Transformation inValidUid = new Transformation("invalid:foo:identifier", "", "foo",
        assertThrows(IllegalArgumentException.class, () -> provider.add(inValidUid));
    public void testTypeValidation() {
        Transformation typeNotMatching = new Transformation("config:foo:identifier", "", "bar",
        assertThrows(IllegalArgumentException.class, () -> provider.add(typeNotMatching));
    public void testSerializationDeserializationResultsInSameConfiguration() {
        Transformation configuration1 = provider.get("config:foo:identifier");
        assertThat(configuration, is(configuration1));
