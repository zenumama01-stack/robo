package org.openhab.core.io.rest.core.internal.tag;
import org.openhab.core.semantics.SemanticTag;
 * A DTO representing a {@link SemanticTag}.
 * @author Jimmy Tanagra - initial contribution
 * @author Laurent Garnier - Class renamed and members uid, description and editable added
@Schema(name = "EnrichedSemanticTag")
public class EnrichedSemanticTagDTO {
    public List<String> synonyms;
    public boolean editable;
    public EnrichedSemanticTagDTO(SemanticTag tag, boolean editable) {
        this.uid = tag.getUID();
        this.name = tag.getUID().substring(tag.getUID().lastIndexOf("_") + 1);
        this.label = tag.getLabel();
        this.description = tag.getDescription();
        this.synonyms = tag.getSynonyms();
        this.editable = editable;
