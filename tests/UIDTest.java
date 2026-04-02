public class UIDTest {
        assertThrows(IllegalArgumentException.class, () -> new ThingUID("binding:type:id_with_invalidchar#"));
    public void testValidUIDs() {
        new ThingUID("binding:type:id-1");
        new ThingUID("binding:type:id_1");
        new ThingUID("binding:type:ID");
        new ThingUID("00:type:ID");
