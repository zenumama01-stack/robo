 * This interface defines the core features of an openHAB semantic tag.
public interface SemanticTag extends Identifiable<String> {
     * Returns the name of the semantic tag.
     * @return the name of the semantic tag
     * Returns the UID of the parent tag.
     * @return the UID of the parent tag
    String getParentUID();
     * Returns the label of the semantic tag.
     * @return semantic tag label or an empty string if undefined
     * Returns the description of the semantic tag.
     * @return semantic tag description or an empty string if undefined
     * Returns the synonyms of the semantic tag.
     * @return semantic tag synonyms as a List
    List<String> getSynonyms();
     * Returns the localized semantic tag.
     * @param locale the locale to be used
     * @return the localized semantic tag
    SemanticTag localized(Locale locale);
