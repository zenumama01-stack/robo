 * Tests for {@link YamlMetadataDTOSerializer}.
class YamlMetadataDTOSerializerTest {
    void testSerializeScalarWhenConfigEmpty() throws Exception {
        dto.value = "scalarValue";
        dto.config = null;
        String yaml = mapper.writeValueAsString(dto).trim();
        assertEquals("--- \"scalarValue\"", yaml);
    void testSerializeScalarWhenConfigEmptyMap() throws Exception {
        dto.config = new HashMap<>();
    void testSerializeObjectWhenConfigPresent() throws Exception {
        dto.value = "objectValue";
        Map<String, Object> config = new HashMap<>();
        config.put("foo", "bar");
        dto.config = config;
        System.out.println("YAML output:\n" + yaml);
        // Check for document start and all expected fields
        assert yaml.startsWith("---");
        assert yaml.contains("value: \"objectValue\"");
        assert yaml.contains("config:");
        assert yaml.contains("foo: \"bar\"");
