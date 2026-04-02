package org.openhab.core.items;
 * An {@link ActiveItem} can be modified. It provides methods for adding and
 * removing tags, adding and removing group names and setting a label and a
 * category.
 * @deprecated This class is not meant as a public API - it should only be used internally from within the framework
public interface ActiveItem extends Item {
     * Sets the label of an item
     * @param label label (can be null)
     * Sets the category of the item (can be null)
     * @param category category
    void setCategory(@Nullable String category);
     * Adds a tag to the item.
     * @param tag a tag that is to be added to item's tags.
    void addTag(String tag);
     * Adds tags to the item.
     * @param tags tags that are to be added to item's tags.
    void addTags(String... tags);
    void addTags(Collection<String> tags);
     * Removes a tag from the item.
     * @param tag a tag that is to be removed from item's tags.
    void removeTag(String tag);
     * Clears all tags of this item.
    void removeAllTags();
     * Removes the according item from a group.
     * @param groupItemName name of the group (must not be null)
    void removeGroupName(String groupItemName);
     * Assigns the according item to a group.
    void addGroupName(String groupItemName);
     * Assigns the according item to the given groups.
     * @param groupItemNames names of the groups (must not be null)
    void addGroupNames(String... groupItemNames);
    void addGroupNames(List<String> groupItemNames);
