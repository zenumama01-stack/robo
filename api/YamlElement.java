package org.openhab.core.model.yaml;
import org.openhab.core.model.yaml.internal.YamlModelRepositoryImpl;
 * The {@link YamlElement} interface must be implemented by any classes that need to be handled by the
 * {@link YamlModelRepositoryImpl}.
 * Implementations
 * <li>MUST have a default constructor to allow deserialization with Jackson</li>
 * <li>MUST provide {@code equals(Object other)} and {@code hashcode()} methods</li>
 * <li>MUST be annotated with {@link YamlElementName} containing the element name</li>
 * <li>SHOULD implement a proper {@code toString()} method</li>
 * @author Jan N. Klug - Refactoring and improvements to JavaDoc
 * @author Laurent Garnier - Added methods setId and cloneWithoutId
 * @author Laurent Garnier - Added parameters to method isValid
public interface YamlElement {
     * Get the identifier of this element.
     * Identifiers
     * <li>MUST be non-null</li>
     * <li>MUST be unique within a model</li>
     * <li>SHOULD be unique across all models</li>
     * @return the identifier as a string
    @NonNull
     * Set the identifier of this element.
     * @param id the identifier as a string
    void setId(@NonNull String id);
     * Clone this element putting the identifier to null.
     * @return the cloned element
    YamlElement cloneWithoutId();
     * Check if this element is valid and should be included in the model.
     * <li>MUST check that at least {link #getId()} returns a non-null value</li>
     * <li>SHOULD log the reason of failed checks at WARN level</li>
     * <li>MAY perform additional checks</li>
     * @param errors a list of error messages to fill with fatal invalid controls; can be null to ignore messages
     * @param warnings a list of warning messages to fill with non-fatal invalid controls; can be null to ignore
     *            messages
     * @return {@code true} if all the checks are completed successfully
    boolean isValid(@Nullable List<@NonNull String> errors, @Nullable List<@NonNull String> warnings);
