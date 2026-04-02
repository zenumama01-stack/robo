 * The {@link YamlGroupDTOTest} contains tests for the {@link YamlGroupDTO} class.
public class YamlGroupDTOTest {
    public void testGetBaseType() throws IOException {
        YamlGroupDTO gr = new YamlGroupDTO();
        assertEquals(null, gr.getBaseType());
        gr.type = "Number";
        assertEquals("Number", gr.getBaseType());
        gr.type = "number";
        gr.dimension = "Dimensionless";
        assertEquals("Number:Dimensionless", gr.getBaseType());
        gr.dimension = "dimensionless";
    public void testGetFunction() throws IOException {
        assertEquals("EQUALITY", gr.getFunction());
        gr.function = "AND";
        assertEquals("AND", gr.getFunction());
        gr.function = "or";
        assertEquals("OR", gr.getFunction());
        gr.function = "Min";
        assertEquals("MIN", gr.getFunction());
    public void testIsValid() throws IOException {
        List<String> err = new ArrayList<>();
        List<String> warn = new ArrayList<>();
        assertTrue(gr.isValid(err, warn));
        gr.type = "String";
        gr.type = "string";
        gr.type = "Other";
        assertFalse(gr.isValid(err, warn));
        gr.dimension = "Other";
        gr.type = "Color";
        gr.dimension = null;
        assertEquals("Color", gr.getBaseType());
        gr.function = "invalid";
    public void testEquals() throws IOException {
        YamlGroupDTO gr1 = new YamlGroupDTO();
        YamlGroupDTO gr2 = new YamlGroupDTO();
        gr1.type = "String";
        gr2.type = "String";
        assertTrue(gr1.equals(gr2));
        assertEquals(gr1.hashCode(), gr2.hashCode());
        gr2.type = "Number";
        assertFalse(gr1.equals(gr2));
        gr1.type = "Number";
        gr2.type = "number";
        gr1.dimension = "Temperature";
        gr2.dimension = "Humidity";
        gr2.dimension = "Temperature";
        gr2.dimension = "temperature";
        gr1.function = "or";
        gr2.function = null;
        gr1.function = null;
        gr2.function = "or";
        gr2.function = "Or";
        gr2.function = "OR";
        gr1.parameters = List.of("ON", "OFF");
        gr2.parameters = null;
        gr2.parameters = List.of("ON");
        gr1.parameters = null;
        gr2.parameters = List.of("ON", "OFF");
        gr1.parameters = List.of("ON");
