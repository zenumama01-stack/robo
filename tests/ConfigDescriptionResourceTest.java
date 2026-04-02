public class ConfigDescriptionResourceTest {
    private static final String PARAM_COUNTRY = "country";
    private @NonNullByDefault({}) ConfigDescriptionResource resource;
    private @Mock @NonNullByDefault({}) LocaleService localeServiceMock;
        final ConfigDescription systemI18n = ConfigDescriptionBuilder.create(systemI18nURI)
                .withParameter(ConfigDescriptionParameterBuilder.create(PARAM_NAME, Type.TEXT)
                        .withDefault(PARAMETER_NAME_DEFAULT_VALUE).build())
        final ConfigDescription systemEphemeris = ConfigDescriptionBuilder.create(URI.create("system:ephemeris"))
                .withParameter(ConfigDescriptionParameterBuilder.create(PARAM_COUNTRY, Type.TEXT).build()).build();
        when(configDescriptionRegistryMock.getConfigDescriptions(any()))
                .thenReturn(List.of(systemI18n, systemEphemeris));
        when(configDescriptionRegistryMock.getConfigDescription(eq(systemI18nURI), any())).thenReturn(systemI18n);
        resource = new ConfigDescriptionResource(configDescriptionRegistryMock, localeServiceMock);
    public void shouldReturnAllConfigDescriptions() throws IOException {
        Response response = resource.getAll(null, null);
        assertThat(JsonParser.parseString(toString(response.getEntity())), is(JsonParser.parseString(
                "[{\"uri\":\"system:i18n\",\"parameters\":[{\"default\":\"test\",\"name\":\"name\",\"required\":false,\"type\":\"TEXT\",\"readOnly\":false,\"multiple\":false,\"advanced\":false,\"verify\":false,\"limitToOptions\":true,\"options\":[],\"filterCriteria\":[]}],\"parameterGroups\":[]},{\"uri\":\"system:ephemeris\",\"parameters\":[{\"name\":\"country\",\"required\":false,\"type\":\"TEXT\",\"readOnly\":false,\"multiple\":false,\"advanced\":false,\"verify\":false,\"limitToOptions\":true,\"options\":[],\"filterCriteria\":[]}],\"parameterGroups\":[]}]")));
    public void shouldReturnAConfigDescription() throws IOException {
        Response response = resource.getByURI(null, CONFIG_DESCRIPTION_SYSTEM_I18N_URI);
                "{\"uri\":\"system:i18n\",\"parameters\":[{\"default\":\"test\",\"name\":\"name\",\"required\":false,\"type\":\"TEXT\",\"readOnly\":false,\"multiple\":false,\"advanced\":false,\"verify\":false,\"limitToOptions\":true,\"options\":[],\"filterCriteria\":[]}],\"parameterGroups\":[]}")));
    public void shouldReturnStatus404() {
        Response response = resource.getByURI(null, "uri:invalid");
        assertThat(response.getStatus(), is(404));
    public String toString(Object entity) throws IOException {
        byte[] bytes;
        if (entity instanceof StreamingOutput streaming) {
            try (ByteArrayOutputStream buffer = new ByteArrayOutputStream()) {
                streaming.write(buffer);
                bytes = buffer.toByteArray();
            bytes = ((InputStream) entity).readAllBytes();
        return new String(bytes, StandardCharsets.UTF_8);
