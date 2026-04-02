 * This is the test class for {@link ConfigDescriptionDTO}.
class ConfigDescriptionDTOTest {
    private static final String CONFIG_DESCRIPTION_SYSTEM_I18N_URI = "system:i18n";
    private static final String PARAM_NAME = "name";
    private static final String PARAMETER_NAME_CONTEXT_NETWORK_ADDRESS = "network-address";
    private static final String PARAMETER_NAME_DEFAULT_VALUE = "test";
    private static final String PARAMETER_NAME_DESCRIPTION = "description";
    private static final List<FilterCriteria> PARAMETER_NAME_FILTER = List.of(new FilterCriteria("name", "value"));
    private static final String PARAMETER_NAME_GROUP = "group";
    private static final String PARAMETER_NAME_LABEL = "label";
    private static final List<ParameterOption> PARAMETER_NAME_OPTIONS = List.of(new ParameterOption("value", "label"));
    private static final String PARAMETER_NAME_PATTERN = "%.1f";
    private static final String PARAMETER_NAME_UNIT = "ms";
    private static final String PARAMETER_NAME_UNIT_LABEL = "milliseconds";
    private static final String PARAMETER_GROUP_NAME = "name";
    private static final String PARAMETER_GROUP_CONTEXT = "context";
    private static final String PARAMETER_GROUP_DESCRIPTION = "description";
    private static final String PARAMETER_GROUP_LABEL = "label";
    public void testConfigDescriptionDTOMappingContainsDecodedURIString() {
        final URI systemI18nURI = URI.create(CONFIG_DESCRIPTION_SYSTEM_I18N_URI);
        ConfigDescription subject = ConfigDescriptionBuilder.create(systemI18nURI).build();
        ConfigDescriptionDTO result = ConfigDescriptionDTOMapper.map(subject);
        assertThat(result.uri, is(CONFIG_DESCRIPTION_SYSTEM_I18N_URI));
    public void testConfigDescriptionParameterDTOMappingIsBidirectional() {
        List<ConfigDescriptionParameter> subject = List.of(ConfigDescriptionParameterBuilder
                .create(PARAM_NAME, Type.INTEGER).withAdvanced(true).withContext(PARAMETER_NAME_CONTEXT_NETWORK_ADDRESS)
                .withDefault(PARAMETER_NAME_DEFAULT_VALUE).withDescription(PARAMETER_NAME_DESCRIPTION)
                .withFilterCriteria(PARAMETER_NAME_FILTER).withGroupName(PARAMETER_NAME_GROUP)
                .withLabel(PARAMETER_NAME_LABEL).withLimitToOptions(false).withMaximum(BigDecimal.TEN)
                .withMinimum(BigDecimal.ZERO).withMultiple(true).withMultipleLimit(Integer.valueOf(3))
                .withOptions(PARAMETER_NAME_OPTIONS).withPattern(PARAMETER_NAME_PATTERN).withReadOnly(false)
                .withRequired(true).withStepSize(BigDecimal.ONE).withUnit(PARAMETER_NAME_UNIT)
                .withUnitLabel(PARAMETER_NAME_UNIT_LABEL).withVerify(true).build());
        List<ConfigDescriptionParameter> result = ConfigDescriptionDTOMapper
                .map(ConfigDescriptionDTOMapper.mapParameters(subject));
        assertThat(result, hasSize(1));
        ConfigDescriptionParameter parameter = result.getFirst();
        assertThat(parameter.getName(), is(PARAM_NAME));
        assertThat(parameter.isAdvanced(), is(true));
        assertThat(parameter.getContext(), is(PARAMETER_NAME_CONTEXT_NETWORK_ADDRESS));
        assertThat(parameter.getDefault(), is(PARAMETER_NAME_DEFAULT_VALUE));
        assertThat(parameter.getDescription(), is(PARAMETER_NAME_DESCRIPTION));
        assertThat(parameter.getFilterCriteria(), is(PARAMETER_NAME_FILTER));
        assertThat(parameter.getGroupName(), is(PARAMETER_NAME_GROUP));
        assertThat(parameter.getLabel(), is(PARAMETER_NAME_LABEL));
        assertThat(parameter.getLimitToOptions(), is(false));
        assertThat(parameter.getMaximum(), is(BigDecimal.TEN));
        assertThat(parameter.getMinimum(), is(BigDecimal.ZERO));
        assertThat(parameter.isMultiple(), is(true));
        assertThat(parameter.getMultipleLimit(), is(Integer.valueOf(3)));
        assertThat(parameter.getOptions(), is(PARAMETER_NAME_OPTIONS));
        assertThat(parameter.getPattern(), is(PARAMETER_NAME_PATTERN));
        assertThat(parameter.isReadOnly(), is(false));
        assertThat(parameter.isRequired(), is(true));
        assertThat(parameter.getStepSize(), is(BigDecimal.ONE));
        assertThat(parameter.getUnit(), is(PARAMETER_NAME_UNIT));
        assertThat(parameter.getUnitLabel(), is(PARAMETER_NAME_UNIT_LABEL));
        assertThat(parameter.isVerifyable(), is(true));
    public void testConfigDescriptionParameterGroupDTOMappingIsBidirectional() {
        List<ConfigDescriptionParameterGroup> subject = List.of(ConfigDescriptionParameterGroupBuilder
                .create(PARAMETER_GROUP_NAME).withAdvanced(true).withContext(PARAMETER_GROUP_CONTEXT)
                .withDescription(PARAMETER_GROUP_DESCRIPTION).withLabel(PARAMETER_GROUP_LABEL).build());
        List<ConfigDescriptionParameterGroup> result = ConfigDescriptionDTOMapper
                .mapParameterGroupsDTO(ConfigDescriptionDTOMapper.mapParameterGroups(subject));
        ConfigDescriptionParameterGroup parameterGroup = result.getFirst();
        assertThat(parameterGroup.getName(), is(PARAMETER_GROUP_NAME));
        assertThat(parameterGroup.isAdvanced(), is(true));
        assertThat(parameterGroup.getContext(), is(PARAMETER_GROUP_CONTEXT));
        assertThat(parameterGroup.getDescription(), is(PARAMETER_GROUP_DESCRIPTION));
        assertThat(parameterGroup.getLabel(), is(PARAMETER_GROUP_LABEL));
