package org.openhab.core.model.yaml.internal.semantics;
import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonValue;
 * The {@link YamlSemanticTagDTO} is a data transfer object used to serialize a semantic tag
 * @author Jimmy Tanagra - Added JsonCreator and JsonValue to support short-form syntax
@YamlElementName("tags")
public class YamlSemanticTagDTO implements YamlElement, Cloneable {
    public YamlSemanticTagDTO() {
    @JsonCreator
    public static YamlSemanticTagDTO fromString(String value) {
        YamlSemanticTagDTO dto = new YamlSemanticTagDTO();
        dto.label = value;
    @JsonValue
    public Object serialize() {
        if ((description == null || description.isBlank()) && (synonyms == null || synonyms.isEmpty())) {
        Map<String, Object> map = new LinkedHashMap<>();
        if (label != null && !label.isBlank()) {
            map.put("label", label);
        if (description != null && !description.isBlank()) {
            map.put("description", description);
        if (synonyms != null && !synonyms.isEmpty()) {
            map.put("synonyms", synonyms);
        YamlSemanticTagDTO copy;
            copy = (YamlSemanticTagDTO) super.clone();
            return new YamlSemanticTagDTO();
                errors.add("tag uid is missing");
        YamlSemanticTagDTO that = (YamlSemanticTagDTO) obj;
        return Objects.equals(uid, that.uid) && Objects.equals(label, that.label)
                && Objects.equals(description, that.description) && Objects.equals(synonyms, that.synonyms);
