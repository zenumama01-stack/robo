package org.openhab.core.model.script.actions;
import static org.junit.jupiter.api.Assertions.assertNull;
import org.openhab.core.model.script.internal.engine.action.SemanticsActionService;
import org.openhab.core.semantics.Tag;
import org.openhab.core.semantics.internal.SemanticTagRegistryImpl;
import org.openhab.core.semantics.model.DefaultSemanticTagProvider;
 * This are tests for {@link Semantics} actions.
public class SemanticsTest {
    private @Mock @NonNullByDefault({}) UnitProvider unitProviderMock;
    private @Mock @NonNullByDefault({}) ManagedSemanticTagProvider managedSemanticTagProviderMock;
    private @NonNullByDefault({}) GroupItem indoorLocationItem;
    private @NonNullByDefault({}) GroupItem bathroomLocationItem;
    private @NonNullByDefault({}) GroupItem equipmentItem;
    private @NonNullByDefault({}) GenericItem temperaturePointItem;
    private @NonNullByDefault({}) GenericItem humidityPointItem;
    private @NonNullByDefault({}) GenericItem subEquipmentItem;
    private @NonNullByDefault({}) Class<? extends Tag> indoorTagClass;
    private @NonNullByDefault({}) Class<? extends Tag> bathroomTagClass;
    private @NonNullByDefault({}) Class<? extends Tag> cleaningRobotTagClass;
    private @NonNullByDefault({}) Class<? extends Tag> batteryTagClass;
        CoreItemFactory itemFactory = new CoreItemFactory(unitProviderMock);
        indoorLocationItem = new GroupItem("TestHouse");
        indoorLocationItem.addTag("Indoor");
        bathroomLocationItem = new GroupItem("TestBathRoom");
        bathroomLocationItem.addTag("Bathroom");
        // Bathroom is placed in Indoor
        indoorLocationItem.addMember(bathroomLocationItem);
        bathroomLocationItem.addGroupName(indoorLocationItem.getName());
        equipmentItem = new GroupItem("Test08");
        equipmentItem.addTag("CleaningRobot");
        // Equipment (Cleaning Robot) is placed in Bathroom
        bathroomLocationItem.addMember(equipmentItem);
        equipmentItem.addGroupName(bathroomLocationItem.getName());
        temperaturePointItem = itemFactory.createItem(CoreItemFactory.NUMBER, "TestTemperature");
        temperaturePointItem.addTag("Measurement");
        temperaturePointItem.addTag("Temperature");
        // Temperature Point is Property of Equipment (Cleaning Robot)
        equipmentItem.addMember(temperaturePointItem);
        temperaturePointItem.addGroupName(equipmentItem.getName());
        humidityPointItem = itemFactory.createItem(CoreItemFactory.NUMBER, "TestHumidity");
        humidityPointItem.addTag("Measurement");
        humidityPointItem.addTag("Humidity");
        subEquipmentItem = itemFactory.createItem(CoreItemFactory.NUMBER, "TestBattery");
        subEquipmentItem.addTag("Battery");
        // Equipment (TestBattery) is a part of Equipment (Cleaning Robot)
        equipmentItem.addMember(subEquipmentItem);
        subEquipmentItem.addGroupName(equipmentItem.getName());
        when(managedSemanticTagProviderMock.getAll()).thenReturn(List.of());
        SemanticTagRegistryImpl semanticTagRegistryImpl = new SemanticTagRegistryImpl(new DefaultSemanticTagProvider(),
                managedSemanticTagProviderMock);
        indoorTagClass = semanticTagRegistryImpl.getTagClassById("Location_Indoor");
        bathroomTagClass = semanticTagRegistryImpl.getTagClassById("Location_Indoor_Room_Bathroom");
        cleaningRobotTagClass = semanticTagRegistryImpl.getTagClassById("Equipment_CleaningRobot");
        batteryTagClass = semanticTagRegistryImpl.getTagClassById("Equipment_PowerSupply_Battery");
        when(itemRegistryMock.getItem("TestHouse")).thenReturn(indoorLocationItem);
        when(itemRegistryMock.getItem("TestBathRoom")).thenReturn(bathroomLocationItem);
        when(itemRegistryMock.getItem("Test08")).thenReturn(equipmentItem);
        when(itemRegistryMock.getItem("TestTemperature")).thenReturn(temperaturePointItem);
        when(itemRegistryMock.getItem("TestHumidity")).thenReturn(humidityPointItem);
        new SemanticsActionService(itemRegistryMock);
    public void testGetLocation() {
        assertThat(Semantics.getLocation(indoorLocationItem), is(nullValue()));
        assertThat(Semantics.getLocation(bathroomLocationItem), is(indoorLocationItem));
        assertThat(Semantics.getLocation(equipmentItem), is(bathroomLocationItem));
        assertThat(Semantics.getLocation(temperaturePointItem), is(bathroomLocationItem));
        assertNull(Semantics.getLocation(humidityPointItem));
    public void testGetLocationType() {
        assertThat(Semantics.getLocationType(indoorLocationItem), is(indoorTagClass));
        assertThat(Semantics.getLocationType(bathroomLocationItem), is(bathroomTagClass));
        assertNull(Semantics.getLocationType(humidityPointItem));
    public void testGetEquipment() {
        assertThat(Semantics.getEquipment(equipmentItem), is(nullValue()));
        assertThat(Semantics.getEquipment(subEquipmentItem), is(equipmentItem));
        assertThat(Semantics.getEquipment(temperaturePointItem), is(equipmentItem));
        assertNull(Semantics.getEquipment(humidityPointItem));
    public void testGetEquipmentType() {
        assertThat(Semantics.getEquipmentType(equipmentItem), is(cleaningRobotTagClass));
        assertThat(Semantics.getEquipmentType(temperaturePointItem), is(cleaningRobotTagClass));
        assertThat(Semantics.getEquipmentType(subEquipmentItem), is(batteryTagClass));
        assertNull(Semantics.getEquipmentType(humidityPointItem));
