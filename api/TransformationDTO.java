package org.openhab.core.io.rest.transform;
 * The {@link TransformationDTO} wraps a {@link Transformation}
@Schema(name = "Transformation")
public class TransformationDTO {
    public Map<String, String> configuration;
    public boolean editable = false;
    public TransformationDTO(Transformation transformation) {
        this.uid = transformation.getUID();
        this.label = transformation.getLabel();
        this.type = transformation.getType();
        this.configuration = transformation.getConfiguration();
