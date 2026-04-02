 * The {@link ConfigDescriptionBuilder} class provides a builder for the {@link ConfigDescription} class.
public class ConfigDescriptionBuilder {
    private List<ConfigDescriptionParameter> parameters = new ArrayList<>();
    private List<ConfigDescriptionParameterGroup> parameterGroups = new ArrayList<>();
    private ConfigDescriptionBuilder(URI uri) {
     * Creates a config description builder
     * @param @param uri the URI of this description within the {@link ConfigDescriptionRegistry}
     * @return the config description builder instance
    public static ConfigDescriptionBuilder create(URI uri) {
        return new ConfigDescriptionBuilder(uri);
     * Adds a {@link ConfigDescriptionParameter}s.
     * @return the updated builder instance
    public ConfigDescriptionBuilder withParameter(ConfigDescriptionParameter parameter) {
        parameters.add(parameter);
     * Adds a list of {@link ConfigDescriptionParameter}s.
    public ConfigDescriptionBuilder withParameters(List<ConfigDescriptionParameter> parameters) {
     * Adds a {@link ConfigDescriptionParameterGroup} associated with the {@link ConfigDescriptionParameter}s.
    public ConfigDescriptionBuilder withParameterGroup(ConfigDescriptionParameterGroup parameterGroup) {
        parameterGroups.add(parameterGroup);
     * Adds a list of {@link ConfigDescriptionParameterGroup} associated with the {@link ConfigDescriptionParameter}s.
    public ConfigDescriptionBuilder withParameterGroups(List<ConfigDescriptionParameterGroup> parameterGroups) {
        this.parameterGroups = parameterGroups;
     * Builds a {@link ConfigDescription} with the settings of this builder.
     * @return the desired result
     * @throws IllegalArgumentException if the URI is invalid
    public ConfigDescription build() throws IllegalArgumentException {
        return new ConfigDescription(uri, parameters, parameterGroups);
