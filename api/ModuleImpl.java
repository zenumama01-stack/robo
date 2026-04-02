 * Modules are building components of the {@link Rule}s. Each ModuleImpl is
 * identified by id, which is unique in scope of the {@link Rule}. It also has a {@link ModuleType} which provides
 * meta
 * data of the module. The meta data
 * defines {@link Input}s, {@link Output}s and {@link ConfigDescriptionParameter}s parameters of the {@link ModuleImpl}.
 * Setters of the module don't have immediate effect on the Rule. To apply the
 * changes, they should be set on the {@link Rule} and the Rule has to be
 * updated by RuleManager
public abstract class ModuleImpl implements Module {
     * Id of the ModuleImpl. It is mandatory and unique identifier in scope of the {@link Rule}. The id of the
     * {@link ModuleImpl} is used to identify the module
     * in the {@link Rule}.
     * The label is a short, user friendly name of the {@link ModuleImpl} defined by
     * this descriptor.
    private @Nullable String label;
     * The description is a long, user friendly description of the {@link ModuleImpl} defined by this descriptor.
     * Configuration values of the ModuleImpl.
     * @see {@link ConfigDescriptionParameter}.
    private Configuration configuration;
     * Unique type id of this module.
    private String typeUID;
     * Constructor of the module.
     * @param id the module id.
     * @param typeUID unique id of the module type.
     * @param configuration configuration values of the module.
     * @param description the description
    public ModuleImpl(String id, String typeUID, @Nullable Configuration configuration, @Nullable String label,
            @Nullable String description) {
        this.typeUID = typeUID;
        this.configuration = new Configuration(configuration);
     * This method is used for setting the id of the ModuleImpl.
     * @param id of the module.
    public void setId(String id) {
    public String getTypeUID() {
        return typeUID;
     * This method is used for setting the typeUID of the ModuleImpl.
     * @param typeUID of the module.
    public void setTypeUID(String typeUID) {
    public @Nullable String getLabel() {
     * This method is used for setting the label of the ModuleImpl.
     * @param label of the module.
    public void setLabel(String label) {
     * This method is used for setting the description of the ModuleImpl.
     * @param description of the module.
    public void setDescription(String description) {
     * This method is used for setting the configuration of the {@link ModuleImpl}.
     * @param configuration new configuration values.
    public void setConfiguration(Configuration configuration) {
