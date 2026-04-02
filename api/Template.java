 * The templates define types of shared, ready to use definitions of automation objects, which
 * can be instantiated and configured to produce automation instances. Each Template has a unique identifier (UID).
 * The {@link Template}s can be used by any creator of automation objects, but they can be modified only by its owner.
public interface Template extends Identifiable<String> {
     * Gets the unique identifier (UID) of a Template.
     * @return the identifier of the Template.
     * Gets the assigned tags to a Template. The templates can have {@code tags} - non-hierarchical keywords or terms
     * for describing them. The tags are used to filter the templates.
     * @return the tags assigned to the template.
     * Gets the label of a Template. The label is a short, human-readable description of the Template defined by its
     * creator.
     * @return the label of the Template.
     * Gets the description of a Template. The description is a detailed, human-understandable description of the
     * purpose of a Template, defined by its creator.
     * @return the description of the Template.
     * Shows the visibility of a Template.
     * @return the visibility of the Template.
