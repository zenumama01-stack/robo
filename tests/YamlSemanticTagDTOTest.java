 * Test class for {@link YamlSemanticTagDTO} to verify correct serialization and deserialization
 * of semantic tags in both short-form and detailed object-form.
class YamlSemanticTagDTOTest {
    private final ObjectMapper mapper = new ObjectMapper();
     * Verifies short-form syntax: { "Tag_uid": "label value" }
    void parsesShortFormTag() throws Exception {
        String json = """
                {"Tag_uid":"label value"}
        Map<String, YamlSemanticTagDTO> tags = mapper.readValue(json,
                mapper.getTypeFactory().constructMapType(Map.class, String.class, YamlSemanticTagDTO.class));
        YamlSemanticTagDTO tag = tags.get("Tag_uid");
        assertThat(tag, notNullValue());
        assertThat(tag.label, is("label value"));
        assertThat(tag.description, nullValue());
        assertThat(tag.synonyms, anyOf(nullValue(), empty()));
     * Verifies object-form syntax using default mapping:
     * { "Tag_uid": { "label": "Label", "description": "Desc", "synonyms": ["a", "b"] } }
    void parsesDetailedFormTag() throws Exception {
                {"Tag_uid":{"label":"Label","description":"Desc","synonyms":["a","b"]}}
        assertThat(tag.label, is("Label"));
        assertThat(tag.description, is("Desc"));
        assertThat(tag.synonyms, contains("a", "b"));
     * Verifies that a simple DTO serializes to a plain String (Short-form)
    void serializesToShortForm() throws Exception {
        dto.label = "label value";
        String json = mapper.writeValueAsString(dto);
        // Should be "label value", not {"label":"label value"...}
        assertThat(json, is("\"label value\""));
     * Verifies that a detailed DTO serializes to a full JSON Object (Detailed-form)
    void serializesToDetailedForm() throws Exception {
        dto.label = "Label";
        dto.description = "Desc";
        dto.synonyms = List.of("a", "b");
        // Verify the structure contains the keys
        assertThat(json, containsString("\"label\":\"Label\""));
        assertThat(json, containsString("\"description\":\"Desc\""));
        assertThat(json, containsString("\"synonyms\":[\"a\",\"b\"]"));
        // Ensure it is wrapped in braces
        assertThat(json, startsWith("{"));
        assertThat(json, endsWith("}"));
     * Verifies round-trip consistency: Map -> String -> Map
    void verifiesMapRoundTrip() throws Exception {
        Map<String, YamlSemanticTagDTO> originalTags = new HashMap<>();
        YamlSemanticTagDTO simple = new YamlSemanticTagDTO();
        simple.label = "Simple";
        YamlSemanticTagDTO complex = new YamlSemanticTagDTO();
        complex.label = "Complex";
        complex.description = "With Desc";
        complex.synonyms = List.of("a", "b");
        originalTags.put("tag1", simple);
        originalTags.put("tag2", complex);
        String json = mapper.writeValueAsString(originalTags);
        // Assert the JSON string looks like the mixed format we want
        assertThat(json, containsString("\"tag1\":\"Simple\""));
        assertThat(json, containsString("\"tag2\":{"));
        // Deserialize back and check equality
        Map<String, YamlSemanticTagDTO> result = mapper.readValue(json,
        assertThat(result.get("tag1").label, is("Simple"));
        assertThat(result.get("tag2").description, is("With Desc"));
     * Verifies that a tag with only a description is serialized in map-form
     * and does not include empty label or synonyms fields.
    void serializesDescriptionOnlyAsMapForm() throws Exception {
        Map<String, YamlSemanticTagDTO> tags = new HashMap<>();
        YamlSemanticTagDTO tag = new YamlSemanticTagDTO();
        tag.description = "Only description";
        tags.put("Tag_desc_only", tag);
        String json = mapper.writeValueAsString(tags);
        assertThat(json, containsString("\"Tag_desc_only\":{"));
        assertThat(json, containsString("\"description\":\"Only description\""));
        assertThat(json, not(containsString("\"label\"")));
        assertThat(json, not(containsString("\"synonyms\"")));
     * Verifies that a tag with only synonyms is serialized in map-form
     * and does not include empty label or description fields.
    void serializesSynonymsOnlyAsMapForm() throws Exception {
        tag.synonyms = List.of("s1", "s2");
        tags.put("Tag_syn_only", tag);
        assertThat(json, containsString("\"Tag_syn_only\":{"));
        assertThat(json, containsString("\"synonyms\":[\"s1\",\"s2\"]"));
        assertThat(json, not(containsString("\"description\"")));
