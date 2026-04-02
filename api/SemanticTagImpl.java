import java.util.MissingResourceException;
 * This is the main implementing class of the {@link SemanticTag} interface.
public class SemanticTagImpl implements SemanticTag {
    private static final String TAGS_BUNDLE_NAME = "tags";
    private String uid;
    private String parent;
    private List<String> synonyms;
    public SemanticTagImpl(String uid, @Nullable String label, @Nullable String description,
            @Nullable List<String> synonyms) {
        this(uid, label, description);
        if (synonyms != null) {
            this.synonyms = new ArrayList<>();
            for (String synonym : synonyms) {
                this.synonyms.add(synonym.trim());
            @Nullable String synonyms) {
        if (synonyms != null && !synonyms.isBlank()) {
            for (String synonym : synonyms.split(",")) {
    private SemanticTagImpl(String uid, @Nullable String label, @Nullable String description) {
        int idx = uid.lastIndexOf("_");
        if (idx < 0) {
            this.name = uid.trim();
            this.parent = "";
            this.name = uid.substring(idx + 1).trim();
            this.parent = uid.substring(0, idx).trim();
        this.label = label == null ? "" : label.trim();
        this.description = description == null ? "" : description.trim();
        this.synonyms = List.of();
    public String getParentUID() {
    public List<String> getSynonyms() {
        return synonyms;
    public SemanticTag localized(Locale locale) {
        ResourceBundle rb = ResourceBundle.getBundle(TAGS_BUNDLE_NAME, locale,
        String label;
        List<String> synonyms;
        String description;
            String entry = rb.getString(uid);
            int idx = entry.indexOf(",");
                label = entry.substring(0, idx);
                String synonymsCsv = entry.substring(idx + 1);
                synonyms = synonymsCsv.isBlank() ? null : List.of(synonymsCsv.split(","));
                label = entry;
                synonyms = null;
        } catch (MissingResourceException e) {
            label = getLabel();
            synonyms = getSynonyms();
            description = rb.getString(uid + "__description");
            description = getDescription();
        return new SemanticTagImpl(uid, label, description, synonyms);
