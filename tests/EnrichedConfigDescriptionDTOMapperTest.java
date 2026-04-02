public class EnrichedConfigDescriptionDTOMapperTest {
    private static final String CONFIG_PARAMETER_NAME = "test";
    private static final String CONFIG_PARAMETER_DEFAULT_VALUE = "first value,second value,third value";
    public void testThatDefaultValuesAreEmptyIfMultipleIsTrue() {
        ConfigDescriptionParameter configDescriptionParameter = ConfigDescriptionParameterBuilder
                .create(CONFIG_PARAMETER_NAME, Type.TEXT).withMultiple(true).build();
        ConfigDescription configDescription = ConfigDescriptionBuilder.create(CONFIG_URI)
                .withParameter(configDescriptionParameter).build();
        ConfigDescriptionDTO cddto = EnrichedConfigDescriptionDTOMapper.map(configDescription);
        assertThat(cddto.parameters, hasSize(1));
        ConfigDescriptionParameterDTO cdpdto = cddto.parameters.getFirst();
        assertThat(cdpdto, instanceOf(EnrichedConfigDescriptionParameterDTO.class));
        assertThat(cdpdto.defaultValue, is(nullValue()));
        EnrichedConfigDescriptionParameterDTO ecdpdto = (EnrichedConfigDescriptionParameterDTO) cdpdto;
        assertThat(ecdpdto.defaultValues, is(nullValue()));
    public void testThatDefaultValueIsNotAList() {
                .create(CONFIG_PARAMETER_NAME, Type.TEXT).withDefault(CONFIG_PARAMETER_DEFAULT_VALUE).build();
        assertThat(cdpdto.defaultValue, is(CONFIG_PARAMETER_DEFAULT_VALUE));
    public void testThatDefaultValuesAreAList() {
                .create(CONFIG_PARAMETER_NAME, Type.TEXT).withDefault(CONFIG_PARAMETER_DEFAULT_VALUE).withMultiple(true)
        assertThat(ecdpdto.defaultValues, is(notNullValue()));
        assertThat(ecdpdto.defaultValues, hasSize(3));
        assertThat(ecdpdto.defaultValues, is(equalTo(List.of("first value", "second value", "third value"))));
    public void testThatDefaultValuesDontSplitEscapedCommas() {
        final String configParameterDefaultValue = "Me\\, myself\\, and I,You \\\\,";
                .create(CONFIG_PARAMETER_NAME, Type.TEXT).withDefault(configParameterDefaultValue).withMultiple(true)
        assertThat(cdpdto.defaultValue, is(configParameterDefaultValue));
        assertThat(ecdpdto.defaultValues, hasSize(2));
        assertThat(ecdpdto.defaultValues, is(equalTo(List.of("Me, myself, and I", "You \\,"))));
