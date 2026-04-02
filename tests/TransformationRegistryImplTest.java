 * The {@link TransformationRegistryImplTest} includes tests for the
 * {@link TransformationRegistryImpl}
public class TransformationRegistryImplTest {
    private static final String SERVICE = "foo";
    private static final String MANAGED_WITHOUT_LANGUAGE_UID = "config:" + SERVICE + ":managed";
    private static final String MANAGED_WITH_EN_LANGUAGE_UID = "config:" + SERVICE + ":managed:en";
    private static final String MANAGED_WITH_DE_LANGUAGE_UID = "config:" + SERVICE + ":managed:de";
    private static final Transformation MANAGED_WITHOUT_LANGUAGE = new Transformation(MANAGED_WITHOUT_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, MANAGED_WITHOUT_LANGUAGE_UID));
    private static final Transformation MANAGED_WITH_EN_LANGUAGE = new Transformation(MANAGED_WITH_EN_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, MANAGED_WITH_EN_LANGUAGE_UID));
    private static final Transformation MANAGED_WITH_DE_LANGUAGE = new Transformation(MANAGED_WITH_DE_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, MANAGED_WITH_DE_LANGUAGE_UID));
    private static final String FILE_WITHOUT_LANGUAGE_UID = "foo/FILE." + SERVICE;
    private static final String FILE_WITH_EN_LANGUAGE_UID = "foo/FILE_en." + SERVICE;
    private static final String FILE_WITH_DE_LANGUAGE_UID = "foo/FILE_de." + SERVICE;
    private static final Transformation FILE_WITHOUT_LANGUAGE = new Transformation(FILE_WITHOUT_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, FILE_WITHOUT_LANGUAGE_UID));
    private static final Transformation FILE_WITH_EN_LANGUAGE = new Transformation(FILE_WITH_EN_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, FILE_WITH_EN_LANGUAGE_UID));
    private static final Transformation FILE_WITH_DE_LANGUAGE = new Transformation(FILE_WITH_DE_LANGUAGE_UID, "",
            SERVICE, Map.of(FUNCTION, FILE_WITH_DE_LANGUAGE_UID));
    private @Mock @NonNullByDefault({}) LocaleProvider localeProviderMock;
    private @Mock @NonNullByDefault({}) ManagedTransformationProvider providerMock;
    private @NonNullByDefault({}) TransformationRegistryImpl registry;
        Mockito.when(localeProviderMock.getLocale()).thenReturn(Locale.US);
        registry = new TransformationRegistryImpl(localeProviderMock);
        registry.addProvider(providerMock);
        registry.added(providerMock, MANAGED_WITHOUT_LANGUAGE);
        registry.added(providerMock, MANAGED_WITH_EN_LANGUAGE);
        registry.added(providerMock, MANAGED_WITH_DE_LANGUAGE);
        registry.added(providerMock, FILE_WITHOUT_LANGUAGE);
        registry.added(providerMock, FILE_WITH_EN_LANGUAGE);
        registry.added(providerMock, FILE_WITH_DE_LANGUAGE);
    public void testManagedReturnsCorrectLanguage() {
        // language contained in uid, default requested (explicit uid takes precedence)
        assertThat(registry.get(MANAGED_WITH_DE_LANGUAGE_UID, null), is(MANAGED_WITH_DE_LANGUAGE));
        // language contained in uid, other requested (explicit uid takes precedence)
        assertThat(registry.get(MANAGED_WITH_DE_LANGUAGE_UID, Locale.FRANCE), is(MANAGED_WITH_DE_LANGUAGE));
        // no language in uid, default requested
        assertThat(registry.get(MANAGED_WITHOUT_LANGUAGE_UID, null), is(MANAGED_WITH_EN_LANGUAGE));
        // no language in uid, other requested
        assertThat(registry.get(MANAGED_WITHOUT_LANGUAGE_UID, Locale.GERMANY), is(MANAGED_WITH_DE_LANGUAGE));
        // no language in uid, unknown requested
        assertThat(registry.get(MANAGED_WITHOUT_LANGUAGE_UID, Locale.FRANCE), is(MANAGED_WITHOUT_LANGUAGE));
    public void testFileReturnsCorrectLanguage() {
        assertThat(registry.get(FILE_WITH_DE_LANGUAGE_UID, null), is(FILE_WITH_DE_LANGUAGE));
        assertThat(registry.get(FILE_WITH_DE_LANGUAGE_UID, Locale.FRANCE), is(FILE_WITH_DE_LANGUAGE));
        assertThat(registry.get(FILE_WITHOUT_LANGUAGE_UID, null), is(FILE_WITH_EN_LANGUAGE));
        assertThat(registry.get(FILE_WITHOUT_LANGUAGE_UID, Locale.GERMANY), is(FILE_WITH_DE_LANGUAGE));
        assertThat(registry.get(FILE_WITHOUT_LANGUAGE_UID, Locale.FRANCE), is(FILE_WITHOUT_LANGUAGE));
