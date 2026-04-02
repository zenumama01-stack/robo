 * This is a bean that provides some meta-information about available icon sets.
public class IconSet {
     * Data format of icons
    public enum Format {
        PNG,
        SVG
    private final Set<Format> formats;
     * Construct a new pojo.
     * @param id the icon set id
     * @param label a short label text
     * @param description a description text
     * @param formats a set of formats that are supported
    public IconSet(String id, String label, String description, Set<Format> formats) {
        this.formats = new HashSet<>(formats);
     * Returns the id of the icon set.
     * @return id of the icon set
     * Returns a label for the icon set.
     * @return a short label text
     * Returns a description for the icon set.
     * @return a description text
     * Provides information about supported file formats of this set.
     * @return a set of formats
    public Set<Format> getFormats() {
        return Collections.unmodifiableSet(formats);
