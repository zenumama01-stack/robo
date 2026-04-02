public class SemanticTagsTest {
    private static final String CUSTOM_LOCATION = "CustomLocation";
    private static final String CUSTOM_EQUIPMENT = "CustomEquipment";
    private static final String CUSTOM_POINT = "CustomPoint";
    private static final String CUSTOM_PROPERTY = "CustomProperty";
    private @NonNullByDefault({}) GroupItem locationItem;
    private @NonNullByDefault({}) GenericItem pointItem;
    private @NonNullByDefault({}) Class<? extends Tag> roomTagClass;
    private @NonNullByDefault({}) Class<? extends Tag> measurementTagClass;
    private @NonNullByDefault({}) Class<? extends Tag> temperatureTagClass;
        locationItem = new GroupItem("TestBathRoom");
        locationItem.addTag("Bathroom");
        pointItem = itemFactory.createItem(CoreItemFactory.NUMBER, "TestTemperature");
        pointItem.addTag("Measurement");
        pointItem.addTag("Temperature");
        SemanticTag customLocationTag = new SemanticTagImpl("Location_" + CUSTOM_LOCATION, null, null, List.of());
        SemanticTag customEquipmentTag = new SemanticTagImpl("Equipment_" + CUSTOM_EQUIPMENT, null, null, List.of());
        SemanticTag customPointTag = new SemanticTagImpl("Point_" + CUSTOM_POINT, null, null, List.of());
        SemanticTag customPropertyTag = new SemanticTagImpl("Property_" + CUSTOM_PROPERTY, null, null, List.of());
        when(managedSemanticTagProviderMock.getAll())
                .thenReturn(List.of(customLocationTag, customEquipmentTag, customPointTag, customPropertyTag));
        new SemanticTagRegistryImpl(new DefaultSemanticTagProvider(), managedSemanticTagProviderMock);
        roomTagClass = SemanticTags.getById("Location_Indoor_Room");
        bathroomTagClass = SemanticTags.getById("Location_Indoor_Room_Bathroom");
        cleaningRobotTagClass = SemanticTags.getById("Equipment_CleaningRobot");
        measurementTagClass = SemanticTags.getById("Point_Measurement");
        temperatureTagClass = SemanticTags.getById("Property_Temperature");
    public void ensureTagsAreUnique() {
        try (BufferedReader reader = new BufferedReader(new FileReader("model/SemanticTags.csv"))) {
            Set<String> tags = new HashSet<>();
            String line;
            reader.readLine(); // Skip the header line
            while ((line = reader.readLine()) != null) {
                // We're only interested in the second column,
                // so quoted fields in synonyms/description are not a problem
                String[] columns = line.split(",");
                String tag = columns[1].trim(); // Tag is in the second column
                assertTrue(tags.add(tag), "Duplicate tag found: " + tag);
            fail("Failed to read SemanticTags.csv. Current dir: " + System.getProperty("user.dir") + " Error: "
                    + e.getMessage());
    public void testTagClasses() {
        assertNotNull(roomTagClass);
        assertNotNull(bathroomTagClass);
        assertNotNull(cleaningRobotTagClass);
        assertNotNull(measurementTagClass);
        assertNotNull(temperatureTagClass);
    public void testByTagId() {
        assertEquals(Location.class, SemanticTags.getById("Location"));
        assertEquals(roomTagClass, SemanticTags.getById("Room"));
        assertEquals(roomTagClass, SemanticTags.getById("Indoor_Room"));
        assertEquals(roomTagClass, SemanticTags.getById("Location_Indoor_Room"));
        assertEquals(bathroomTagClass, SemanticTags.getById("Bathroom"));
        assertEquals(bathroomTagClass, SemanticTags.getById("Room_Bathroom"));
        assertEquals(bathroomTagClass, SemanticTags.getById("Indoor_Room_Bathroom"));
        assertEquals(bathroomTagClass, SemanticTags.getById("Location_Indoor_Room_Bathroom"));
    public void testGetSemanticType() {
        assertEquals(bathroomTagClass, SemanticTags.getSemanticType(locationItem));
        assertEquals(cleaningRobotTagClass, SemanticTags.getSemanticType(equipmentItem));
        assertEquals(measurementTagClass, SemanticTags.getSemanticType(pointItem));
        assertEquals(bathroomTagClass, SemanticTags.getLocation(locationItem));
        assertEquals(cleaningRobotTagClass, SemanticTags.getEquipment(equipmentItem));
    public void testGetPoint() {
        assertEquals(measurementTagClass, SemanticTags.getPoint(pointItem));
    public void testGetProperty() {
        assertEquals(temperatureTagClass, SemanticTags.getProperty(pointItem));
    public void testAddLocation() {
        String tagName = CUSTOM_LOCATION;
        Class<? extends Tag> customTag = SemanticTags.getById(tagName);
        assertNotNull(customTag);
        GroupItem myItem = new GroupItem("MyLocation");
        myItem.addTag(tagName);
        assertEquals(customTag, SemanticTags.getLocation(myItem));
    public void testAddEquipment() {
        String tagName = CUSTOM_EQUIPMENT;
        GroupItem myItem = new GroupItem("MyEquipment");
        assertEquals(customTag, SemanticTags.getEquipment(myItem));
    public void testAddPoint() {
        String tagName = CUSTOM_POINT;
        GroupItem myItem = new GroupItem("MyItem");
        assertEquals(customTag, SemanticTags.getPoint(myItem));
    public void testAddProperty() {
        String tagName = CUSTOM_PROPERTY;
        assertEquals(customTag, SemanticTags.getProperty(myItem));
