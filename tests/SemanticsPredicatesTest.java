 * These are tests for {@link SemanticsPredicates}.
public class SemanticsPredicatesTest {
    public void testIsLocation() {
        assertTrue(SemanticsPredicates.isLocation().test(locationItem));
        assertFalse(SemanticsPredicates.isLocation().test(equipmentItem));
        assertFalse(SemanticsPredicates.isLocation().test(pointItem));
    public void testIsEquipment() {
        assertFalse(SemanticsPredicates.isEquipment().test(locationItem));
        assertTrue(SemanticsPredicates.isEquipment().test(equipmentItem));
        assertFalse(SemanticsPredicates.isEquipment().test(pointItem));
    public void testIsPoint() {
        assertFalse(SemanticsPredicates.isPoint().test(locationItem));
        assertFalse(SemanticsPredicates.isPoint().test(equipmentItem));
        assertTrue(SemanticsPredicates.isPoint().test(pointItem));
    public void testRelatesTo() {
        Class<? extends Property> temperatureTagClass = (Class<? extends Property>) Objects
                .requireNonNull(SemanticTags.getById("Property_Temperature"));
        Class<? extends Property> humidityTagClass = (Class<? extends Property>) Objects
                .requireNonNull(SemanticTags.getById("Property_Humidity"));
        assertFalse(SemanticsPredicates.relatesTo(temperatureTagClass).test(locationItem));
        assertFalse(SemanticsPredicates.relatesTo(temperatureTagClass).test(equipmentItem));
        assertTrue(SemanticsPredicates.relatesTo(temperatureTagClass).test(pointItem));
        assertFalse(SemanticsPredicates.relatesTo(humidityTagClass).test(equipmentItem));
