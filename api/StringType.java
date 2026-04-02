public class StringType implements PrimitiveType, State, Command {
    public static final StringType EMPTY = new StringType();
    public StringType() {
    public StringType(@Nullable String value) {
        this.value = value != null ? value : "";
    public static StringType valueOf(@Nullable String value) {
        return new StringType(value);
        return value.hashCode();
        if (obj instanceof String) {
            return obj.equals(value);
        StringType other = (StringType) obj;
        return Objects.equals(this.value, other.value);
