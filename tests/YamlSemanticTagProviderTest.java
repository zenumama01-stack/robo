 * The {@link YamlSemanticTagProviderTest} contains tests for the {@link YamlSemanticTagProvider} class.
public class YamlSemanticTagProviderTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/model/semantics");
    private @NonNullByDefault({}) YamlModelRepositoryImpl modelRepository;
    private @NonNullByDefault({}) YamlSemanticTagProvider semanticTagProvider;
    private @NonNullByDefault({}) TestSemanticTagChangeListener semanticTagListener;
    @SuppressWarnings({ "null", "unchecked" })
        semanticTagListener = new TestSemanticTagChangeListener();
        semanticTagProvider = new YamlSemanticTagProvider();
        semanticTagProvider.addProviderChangeListener(semanticTagListener);
        modelRepository = new YamlModelRepositoryImpl(watchServiceMock);
        modelRepository.addYamlModelListener(semanticTagProvider);
    public void testShortFormTagsLoadingAndGeneration() throws IOException {
        processYamlResource("tagWithShortFormSyntax.yaml");
        assertThat(semanticTagListener.semanticTags, is(aMapWithSize(1)));
        assertThat(semanticTagListener.semanticTags, hasKey("Tag_uid"));
        Collection<SemanticTag> tags = semanticTagProvider.getAll();
        assertThat(tags, hasSize(1));
        SemanticTag tag = tags.iterator().next();
        assertThat(tag.getUID(), is("Tag_uid"));
        assertThat(tag.getName(), is("uid"));
        assertThat(tag.getParentUID(), is("Tag"));
        assertThat(tag.getLabel(), is("TagLabel"));
        assertThat(tag.getDescription(), is(""));
        assertThat(tag.getSynonyms(), hasSize(0));
        // Verify YAML output contains short form
        String outYaml = generateYamlFromTags(tags);
        assertThat(outYaml, containsString("Tag_uid: TagLabel"));
    public void testMapFormTagsLoadingAndGeneration() throws IOException {
        processYamlResource("tagWithMapForm.yaml");
        assertThat(tag.getDescription(), is("Some description"));
        assertThat(tag.getSynonyms(), contains("syn1", "syn2"));
        // Verify YAML output contains map form
        assertThat(outYaml, containsString("label: TagLabel"));
        assertThat(outYaml, containsString("description: Some description"));
        assertThat(outYaml, containsString("syn1"));
        assertThat(outYaml, containsString("syn2"));
    public void testShortFormTagWithNullLabelDeserializesAndSerializes() throws IOException {
        processYamlResource("tagWithShortFormSyntaxWithNullLabel.yaml");
        assertThat(semanticTagListener.semanticTags, hasKey("Tag_null"));
        // DTO currently represents an explicit null label as null
        assertThat(tag.getUID(), is("Tag_null"));
        assertThat(tag.getName(), is("null"));
        assertThat(tag.getLabel(), is(""));
        // Generation should contain the tag key but should not emit label/description/synonyms
        assertThat(outYaml, containsString("Tag_null:"));
        assertThat(outYaml, not(containsString("label:")));
        assertThat(outYaml, not(containsString("description:")));
        assertThat(outYaml, not(containsString("synonyms:")));
    private void processYamlResource(String resourceName) throws IOException {
        Files.copy(SOURCE_PATH.resolve(resourceName), fullModelPath);
    private String generateYamlFromTags(Collection<SemanticTag> tags) {
        List<YamlElement> elts = new ArrayList<>();
        tags.forEach(tag -> {
            dto.uid = tag.getUID();
            dto.label = tag.getLabel();
            dto.description = tag.getDescription();
            dto.synonyms = tag.getSynonyms();
            elts.add(dto);
        modelRepository.addElementsToBeGenerated("tags", elts);
        modelRepository.generateFileFormat("tags", out);
        return out.toString();
    private static class TestSemanticTagChangeListener implements ProviderChangeListener<SemanticTag> {
        public final Map<String, SemanticTag> semanticTags = new HashMap<>();
        public void added(Provider<SemanticTag> provider, SemanticTag element) {
            semanticTags.put(element.getUID(), element);
        public void removed(Provider<SemanticTag> provider, SemanticTag element) {
            semanticTags.remove(element.getUID());
        public void updated(Provider<SemanticTag> provider, SemanticTag oldelement, SemanticTag element) {
