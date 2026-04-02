public class YamlItemConverterTest {
    public void testExpireMetadataConvertedToShortForm() {
        Metadata expireMetadata = new Metadata(new MetadataKey("expire", "item_name"), "10m", Map.of());
        YamlItemDTO dto = convertWithMetadata(expireMetadata, "String");
        assertEquals("10m", dto.expire);
        assertNull(dto.metadata);
    public void testExpireMetadataEmptyStringStaysInShortForm() {
        Metadata expireMetadata = new Metadata(new MetadataKey("expire", "item_name"), "", Map.of());
        assertEquals("", dto.expire);
    public void testExpireMetadataWithConfigStaysMetadata() {
        Metadata expireMetadata = new Metadata(new MetadataKey("expire", "item_name"), "10m", Map.of("command", "OFF"));
        assertNull(dto.expire);
        assertNotNull(dto.metadata);
        assertTrue(dto.metadata.containsKey("expire"));
        assertEquals("10m", dto.metadata.get("expire").getValue());
        assertEquals("OFF", dto.metadata.get("expire").config.get("command"));
    public void testAutoupdateMetadataSetsField() {
        Metadata autoupdate = new Metadata(new MetadataKey("autoupdate", "item_name"), "true", Map.of());
        YamlItemDTO dto = convertWithMetadata(autoupdate, "String");
        assertEquals(Boolean.TRUE, dto.autoupdate);
    public void testAutoupdateMetadataEmptyStringIsTreatedAsNotSet() {
        Metadata autoupdate = new Metadata(new MetadataKey("autoupdate", "item_name"), "", Map.of());
        assertNull(dto.autoupdate);
    public void testUnitMetadataSetsField() {
        Metadata unit = new Metadata(new MetadataKey("unit", "item_name"), "kWh", Map.of());
        YamlItemDTO dto = convertWithMetadata(unit, "Number");
        assertEquals("kWh", dto.unit);
    public void testUnitMetadataEmptyStringStaysInShortForm() {
        Metadata unit = new Metadata(new MetadataKey("unit", "item_name"), "", Map.of());
        assertEquals("", dto.unit);
    private YamlItemDTO convertWithMetadata(Metadata metadata, String itemType) {
        CapturingYamlModelRepository repository = new CapturingYamlModelRepository();
        YamlItemConverter converter = new YamlItemConverter(repository, mock(YamlItemProvider.class),
                mock(YamlMetadataProvider.class), mock(YamlChannelLinkProvider.class),
                mock(ConfigDescriptionRegistry.class));
        Item item = mock(Item.class);
        when(item.getName()).thenReturn(metadata.getUID().getItemName());
        when(item.getLabel()).thenReturn(null);
        when(item.getType()).thenReturn(itemType);
        when(item.getCategory()).thenReturn(null);
        when(item.getGroupNames()).thenReturn(List.of());
        when(item.getTags()).thenReturn(Set.of());
        converter.setItemsToBeSerialized("id", List.of(item), List.of(metadata), Map.of(), false);
        List<YamlElement> elements = repository.getElements();
        assertEquals(1, elements.size());
        assertInstanceOf(YamlItemDTO.class, elements.getFirst());
        return (YamlItemDTO) elements.getFirst();
    private static class CapturingYamlModelRepository implements YamlModelRepository {
        private List<YamlElement> elements = new ArrayList<>();
            this.elements = elements;
        public @Nullable String createIsolatedModel(InputStream inputStream, List<String> errors,
        public List<YamlElement> getElements() {
