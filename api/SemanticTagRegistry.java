 * {@link SemanticTagRegistry} tracks all {@link SemanticTag}s from different {@link SemanticTagProvider}s
 * and provides access to them.
public interface SemanticTagRegistry extends Registry<SemanticTag, String> {
     * Retrieves the class for a given id.
     * @param tagId the id of the tag. The id can be fully qualified (e.g. "Location_Room_Bedroom") or a segment, if
     *            this uniquely identifies the tag
     *            (e.g. "Bedroom").
     * @return the class for the id or null, if non exists.
    Class<? extends Tag> getTagClassById(String tagId);
     * Checks if a tag with a given id can be added to the registry.
     * To be added, no tag with this id must already exist in the registry, the tag name extracted from this id
     * must have a valid syntax, the parent tag extracted from this id must already exist in the registry and
     * should be either a default semantic tag or a managed semantic tag, and no tag with a same name must already
     * exist in the registry.
     * @param tag a tag to be added to the registry
     * @return true if the tag can be added, false if not
    boolean canBeAdded(SemanticTag tag);
     * Returns the provided tag + all tags having the provided tag as ancestor.
     * @param tag a tag in the registry
     * @return a list of all tags having the provided tag as ancestor, including the provided tag itself
    List<SemanticTag> getSubTree(SemanticTag tag);
     * Indicates if a tag is editable.
     * To be editable, a tag must be managed.
     * @return true if the provided tag is editable, false if not
    boolean isEditable(SemanticTag tag);
     * Removes the provided tag and all tags having the provided tag as ancestor.
     * Only removable (managed) tags are removed.
    void removeSubTree(SemanticTag tag);
