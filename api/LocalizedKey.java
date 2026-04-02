 * Common class (used as key) for i18n to store a localized object in a cache.
public class LocalizedKey {
    final Object key;
    final @Nullable String locale;
    public LocalizedKey(Object id, @Nullable String locale) {
        this.key = id;
    public Object getKey() {
    public @Nullable String getLocale() {
        result = prime * result + key.hashCode();
        result = prime * result + (locale instanceof String string ? string.hashCode() : 0);
        LocalizedKey other = (LocalizedKey) obj;
        if (!Objects.equals(key, other.key)) {
        return Objects.equals(locale, other.locale);
