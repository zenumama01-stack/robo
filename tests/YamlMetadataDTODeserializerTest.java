 * Tests for {@link YamlMetadataDTODeserializer}.
class YamlMetadataDTODeserializerTest {
    private final ObjectMapper mapper = new ObjectMapper(new YAMLFactory());
    @ValueSource(strings = { "string value", "123", "45.67", "true", "false" })
    void shouldDeserializeScalarAsValueWithEmptyConfig(String scalar) throws IOException {
        String yaml = "ns: " + scalar;
        YamlMetadataDTO dto = mapper.treeToValue(mapper.readTree(yaml).get("ns"), YamlMetadataDTO.class);
        assertEquals(scalar, dto.getValue());
        assertNull(dto.config);
    void shouldDeserializeObjectWithValueAndConfig() throws IOException {
        String yaml = "ns: { value: bar, config: { a: 1, b: two } }";
        assertEquals("bar", dto.getValue());
        assertNotNull(dto.config);
        assertEquals(1, dto.config.get("a"));
        assertEquals("two", dto.config.get("b"));
    void shouldDeserializeObjectWithValueAndNoConfig() throws IOException {
        String yaml = "ns: { value: bar }";
    void shouldDeserializeObjectWithNoValueAndConfig() throws IOException {
        String yaml = "ns: { config: { a: 1, b: two } }";
        assertEquals("", dto.getValue());
    void shouldDeserializeEmptyObjectAsEmptyDto() throws IOException {
        String yaml = "ns: { }";
    @ValueSource(strings = { "null", "", "''" })
    void shouldDeserializeNullAndEmptyStringAsEmptyDto(String scalar) throws IOException {
