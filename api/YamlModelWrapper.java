 * The {@link YamlModelWrapper} is used to store the information read from a model in the model cache.
public class YamlModelWrapper {
    private final int version;
    private final boolean readOnly;
    private final Map<String, @Nullable JsonNode> nodes = new ConcurrentHashMap<>();
    public YamlModelWrapper(int version, boolean readOnly) {
    public int getVersion() {
    public Map<String, @Nullable JsonNode> getNodes() {
