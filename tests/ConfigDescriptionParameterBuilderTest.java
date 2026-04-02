 * Tests for {@link ConfigDescriptionParameterBuilder) class.
public class ConfigDescriptionParameterBuilderTest {
    public void assertThatCreatedConfigDescriptionParameterReturnExpectedValues() {
        String name = "Dummy";
        Type type = Type.INTEGER;
        BigDecimal min = new BigDecimal(2.0);
        BigDecimal max = new BigDecimal(4.0);
        BigDecimal stepSize = new BigDecimal(1.0);
        String pattern = "pattern";
        boolean verify = true;
        boolean readOnly = true;
        boolean multiple = false;
        String context = "context";
        String defaultVal = "default";
        String label = "label";
        String description = "description";
        String unit = "m";
        String unitLabel = "unitLabel";
        ParameterOption[] options = new ParameterOption[] { new ParameterOption("val", "label") };
        FilterCriteria[] criterias = new FilterCriteria[] { new FilterCriteria("name", "value") };
        String groupName = "groupName";
        boolean advanced = false;
        boolean limitToOptions = true;
        Integer multipleLimit = Integer.valueOf(17);
        //@formatter:off
        ConfigDescriptionParameter param = ConfigDescriptionParameterBuilder.create(name, type)
                .withMinimum(min)
                .withMaximum(max)
                .withStepSize(stepSize)
                .withPattern(pattern)
                .withRequired(required)
                .withReadOnly(readOnly)
                .withMultiple(multiple)
                .withContext(context)
                .withDefault(defaultVal)
                .withLabel(label)
                .withDescription(description)
                .withOptions(List.of(options))
                .withFilterCriteria(List.of(criterias))
                .withGroupName(groupName)
                .withAdvanced(advanced)
                .withLimitToOptions(limitToOptions)
                .withMultipleLimit(multipleLimit)
                .withUnit(unit)
                .withUnitLabel(unitLabel)
                .withVerify(verify)
        //@formatter:on
        assertThat(param.getMinimum(), is(min));
        assertThat(param.getMaximum(), is(max));
        assertThat(param.getStepSize(), is(stepSize));
        assertThat(param.getPattern(), is(pattern));
        assertThat(param.getDefault(), is(defaultVal));
        assertThat(param.getGroupName(), is(groupName));
        assertThat(param.getMultipleLimit(), is(multipleLimit));
        assertThat(param.getFilterCriteria(), hasItems(criterias));
        assertThat(param.getOptions(), hasItems(options));
        assertThat(param.getUnit(), is(unit));
        assertThat(param.getUnitLabel(), is(unitLabel));
        assertFalse(param.isRequired());
        assertTrue(param.isReadOnly());
        assertFalse(param.isMultiple());
        assertTrue(param.isVerifyable());
        assertFalse(param.isAdvanced());
        assertTrue(param.getLimitToOptions());
        param = ConfigDescriptionParameterBuilder.create(name, type).withUnitLabel(unitLabel).build();
    public void assertThatGetterForNotNullableAttributesInitializedWithNullReturnExpectedValues() {
        ConfigDescriptionParameter param = ConfigDescriptionParameterBuilder.create("Dummy", Type.BOOLEAN)
                .withMinimum(null)
                .withMaximum(null)
                .withStepSize(null)
                .withPattern(null)
                .withRequired(null)
                .withReadOnly(null)
                .withMultiple(null)
                .withVerify(null)
                .withContext(null)
                .withDefault(null)
                .withLabel(null)
                .withDescription(null)
                .withOptions(null)
                .withFilterCriteria(null)
                .withGroupName(null)
                .withAdvanced(null)
                .withLimitToOptions(null)
                .withMultipleLimit(null)
                .withUnit(null)
                .withUnitLabel(null)
        // nullable attributes
        assertThat(param.getMinimum(), is(nullValue()));
        assertThat(param.getMaximum(), is(nullValue()));
        assertThat(param.getStepSize(), is(nullValue()));
        assertThat(param.getPattern(), is(nullValue()));
        assertThat(param.getContext(), is(nullValue()));
        assertThat(param.getDefault(), is(nullValue()));
        assertThat(param.getLabel(), is(nullValue()));
        assertThat(param.getDescription(), is(nullValue()));
        assertThat(param.getGroupName(), is(nullValue()));
        assertThat(param.getMultipleLimit(), is(nullValue()));
        assertThat(param.getUnit(), is(nullValue()));
        assertThat(param.getUnitLabel(), is(nullValue()));
        // list attributes
        assertTrue(param.getFilterCriteria().isEmpty());
        assertTrue(param.getOptions().isEmpty());
        // boolean attributes
        assertFalse(param.isReadOnly());
        ConfigDescriptionParameter param2 = ConfigDescriptionParameterBuilder.create("Dummy", Type.BOOLEAN).build();
        assertFalse(param2.isRequired());
        assertFalse(param2.isReadOnly());
        assertFalse(param2.isMultiple());
        assertFalse(param2.isAdvanced());
        assertTrue(param2.getLimitToOptions());
        assertTrue(param2.getFilterCriteria().isEmpty());
        assertTrue(param2.getOptions().isEmpty());
    public void assertThatNameMustNotBeNull() {
                () -> ConfigDescriptionParameterBuilder.create(null, Type.BOOLEAN).build());
    public void assertThatNameMustNotBeEmpty() {
                () -> ConfigDescriptionParameterBuilder.create("", Type.BOOLEAN).build());
    public void assertThatTypeMustNotBeNull() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", null).build());
    public void assertThatUnitForTextParameterIsNotAllowed() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", Type.TEXT).withUnit("m").build());
    public void assertThatUnitForBooleanParameterIsNotAllowed() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", Type.BOOLEAN).withUnit("m").build());
    public void assertThatUnitLabelForTextParameterIsNotAllowed() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", Type.TEXT).withUnitLabel("Runs").build());
    public void assertThatUnitLabelForBooleanParameterIsNotAllowed() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", Type.BOOLEAN).withUnitLabel("Runs").build());
    public void assertThatAparameterWithAnInvalidUnitCannotBeCreated() {
                () -> ConfigDescriptionParameterBuilder.create("Dummy", Type.BOOLEAN).withUnit("invalid").build());
