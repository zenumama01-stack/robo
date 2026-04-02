 * The {@link ConfigDescriptionParameterGroupBuilder} class provides a builder for the
 * {@link ConfigDescriptionParameterGroup} class.
public class ConfigDescriptionParameterGroupBuilder {
    private @Nullable Boolean advanced;
    private ConfigDescriptionParameterGroupBuilder(String name) {
     * Creates a parameter group builder
     * @return parameter group builder
    public static ConfigDescriptionParameterGroupBuilder create(String name) {
        return new ConfigDescriptionParameterGroupBuilder(name);
     * Set. the context of the group.
     * @param context group context as a string
    public ConfigDescriptionParameterGroupBuilder withContext(@Nullable String context) {
     * Sets the advanced flag for this group.
     * @param advanced true if the group contains advanced properties
    public ConfigDescriptionParameterGroupBuilder withAdvanced(@Nullable Boolean advanced) {
     * Sets the human readable label of the group.
     * @param label as a string
    public ConfigDescriptionParameterGroupBuilder withLabel(@Nullable String label) {
     * Sets the human readable description of the parameter group.
     * @param description as a string
    public ConfigDescriptionParameterGroupBuilder withDescription(@Nullable String description) {
    public ConfigDescriptionParameterGroup build() throws IllegalArgumentException {
        return new ConfigDescriptionParameterGroup(name, context, advanced != null && advanced, label, description);
