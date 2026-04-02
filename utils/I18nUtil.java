public class I18nUtil {
    /** The 'text' pattern (prefix) which marks constants. */
    private static final String CONSTANT_PATTERN = "@text/";
    public static boolean isConstant(@Nullable String key) {
        return key != null && key.startsWith(CONSTANT_PATTERN);
    public static String stripConstant(String key) {
        return key.replace(CONSTANT_PATTERN, "");
     * If key is a constant strip the constant part, otherwise use the supplier provided string.
     * @param key the key
     * @param supplier the supplier that return value is used if key is identified as a constant
     * @return the key with the stripped constant marker or the supplier provided key if it is not identified as a
     *         constant
    public static String stripConstantOr(final @Nullable String key, Supplier<String> supplier) {
        if (key != null && key.startsWith(CONSTANT_PATTERN)) {
            return stripConstant(key);
