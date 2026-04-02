 * Wrapper class to collect information about actions modules to be created
public class ModuleInformation {
    private String description;
    private Visibility visibility;
    private Set<String> tags;
    private List<Input> inputs;
    private List<Output> outputs;
    private String configName;
    private String thingUID;
    public ModuleInformation(String uid, Object actionProvider, Method m) {
        this.method = m;
    public Method getMethod() {
    public Object getActionProvider() {
        return actionProvider;
    public void setVisibility(Visibility visibility) {
        this.visibility = visibility;
    public List<Input> getInputs() {
    public void setInputs(List<Input> inputs) {
        this.inputs = inputs;
    public List<Output> getOutputs() {
    public void setOutputs(List<Output> outputs) {
    public String getConfigName() {
    public void setTags(Set<String> tags) {
        this.tags = tags;
    public void setConfigName(String configName) {
        this.configName = configName;
    public String getThingUID() {
        return thingUID;
    public void setThingUID(String thingUID) {
        this.thingUID = thingUID;
        result = prime * result + ((actionProvider == null) ? 0 : actionProvider.hashCode());
        result = prime * result + ((configName == null) ? 0 : configName.hashCode());
        result = prime * result + ((label == null) ? 0 : label.hashCode());
        result = prime * result + ((method == null) ? 0 : method.hashCode());
        result = prime * result + ((thingUID == null) ? 0 : thingUID.hashCode());
        result = prime * result + ((uid == null) ? 0 : uid.hashCode());
        result = prime * result + ((visibility == null) ? 0 : visibility.hashCode());
    public boolean equals(Object obj) {
        ModuleInformation other = (ModuleInformation) obj;
        if (actionProvider == null) {
            if (other.actionProvider != null) {
        } else if (!actionProvider.equals(other.actionProvider)) {
        if (configName == null) {
            if (other.configName != null) {
        } else if (!configName.equals(other.configName)) {
        if (label == null) {
            if (other.label != null) {
        } else if (!label.equals(other.label)) {
        if (method == null) {
            if (other.method != null) {
        } else if (!method.equals(other.method)) {
            if (other.thingUID != null) {
        } else if (!thingUID.equals(other.thingUID)) {
            if (other.uid != null) {
        } else if (!uid.equals(other.uid)) {
        if (visibility != other.visibility) {
