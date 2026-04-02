 * Defines visibility values of {@link Rule}s, {@link ModuleType}s and {@link Template}s.
public enum Visibility {
     * The UI has always to show an object with such visibility.
    VISIBLE,
     * The UI has always to hide an object with such visibility.
    HIDDEN,
     * The UI has to show an object with such visibility only to experts.
    EXPERT;
     * Tries to parse the specified string value into a {@link Visibility} instance. If the parsing fails, {@code null}
     * is returned.
     * @param value the {@link String} to parse.
     * @return The resulting {@link Visibility} or {@code null}.
    public static @Nullable Visibility typeOf(@Nullable String value) {
        if (value == null || value.isBlank()) {
        String s = value.trim().toUpperCase(Locale.ROOT);
        for (Visibility element : values()) {
            if (s.equals(element.name())) {
