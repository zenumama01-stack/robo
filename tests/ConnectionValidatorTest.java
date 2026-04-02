public class ConnectionValidatorTest {
    public void testValidConnections() {
        assertTrue(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "$name"));
        assertTrue(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "${name}"));
        assertTrue(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "moduleId.outputName"));
        assertTrue(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.list[1].name.values"));
        assertTrue(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module1.map[\"na[m}.\"e\"][1].values_1-2"));
    public void testInvalidConnections() {
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "$name}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "{name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "name}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "${name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "${name.values}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "$name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "moduleId.outputName."));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "[1].name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "list.[1]name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, ".module.array[1].name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module1.map\"na[m}.\"e\"]"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.map[\"na[m}.\"e\""));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.list[1.name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.list1].name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.list[1].name."));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module[\"name\"]"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.[name]"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module[\"name]"));
        assertFalse(Pattern.matches(ConnectionValidator.CONNECTION_PATTERN, "module.[name\"]"));
    public void testInvalidConfigReference() {
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, ""));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "$name}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "{name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "name}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "${name"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "${name.values}"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "$name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "[1].name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "${name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.CONFIG_REFERENCE_PATTERN, "$name.values}"));
    public void testValidOutputReference() {
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "[1]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".phones[1]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".phones[1].number"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "[\"test\"]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "[\"test\"].name"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".map[\"test\"]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".map[\"test\"].name"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".bean.values"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".bean.array[1]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".bean.map[\"na[m}.\"e\"]"));
        assertTrue(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".bean.map[\"na[m}.\"e\"].values"));
    public void testInvalidOutputReference() {
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "phones."));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "phones["));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "]phones"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "phones[].name"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "phones[\"\"].name"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "list.[1]name.values"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, ".map\"na[m}.\"e\"]"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.map[\"na[m}.\"e\""));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.list[1.name"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.list1].name"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.list[1].name."));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module[\"name\"]"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.[name]"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module[\"name]"));
        assertFalse(Pattern.matches(ConnectionValidator.OUTPUT_REFERENCE_PATTERN, "module.[name\"]"));
