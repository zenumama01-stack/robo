package org.openhab.core.ui.components;
 * A root component is a special type of {@link UIComponent} at the root of the hierarchy.
 * It has a number of specific parameters, a set of tags, a timestamp, some configurable
 * parameters ("props") and is identifiable by its UID (generally a GUID).
public class RootUIComponent extends UIComponent implements Identifiable<String> {
    ConfigDescriptionDTO props;
    Date timestamp;
     * Empty constructor for deserialization.
    public RootUIComponent() {
     * Constructs a root component.
     * @param name the name of the UI component to render on client frontends, ie. "oh-block"
    public RootUIComponent(String name) {
        this.uid = UUID.randomUUID().toString();
        this.props = new ConfigDescriptionDTO(null, new ArrayList<>(), new ArrayList<>());
     * Constructs a root component with a specific UID.
     * @param uid the UID of the new card
    public RootUIComponent(String uid, String name) {
     * Gets the set of tags attached to the component
     * @return the card tags
     * Gets the timestamp of the component
     * @return the timestamp
    public @Nullable Date getTimestamp() {
     * Sets the specified timestamp of the component
     * @param date the timestamp
    public void setTimestamp(Date date) {
        this.timestamp = date;
     * Updates the timestamp of the component to the current date and time.
    public void updateTimestamp() {
     * Returns whether the component has a certain tag
     * @param tag the tag to check
     * @return true if the component is tagged with the specified tag
    public boolean hasTag(String tag) {
        return (tags != null && tags.contains(tag));
     * Adds a tag to the component
     * @param tag the tag to add
    public void addTag(String tag) {
     * Adds several tags to the component
     * @param tags the tags to add
    public void addTags(Collection<String> tags) {
    public void addTags(String... tags) {
        this.tags.addAll(Arrays.asList(tags));
     * Removes a tag on a component
     * @param tag the tag to remove
    public void removeTag(String tag) {
        this.tags.remove(tag);
     * Removes all tags on the component
    public void removeAllTags() {
        this.tags.clear();
     * Gets the configurable parameters ("props") of the component
     * @return the configurable parameters
    public ConfigDescriptionDTO getProps() {
     * Sets the configurable parameters ("props") of the component
     * @param props the configurable parameters
    public void setProps(ConfigDescriptionDTO props) {
        this.props = props;
