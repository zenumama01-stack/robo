 * This class defines connection between {@link Input} of the current {@link Module} and {@link Output} of the external
 * one. The current module is the module containing {@link Connection} instance and the external one is the module where
 * the current is connected to. <br>
 * The input of the current module is defined by name of the {@link Input}. The {@link Output} of the external module is
 * defined by id of the module and name of the output.
 * @author Ana Dimova - new reference syntax: list[index], map["key"], bean.field
public final class Connection {
    private final @Nullable String outputModuleId;
    private final @Nullable String outputName;
    private final @Nullable String reference;
    private final String inputName;
     * This constructor is responsible for creation of connections between modules in the rule.
     * @param inputName is a unique id of the {@code Input} in scope of the {@link Module}.
     * @param reference the reference tokens of this connection
    public Connection(String inputName, String reference) {
        this(inputName, null, null, Objects.requireNonNull(reference, "Configuration Reference can't be null."));
     * @param inputName is a unique name of the {@code Input} in scope of the {@link Module}.
     * @param outputModuleId is a unique id of the {@code Module} in scope of the {@link Rule}.
     * @param outputName is a unique name of the {@code Output} in scope of the {@link Module}.
    public Connection(String inputName, @Nullable String outputModuleId, @Nullable String outputName,
            @Nullable String reference) {
        this.inputName = Objects.requireNonNull(inputName, "Input name can't be null.");
        if (inputName.isEmpty()) {
            throw new IllegalArgumentException("Invalid name for Input.");
        this.outputName = outputName;
        this.outputModuleId = outputModuleId;
        this.reference = reference;
     * Gets the identifier of external {@link Module} of this connection.
     * @return id of external {@link Module}
    public @Nullable String getOutputModuleId() {
        return outputModuleId;
     * Gets the output name of external {@link Module} of this connection.
     * @return name of {@link Output} of external {@link Module}.
    public @Nullable String getOutputName() {
        return outputName;
     * Gets input name of current {@link Module} of this connection.
     * @return name {@link Input} of the current {@link Module}
    public String getInputName() {
        return inputName;
     * Gets the reference tokens of this connection.
     * @return the reference tokens.
    public @Nullable String getReference() {
        return reference;
        sb.append(this.getClass().getSimpleName());
        sb.append("[");
        if (outputModuleId != null) {
            sb.append(outputModuleId);
            sb.append(".");
            sb.append(outputName);
        if (reference != null) {
            sb.append("(");
            sb.append(reference);
            sb.append(")");
        sb.append("->");
        sb.append(inputName);
        sb.append("]");
        return sb.toString();
        result = prime * result + toString().hashCode();
     * Compares two connection objects. Two connections are equal if they own equal {@code inputName},
     * {@code outputModuleId}, {@code outputName} and {@code reference}.
     * @return {@code true} when own equal {@code inputName}, {@code outputModuleId}, {@code outputName} and
     *         {@code reference} and {@code false} in the opposite.
        Connection other = (Connection) obj;
        if (!inputName.equals(other.inputName)) {
        if (outputModuleId == null) {
            if (other.outputModuleId != null) {
        } else if (!outputModuleId.equals(other.outputModuleId)) {
        if (outputName == null) {
            if (other.outputName != null) {
        } else if (!outputName.equals(other.outputName)) {
        if (reference == null) {
            if (other.reference != null) {
        } else if (!reference.equals(other.reference)) {
