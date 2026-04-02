 * The {@link YamlModelRepository} defines methods to update elements in a YAML model.
 * @author Laurent Garnier - Added methods addElementsToBeGenerated, generateFileFormat, createIsolatedModel and
 *         removeIsolatedModel
public interface YamlModelRepository {
    void addElementToModel(String modelName, YamlElement element);
    void removeElementFromModel(String modelName, YamlElement element);
    void updateElementInModel(String modelName, YamlElement element);
     * Associate a list of elements to be generated to an identifier.
     * @param id the identifier of the file format generation
     * @param elements the elements to be added
    void addElementsToBeGenerated(String id, List<YamlElement> elements);
     * Generate the YAML file format for all elements that were associated to the provided identifier.
     * @param out the output stream to write to
    void generateFileFormat(String id, OutputStream out);
    String createIsolatedModel(InputStream inputStream, List<String> errors, List<String> warnings);
     * Removes an isolated model from the repository
     * @param modelName the name of the model to be removed
    void removeIsolatedModel(String modelName);
