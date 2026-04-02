import static org.openhab.core.config.core.ConfigDescriptionParameter.Type.*;
public class ConfigUtilTest {
    private final URI configUri = URI.create("system:ephemeris");
    private final ConfigDescriptionParameterBuilder configDescriptionParameterBuilder1 = ConfigDescriptionParameterBuilder
            .create("p1", DECIMAL).withMultiple(true).withMultipleLimit(7);
    private final ConfigDescriptionParameterBuilder configDescriptionParameterBuilder2 = ConfigDescriptionParameterBuilder
            .create("p2", TEXT).withMultiple(true).withMultipleLimit(2);
    public void verifyNormalizeDefaultTypeForTextReturnsString() {
        assertThat(ConfigUtil.getDefaultValueAsCorrectType(
                ConfigDescriptionParameterBuilder.create("test", TEXT).withDefault("foo").build()), is("foo"));
                ConfigDescriptionParameterBuilder.create("test", TEXT).withDefault("1.0").build()), is("1.0"));
    public void verifyNormalizeDefaultTypeForBooleanReturnsBoolean() {
        assertThat(
                ConfigUtil.getDefaultValueAsCorrectType(
                        ConfigDescriptionParameterBuilder.create("test", BOOLEAN).withDefault("true").build()),
                is(Boolean.TRUE));
                        ConfigDescriptionParameterBuilder.create("test", BOOLEAN).withDefault("YES").build()),
                is(Boolean.FALSE));
    public void verifyNormalizeDefaultTypeForIntegerReturnsIntegerOrNull() {
                        ConfigDescriptionParameterBuilder.create("test", INTEGER).withDefault("1").build()),
                is(BigDecimal.ONE));
                        ConfigDescriptionParameterBuilder.create("test", INTEGER).withDefault("1.2").build()),
                        ConfigDescriptionParameterBuilder.create("test", INTEGER).withDefault("foo").build()),
                is(nullValue()));
    public void verifyNormalizeDefaultTypeForDecimalReturnsimalBigDecOrNull() {
                        ConfigDescriptionParameterBuilder.create("test", DECIMAL).withDefault("1").build()),
                        ConfigDescriptionParameterBuilder.create("test", DECIMAL).withDefault("1.2").build()),
                is(new BigDecimal("1.2")));
                        ConfigDescriptionParameterBuilder.create("test", DECIMAL).withDefault("foo").build()),
    public void verifyGetNumberOfDecimalPlacesWorksCorrectly() {
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("0.001")), is(3));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("0.01")), is(2));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("0.1")), is(1));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("1.000")), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("1.00")), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("1.0")), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(BigDecimal.ONE), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("10")), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("100")), is(0));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("100.1")), is(1));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("100.01")), is(2));
        assertThat(ConfigUtil.getNumberOfDecimalPlaces(new BigDecimal("100.001")), is(3));
    public void verifyApplyDefaultConfigurationReturnsNullIfNotSet() {
        ConfigDescription configDescription = ConfigDescriptionBuilder.create(configUri)
                .withParameter(configDescriptionParameterBuilder2.build()).build();
        ConfigUtil.applyDefaultConfiguration(configuration, configDescription);
        assertThat(configuration.get("p2"), is(nullValue()));
    public void verifyApplyDefaultConfigurationReturnsAListWithASingleValues() {
        configDescriptionParameterBuilder1.withDefault("2.5");
                .withParameter(configDescriptionParameterBuilder1.build()).build();
        verifyValuesOfConfiguration(configuration.get("p1"), 1, List.of(new BigDecimal("2.5")));
    public void verifyApplyDefaultConfigurationReturnsAListWithMultipleValues() {
        configDescriptionParameterBuilder1.withDefault("2.3,2.4,2.5");
        verifyValuesOfConfiguration(configuration.get("p1"), 3,
                List.of(new BigDecimal("2.3"), new BigDecimal("2.4"), new BigDecimal("2.5")));
    public void verifyApplyDefaultConfigurationIgnoresWrongTypes() {
        configDescriptionParameterBuilder1.withDefault("2.3,2.4,foo,2.5");
    public void verifyApplyDefaultConfigurationReturnsAListWithTrimmedValues() {
        configDescriptionParameterBuilder2.withDefault("first value,  second value  ,third value,,,");
        verifyValuesOfConfiguration(configuration.get("p2"), 3, List.of("first value", "second value", "third value"));
    private void verifyValuesOfConfiguration(Object subject, int expectedSize, List<?> expectedValues) {
        assertThat(subject, is(notNullValue()));
        assertThat(subject, is(instanceOf(List.class)));
        assertThat(((List<?>) subject).size(), is(expectedSize));
        assertThat(((List<?>) subject), is(expectedValues));
    public void firstDesciptionWinsForNormalization() {
        ConfigDescription configDescriptionInteger = ConfigDescriptionBuilder.create(URI.create("thing:fooThing"))
                .withParameter(ConfigDescriptionParameterBuilder.create("foo", INTEGER).build()).build();
        ConfigDescription configDescriptionString = ConfigDescriptionBuilder.create(URI.create("thingType:fooThing"))
                .withParameter(ConfigDescriptionParameterBuilder.create("foo", TEXT).build()).build();
        assertThat(ConfigUtil.normalizeTypes(Map.of("foo", "1"), List.of(configDescriptionInteger)).get("foo"),
                is(instanceOf(BigDecimal.class)));
        assertThat(ConfigUtil.normalizeTypes(Map.of("foo", "1"), List.of(configDescriptionString)).get("foo"),
                is(instanceOf(String.class)));
        assertThat(ConfigUtil
                .normalizeTypes(Map.of("foo", "1"), List.of(configDescriptionInteger, configDescriptionString))
                .get("foo"), is(instanceOf(BigDecimal.class)));
                .normalizeTypes(Map.of("foo", "1"), List.of(configDescriptionString, configDescriptionInteger))
                .get("foo"), is(instanceOf(String.class)));
