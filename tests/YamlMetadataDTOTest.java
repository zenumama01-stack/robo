 * The {@link YamlMetadataDTOTest} contains tests for the {@link YamlMetadataDTO} class.
public class YamlMetadataDTOTest {
    public void testGetValue() throws IOException {
        md.value = null;
        assertEquals("", md.getValue());
        assertEquals("value", md.getValue());
        md1.value = null;
        md2.value = null;
        assertTrue(md1.equals(md2));
        assertEquals(md1.hashCode(), md2.hashCode());
        md2.value = "";
        md1.value = "";
        assertFalse(md1.equals(md2));
        md2.value = "other value";
    public void testEqualsWithConfigurations() throws IOException {
        md1.config = null;
        md2.config = null;
        md1.config = Map.of();
        md2.config = Map.of();
        md1.config = Map.of("param1", "value", "param2", 50, "param3", true);
        md2.config = Map.of("param1", "value", "param2", 50, "param3", true);
        md2.config = Map.of("param1", "other value", "param2", 50, "param3", true);
        md2.config = Map.of("param1", "value", "param2", 25, "param3", true);
        md2.config = Map.of("param1", "value", "param2", 50, "param3", false);
        md2.config = Map.of("param1", "value", "param2", 50);
        md2.config = Map.of("param3", true, "param2", 50, "param1", "value");
