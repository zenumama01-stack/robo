 * A representation of a sitemap Selection widget.
public interface Selection extends NonLinkableWidget {
     * Get the switch {@link Mapping}s. This method should return a modifiable list, allowing updates to the mappings.
     * @return mappings
    List<Mapping> getMappings();
     * Replace the widget mappings with a new list of mappings.
     * @param mappings
    void setMappings(List<Mapping> mappings);
