package org.openhab.core.config.core;
 * The {@link ConfigDescription} class contains a description for a concrete
 * configuration of e.g. a {@code Thing}, a {@code Bridge} or other specific
 * configurable services. This class <i>does not</i> contain the configuration
 * data itself and is usually used for data validation of the concrete
 * configuration or for supporting user interfaces.
 * The {@link ConfigDescriptionParameterGroup} provides a method to group parameters to allow the UI to better display
 * the parameter information. This can be left blank for small devices where there are only a few parameters, however
 * devices with larger numbers of parameters can set the group member in the {@link ConfigDescriptionParameter} and then
 * provide group information as part of the {@link ConfigDescription} class.
 * The description is stored within the {@link ConfigDescriptionRegistry} under the given URI. The URI has to follow the
 * syntax {@code '<scheme>:<token>[:<token>]'} (e.g. {@code "binding:hue:bridge"}).
 * <b>Hint:</b> This class is immutable.
 * @author Chris Jackson - Added parameter groups
 * @author Thomas Höfer - Added convenient operation to get config description parameters in a map
public class ConfigDescription implements Identifiable<URI> {
    private final URI uri;
    private final List<ConfigDescriptionParameter> parameters;
    private final List<ConfigDescriptionParameterGroup> parameterGroups;
     * Creates a new instance of this class with the specified parameters.
     * @param uri the URI of this description within the {@link ConfigDescriptionRegistry}
     * @param parameters the list of configuration parameters that belong to the given URI
     * @param groups the list of groups associated with the parameters
     * @throws IllegalArgumentException if the URI is null or invalid
     * @deprecated Use the {@link ConfigDescriptionBuilder} instead.
    ConfigDescription(URI uri, @Nullable List<ConfigDescriptionParameter> parameters,
            @Nullable List<ConfigDescriptionParameterGroup> groups) {
        if (uri == null) {
            throw new IllegalArgumentException("The URI must not be null!");
        if (!uri.isAbsolute()) {
            throw new IllegalArgumentException("The scheme is missing!");
        if (!uri.isOpaque()) {
            throw new IllegalArgumentException("The scheme specific part (token) must not start with a slash ('/')!");
        this.uri = uri;
        this.parameters = parameters == null ? List.of() : Collections.unmodifiableList(parameters);
        this.parameterGroups = groups == null ? List.of() : Collections.unmodifiableList(groups);
     * Returns the URI of this description within the {@link ConfigDescriptionRegistry}.
     * The URI follows the syntax {@code '<scheme>:<token>[:<token>]'} (e.g. {@code "binding:hue:bridge"}).
     * @return the URI of this description
    public URI getUID() {
     * Returns the corresponding {@link ConfigDescriptionParameter}s.
     * The returned list is immutable.
     * @return the corresponding configuration description parameters (could be empty)
    public List<ConfigDescriptionParameter> getParameters() {
     * Returns a map representation of the {@link ConfigDescriptionParameter}s. The map will use the name of the
     * parameter as key and the parameter as value.
     * @return the unmodifiable map of configuration description parameters which uses the name as key and the parameter
     *         as value (could be empty)
    public Map<String, ConfigDescriptionParameter> toParametersMap() {
        Map<String, ConfigDescriptionParameter> map = new HashMap<>();
        for (ConfigDescriptionParameter parameter : parameters) {
            map.put(parameter.getName(), parameter);
        return Collections.unmodifiableMap(map);
     * Returns the list of configuration parameter groups associated with the parameters.
     * @return the list of parameter groups parameter (could be empty)
    public List<ConfigDescriptionParameterGroup> getParameterGroups() {
        return parameterGroups;
        return "ConfigDescription [uri=" + uri + ", parameters=" + parameters + ", groups=" + parameterGroups + "]";
