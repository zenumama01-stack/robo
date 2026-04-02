package org.openhab.core.thing.link;
 * {@link AbstractLink} is the abstract base class of all links.
public abstract class AbstractLink implements Identifiable<String> {
     * Returns the link ID for a given item name and UID
     * @param itemName item name
     * @param uid UID
     * @return the item channel link ID
    public static String getIDFor(String itemName, UID uid) {
        return itemName + " -> " + uid.toString();
    private final @NonNullByDefault({}) String itemName;
     * @param itemName the item name for the link
     * @throws IllegalArgumentException if the item name is invalid
    protected AbstractLink(String itemName) {
        ItemUtil.assertValidItemName(itemName);
    protected AbstractLink() {
        this.itemName = null;
        if (obj instanceof AbstractLink link) {
            return getUID().equals(link.getUID());
     * Returns the ID for the link.
     * @return id (can not be null)
        return getIDFor(getItemName(), getLinkedUID());
     * Returns the item that is linked to the object.
     * @return item name (can not be null)
    public String getItemName() {
     * Returns the UID of the object, which is linked to the item.
     * @return UID (can not be null)
    public abstract UID getLinkedUID();
        return itemName.hashCode() * getLinkedUID().hashCode();
        return getUID();
