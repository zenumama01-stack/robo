 * The {@link SemanticTagDTOMapper} is an utility class to map semantic tags into
 * semantic tag data transfer objects (DTOs).
public class SemanticTagDTOMapper {
     * Maps semantic tag DTO into semantic tag object.
     * @param tagDTO the DTO
     * @return the semantic tag object
    public static @Nullable SemanticTag map(@Nullable SemanticTagDTO tagDTO) {
        if (tagDTO == null) {
            throw new IllegalArgumentException("The argument 'tagDTO' must not be null.");
        if (tagDTO.uid == null) {
            throw new IllegalArgumentException("The argument 'tagDTO.uid' must not be null.");
     * Maps semantic tag object into semantic tag DTO.
     * @param tag the semantic tag
     * @return the semantic tag DTO
    public static SemanticTagDTO map(SemanticTag tag) {
        SemanticTagDTO tagDTO = new SemanticTagDTO();
        tagDTO.uid = tag.getUID();
        tagDTO.label = tag.getLabel();
        tagDTO.description = tag.getDescription();
        tagDTO.synonyms = tag.getSynonyms();
        return tagDTO;
