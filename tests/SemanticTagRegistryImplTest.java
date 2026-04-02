public class SemanticTagRegistryImplTest {
    private @NonNullByDefault({}) SemanticTagRegistry semanticTagRegistry;
    private @NonNullByDefault({}) SemanticTag userLocationTag;
    private @NonNullByDefault({}) SemanticTag userSubLocationTag;
        userLocationTag = new SemanticTagImpl("Location_UserLocation", "Custom label", "Custom description",
                " Synonym1, Synonym2 , Synonym With Space ");
        userSubLocationTag = new SemanticTagImpl("Location_UserLocation_UserSubLocation", null, null, List.of());
        when(managedSemanticTagProviderMock.getAll()).thenReturn(List.of(userLocationTag, userSubLocationTag));
        semanticTagRegistry = new SemanticTagRegistryImpl(new DefaultSemanticTagProvider(),
        roomTagClass = semanticTagRegistry.getTagClassById("Location_Indoor_Room");
        bathroomTagClass = semanticTagRegistry.getTagClassById("Location_Indoor_Room_Bathroom");
        cleaningRobotTagClass = semanticTagRegistry.getTagClassById("Equipment_CleaningRobot");
        measurementTagClass = semanticTagRegistry.getTagClassById("Point_Measurement");
        temperatureTagClass = semanticTagRegistry.getTagClassById("Property_Temperature");
    public void testGetById() {
        assertEquals(Location.class, semanticTagRegistry.getTagClassById("Location"));
        assertEquals(roomTagClass, semanticTagRegistry.getTagClassById("Room"));
        assertEquals(roomTagClass, semanticTagRegistry.getTagClassById("Indoor_Room"));
        assertEquals(roomTagClass, semanticTagRegistry.getTagClassById("Location_Indoor_Room"));
        assertEquals(bathroomTagClass, semanticTagRegistry.getTagClassById("Bathroom"));
        assertEquals(bathroomTagClass, semanticTagRegistry.getTagClassById("Room_Bathroom"));
        assertEquals(bathroomTagClass, semanticTagRegistry.getTagClassById("Indoor_Room_Bathroom"));
        assertEquals(bathroomTagClass, semanticTagRegistry.getTagClassById("Location_Indoor_Room_Bathroom"));
    public void testBuildId() {
        assertEquals("Location", SemanticTagRegistryImpl.buildId(Location.class));
        assertEquals("Location_Indoor_Room", SemanticTagRegistryImpl.buildId(roomTagClass));
        assertEquals("Location_Indoor_Room_Bathroom", SemanticTagRegistryImpl.buildId(bathroomTagClass));
        assertEquals("Equipment", SemanticTagRegistryImpl.buildId(Equipment.class));
        assertEquals("Equipment_CleaningRobot", SemanticTagRegistryImpl.buildId(cleaningRobotTagClass));
        assertEquals("Point", SemanticTagRegistryImpl.buildId(Point.class));
        assertEquals("Point_Measurement", SemanticTagRegistryImpl.buildId(measurementTagClass));
        assertEquals("Property", SemanticTagRegistryImpl.buildId(Property.class));
        assertEquals("Property_Temperature", SemanticTagRegistryImpl.buildId(temperatureTagClass));
    public void testIsEditable() {
        when(managedSemanticTagProviderMock.get(eq("Location"))).thenReturn(null);
        when(managedSemanticTagProviderMock.get(eq("Location_Indoor"))).thenReturn(null);
        when(managedSemanticTagProviderMock.get(eq("Location_Indoor_Room"))).thenReturn(null);
        when(managedSemanticTagProviderMock.get(eq("Location_Indoor_Room_Bathroom"))).thenReturn(null);
        when(managedSemanticTagProviderMock.get(eq("Location_UserLocation"))).thenReturn(userLocationTag);
        when(managedSemanticTagProviderMock.get(eq("Location_UserLocation_UserSubLocation")))
                .thenReturn(userSubLocationTag);
        assertFalse(semanticTagRegistry.isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location"))));
        assertFalse(semanticTagRegistry.isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location_Indoor"))));
        assertFalse(semanticTagRegistry
                .isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location_Indoor_Room"))));
                .isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location_Indoor_Room_Bathroom"))));
        assertTrue(semanticTagRegistry
                .isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location_UserLocation"))));
                .isEditable(Objects.requireNonNull(semanticTagRegistry.get("Location_UserLocation_UserSubLocation"))));
