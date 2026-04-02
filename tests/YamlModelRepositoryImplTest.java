import static org.hamcrest.Matchers.contains;
import org.apache.commons.lang3.StringUtils;
import org.junit.jupiter.params.provider.CsvSource;
import org.openhab.core.model.yaml.test.FirstTypeDTO;
import org.openhab.core.model.yaml.test.SecondTypeDTO;
import org.yaml.snakeyaml.Yaml;
 * The {@link YamlModelRepositoryImplTest} contains tests for the {@link YamlModelRepositoryImpl} class.
 * @author Laurent Garnier - Extended tests to cover version 2
 * @author Laurent Garnier - Added one test for version management
public class YamlModelRepositoryImplTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/model");
    private static final Path SOURCE_PATH_ITEMS = Path.of("src/test/resources/model/items");
    private static final String MODEL_NAME = "model.yaml";
    private static final Path MODEL_PATH = Path.of(MODEL_NAME);
    private @TempDir @NonNullByDefault({}) Path watchPath;
    private @NonNullByDefault({}) Path fullModelPath;
    private @Mock @NonNullByDefault({}) YamlModelListener<@NonNull FirstTypeDTO> firstTypeListener;
    private @Mock @NonNullByDefault({}) YamlModelListener<@NonNull SecondTypeDTO> secondTypeListener1;
    private @Mock @NonNullByDefault({}) YamlModelListener<@NonNull SecondTypeDTO> secondTypeListener2;
    private @Captor @NonNullByDefault({}) ArgumentCaptor<Collection<FirstTypeDTO>> firstTypeCaptor;
    private @Captor @NonNullByDefault({}) ArgumentCaptor<Collection<SecondTypeDTO>> secondTypeCaptor1;
    private @Captor @NonNullByDefault({}) ArgumentCaptor<Collection<SecondTypeDTO>> secondTypeCaptor2;
        fullModelPath = watchPath.resolve(MODEL_PATH);
        when(watchServiceMock.getWatchPath()).thenReturn(watchPath);
        when(firstTypeListener.getElementClass()).thenReturn(FirstTypeDTO.class);
        when(firstTypeListener.isVersionSupported(anyInt())).thenReturn(true);
        when(firstTypeListener.isDeprecated()).thenReturn(false);
        when(secondTypeListener1.getElementClass()).thenReturn(SecondTypeDTO.class);
        when(secondTypeListener1.isVersionSupported(anyInt())).thenReturn(true);
        when(secondTypeListener1.isDeprecated()).thenReturn(false);
        when(secondTypeListener2.getElementClass()).thenReturn(SecondTypeDTO.class);
        when(secondTypeListener2.isVersionSupported(anyInt())).thenReturn(true);
        when(secondTypeListener2.isDeprecated()).thenReturn(false);
    public void testFileAddedAfterListeners() throws IOException {
        Files.copy(SOURCE_PATH.resolve("modelFileAddedOrRemoved.yaml"), fullModelPath);
        YamlModelRepositoryImpl modelRepository = new YamlModelRepositoryImpl(watchServiceMock);
        modelRepository.addYamlModelListener(firstTypeListener);
        modelRepository.addYamlModelListener(secondTypeListener1);
        modelRepository.addYamlModelListener(secondTypeListener2);
        modelRepository.processWatchEvent(WatchService.Kind.CREATE, fullModelPath);
        verify(firstTypeListener).addedModel(eq(MODEL_NAME), firstTypeCaptor.capture());
        verify(firstTypeListener, never()).updatedModel(any(), any());
        verify(firstTypeListener, never()).removedModel(any(), any());
        verify(secondTypeListener1).addedModel(eq(MODEL_NAME), secondTypeCaptor1.capture());
        verify(secondTypeListener1, never()).updatedModel(any(), any());
        verify(secondTypeListener1, never()).removedModel(any(), any());
        verify(secondTypeListener2).addedModel(eq(MODEL_NAME), secondTypeCaptor2.capture());
        verify(secondTypeListener2, never()).updatedModel(any(), any());
        verify(secondTypeListener2, never()).removedModel(any(), any());
        List<Collection<FirstTypeDTO>> firstTypeCaptorValues = firstTypeCaptor.getAllValues();
        assertThat(firstTypeCaptorValues, hasSize(1));
        List<Collection<SecondTypeDTO>> secondTypeCaptor1Values = secondTypeCaptor1.getAllValues();
        assertThat(secondTypeCaptor1Values, hasSize(1));
        List<Collection<SecondTypeDTO>> secondTypeCaptor2Values = secondTypeCaptor2.getAllValues();
        assertThat(secondTypeCaptor2Values, hasSize(1));
        Collection<FirstTypeDTO> firstTypeElements = firstTypeCaptorValues.getFirst();
        Collection<SecondTypeDTO> secondTypeElements1 = secondTypeCaptor1Values.getFirst();
        Collection<SecondTypeDTO> secondTypeElements2 = secondTypeCaptor2Values.getFirst();
        assertThat(firstTypeElements, hasSize(2));
        assertThat(firstTypeElements,
                containsInAnyOrder(new FirstTypeDTO("First1", "Description1"), new FirstTypeDTO("First2", null)));
        assertThat(secondTypeElements1, hasSize(1));
        assertThat(secondTypeElements1, contains(new SecondTypeDTO("Second1", "Label1")));
        assertThat(secondTypeElements2, hasSize(1));
        assertThat(secondTypeElements2, contains(new SecondTypeDTO("Second1", "Label1")));
    public void testFileAddedBeforeListeners() throws IOException {
    public void testFileUpdated() throws IOException {
        Files.copy(SOURCE_PATH.resolve("modelFileUpdatePost.yaml"), fullModelPath);
        verify(firstTypeListener).addedModel(eq(MODEL_NAME), any());
        Files.copy(SOURCE_PATH.resolve("modelFileUpdatePre.yaml"), fullModelPath, StandardCopyOption.REPLACE_EXISTING);
        modelRepository.processWatchEvent(WatchService.Kind.MODIFY, fullModelPath);
        verify(firstTypeListener, times(2)).addedModel(eq(MODEL_NAME), firstTypeCaptor.capture());
        verify(firstTypeListener).updatedModel(eq(MODEL_NAME), firstTypeCaptor.capture());
        verify(firstTypeListener).removedModel(eq(MODEL_NAME), firstTypeCaptor.capture());
        assertThat(firstTypeCaptorValues, hasSize(4));
        // added originally
        assertThat(firstTypeCaptorValues.getFirst(), hasSize(3));
        assertThat(firstTypeCaptorValues.getFirst(), containsInAnyOrder(new FirstTypeDTO("First", "First original"),
                new FirstTypeDTO("Second", "Second original"), new FirstTypeDTO("Third", "Third original")));
        // added by update
        assertThat(firstTypeCaptorValues.get(1), hasSize(1));
        assertThat(firstTypeCaptorValues.get(1), contains(new FirstTypeDTO("Fourth", "Fourth original")));
        // updated by update
        assertThat(firstTypeCaptorValues.get(2), hasSize(1));
        assertThat(firstTypeCaptorValues.get(2), contains(new FirstTypeDTO("Second", "Second modified")));
        // removed by update
        assertThat(firstTypeCaptorValues.get(3), hasSize(1));
        assertThat(firstTypeCaptorValues.get(3), contains(new FirstTypeDTO("Third", "Third original")));
    @CsvSource({ //
            "modelFileUpdateRemovedElements.yaml", "modelFileUpdateRenamedElements.yaml",
            "modelFileUpdateRemovedVersion.yaml" //
    public void testFileRemovedElements(String file) throws IOException {
        Files.copy(SOURCE_PATH.resolve(file), fullModelPath, StandardCopyOption.REPLACE_EXISTING);
        Collection<FirstTypeDTO> firstTypeElements = firstTypeCaptor.getAllValues().getFirst();
        // Check that the elements were removed
        assertThat(firstTypeElements, hasSize(3));
        assertThat(firstTypeElements, containsInAnyOrder(new FirstTypeDTO("First", "First original"),
    public void testFileRemoved() throws IOException {
        modelRepository.processWatchEvent(WatchService.Kind.DELETE, fullModelPath);
        assertThat(firstTypeCaptorValues, hasSize(2));
        // all are added
        // all are removed
        assertThat(firstTypeCaptorValues.get(1), hasSize(3));
        assertThat(firstTypeCaptorValues.get(1), containsInAnyOrder(new FirstTypeDTO("First", "First original"),
    public void testAddElementToModel() throws IOException {
        Files.copy(SOURCE_PATH.resolve("modifyModelInitialContent.yaml"), fullModelPath);
        FirstTypeDTO added = new FirstTypeDTO("element3", "description3");
        modelRepository.addElementToModel(MODEL_NAME, added);
        SecondTypeDTO added2 = new SecondTypeDTO("elt1", "My label");
        modelRepository.addElementToModel(MODEL_NAME, added2);
        String actualFileContent = Files.readString(fullModelPath);
        String expectedFileContent = Files.readString(SOURCE_PATH.resolve("addToModelExpectedContent.yaml"));
        Yaml yaml = new Yaml();
        assertThat(yaml.load(actualFileContent), equalTo(yaml.load(expectedFileContent)));
        assertThat(firstTypeCaptorValues.get(1), contains(new FirstTypeDTO("element3", "description3")));
        assertThat(secondTypeCaptor1Values.getFirst(), hasSize(1));
        assertThat(secondTypeCaptor1Values.getFirst(), contains(new SecondTypeDTO("elt1", "My label")));
    public void testUpdateElementInModel() throws IOException {
        FirstTypeDTO updated = new FirstTypeDTO("element1", "newDescription1");
        modelRepository.updateElementInModel(MODEL_NAME, updated);
        String expectedFileContent = Files.readString(SOURCE_PATH.resolve("updateInModelExpectedContent.yaml"));
        assertThat(firstTypeCaptorValues.getFirst(), hasSize(1));
        assertThat(firstTypeCaptorValues.getFirst(), contains(new FirstTypeDTO("element1", "newDescription1")));
    public void testRemoveElementFromModel() throws IOException {
        FirstTypeDTO removed = new FirstTypeDTO("element1", "description1");
        modelRepository.removeElementFromModel(MODEL_NAME, removed);
        String expectedFileContent = Files.readString(SOURCE_PATH.resolve("removeFromModelExpectedContent.yaml"));
        assertThat(firstTypeCaptorValues.getFirst(), contains(new FirstTypeDTO("element1", "description1")));
    public void testReadOnlyModelNotUpdated() throws IOException {
        FirstTypeDTO updated = new FirstTypeDTO("element2", "newDescription2");
        String expectedFileContent = Files.readString(SOURCE_PATH.resolve("modelFileAddedOrRemoved.yaml"));
    public void testExistingProviderForVersion() throws IOException {
        // Provider supports version 1
        when(firstTypeListener.isVersionSupported(eq(1))).thenReturn(true);
        Collection<FirstTypeDTO> firstTypeElements = firstTypeCaptor.getValue();
    public void testExistingDeprecatedProviderForVersion() throws IOException {
        // Provider supports version 1 as deprecated
        when(firstTypeListener.isDeprecated()).thenReturn(true);
    public void testNoProviderForVersion() throws IOException {
        // Provider does not support version 1
        when(firstTypeListener.isVersionSupported(eq(1))).thenReturn(false);
        verify(firstTypeListener, never()).addedModel(any(), any());
    public void testDifferentProvidersDependingOnVersion() throws IOException {
        // secondTypeListener1 supports version 1
        when(secondTypeListener1.isVersionSupported(eq(1))).thenReturn(true);
        // secondTypeListener2 does not supports version 1
        when(secondTypeListener2.isVersionSupported(eq(1))).thenReturn(false);
        verify(secondTypeListener2, never()).addedModel(any(), any());
        Collection<SecondTypeDTO> secondTypeElements = secondTypeCaptor1.getValue();
        assertThat(secondTypeElements, hasSize(1));
        assertThat(secondTypeElements, contains(new SecondTypeDTO("Second1", "Label1")));
    public void testObjectFormMetadataLoadingAndGeneration() throws IOException {
        Files.copy(SOURCE_PATH_ITEMS.resolve("itemWithObjectFormMetadata.yaml"), fullModelPath);
        YamlModelListener<YamlItemDTO> itemListener = mock(YamlModelListener.class);
        when(itemListener.getElementClass()).thenReturn(YamlItemDTO.class);
        when(itemListener.isVersionSupported(anyInt())).thenReturn(true);
        when(itemListener.isDeprecated()).thenReturn(false);
        modelRepository.addYamlModelListener(itemListener);
        // Verify the listener was called
        ArgumentCaptor<Collection<YamlItemDTO>> captor = ArgumentCaptor.forClass(Collection.class);
        verify(itemListener).addedModel(eq(MODEL_NAME), captor.capture());
        // Verify that the valid item with object-form metadata was loaded
        Collection<YamlItemDTO> items = captor.getValue();
        assertThat(items, hasSize(1));
        YamlItemDTO item = items.iterator().next();
        assertThat(item.name, is("ValidItem"));
        assertThat(item.metadata, is(notNullValue()));
        assertThat(item.metadata.keySet(), containsInAnyOrder("alexa", "homekit"));
        assertThat(item.metadata.get("alexa").value, is("Switchable"));
        assertThat(item.metadata.get("alexa").config, is(Map.of("setting1", "value1")));
        assertThat(item.metadata.get("homekit").value, is("Lighting"));
        assertThat(item.metadata.get("homekit").config, is(nullValue()));
        // Verify YAML output contains object form metadata
        modelRepository.addElementsToBeGenerated("items", List.copyOf(items));
        ByteArrayOutputStream out = new ByteArrayOutputStream();
        modelRepository.generateFileFormat("items", out);
        String yaml = out.toString();
        // Should contain object form metadata for 'alexa'
        assertThat(yaml, containsString("alexa:"));
        assertThat(yaml, containsString("value: Switchable"));
        assertThat(yaml, containsOnlyOnce("config:"));
        assertThat(yaml, containsString("setting1: value1"));
    public void testShortFormMetadataLoadingAndGeneration() throws IOException {
        Files.copy(SOURCE_PATH_ITEMS.resolve("itemWithShortFormMetadata.yaml"), fullModelPath);
        // Verify items were loaded with short-form metadata
        // deserializer converts short-form to value field
        assertThat(item.metadata.get("alexa").config, is(nullValue()));
        // Verify YAML output contains short form metadata
        assertThat(yaml, containsString("alexa: Switchable"));
        assertThat(yaml, containsString("homekit: Lighting"));
        // Should not contain object form keys
        assertThat(yaml, not(containsString("value:")));
        assertThat(yaml, not(containsString("config:")));
    public void testMixedFormMetadataLoadingAndGeneration() throws IOException {
        Files.copy(SOURCE_PATH_ITEMS.resolve("itemWithMixedFormMetadata.yaml"), fullModelPath);
        // Verify that the valid item with mixed-form metadata was loaded
        assertThat(item.metadata.keySet(), containsInAnyOrder("alexa", "homekit", "matter"));
        assertThat(item.metadata.get("matter").value, is("OnOffLight"));
        assertThat(item.metadata.get("matter").config, is(nullValue()));
        // Verify YAML output contains metadata in both forms
        // Should contain short form metadata for 'homekit' and 'matter'
        assertThat(yaml, containsString("matter: OnOffLight"));
    private static Matcher<String> containsOnlyOnce(String substring) {
        return new TypeSafeMatcher<String>() {
            protected boolean matchesSafely(String item) {
                return StringUtils.countMatches(item, substring) == 1;
            public void describeTo(Description description) {
                description.appendText("string appearing exactly once: ").appendValue(substring);
            protected void describeMismatchSafely(String item, Description mismatchDescription) {
                int count = StringUtils.countMatches(item, substring);
                mismatchDescription.appendText("was found ").appendValue(count).appendText(" times");
