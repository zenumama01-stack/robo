package org.openhab.core.model.yaml.test;
 * The {@link FirstTypeDTO} is a test type implementing {@link YamlElement}
@YamlElementName("firstType")
public class FirstTypeDTO implements YamlElement, Cloneable {
    public FirstTypeDTO() {
    public FirstTypeDTO(String uid, String description) {
        FirstTypeDTO copy;
            copy = (FirstTypeDTO) super.clone();
            return new FirstTypeDTO();
        return uid != null && !uid.isBlank();
    public boolean equals(Object o) {
        FirstTypeDTO that = (FirstTypeDTO) o;
        return Objects.equals(uid, that.uid) && Objects.equals(description, that.description);
        return Objects.hash(uid, description);
