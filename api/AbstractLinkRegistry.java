 * {@link AbstractLinkRegistry} is an abstract class for link based registries,
 * which handle {@link AbstractLink}s.
 * @author Markus Rathgeb - Use separate collections to improve performance
 * @param <L> Concrete type of the abstract link
public abstract class AbstractLinkRegistry<L extends AbstractLink, P extends Provider<L>>
        extends AbstractRegistry<L, String, P> {
    private final ReentrantReadWriteLock toLinkLock = new ReentrantReadWriteLock();
    private final Map<String, Set<L>> itemNameToLink = new HashMap<>();
    private final Map<UID, Set<L>> linkedUidToLink = new HashMap<>();
    protected AbstractLinkRegistry(final Class<P> providerClazz) {
        super(providerClazz);
    protected void onAddElement(final L element) {
        super.onAddElement(element);
        toLinkAdded(element);
    protected void onRemoveElement(final L element) {
        super.onRemoveElement(element);
        toLinkRemoved(element);
    protected void onUpdateElement(final L oldElement, final L element) {
        super.onUpdateElement(oldElement, element);
        toLinkRemoved(oldElement);
    private void toLinkAdded(final L element) {
        final String itemName = element.getItemName();
        final UID linkedUid = element.getLinkedUID();
        toLinkLock.writeLock().lock();
            Set<L> set;
            set = Objects.requireNonNull(itemNameToLink.computeIfAbsent(itemName, k -> new HashSet<>()));
            set.add(element);
            set = Objects.requireNonNull(linkedUidToLink.computeIfAbsent(linkedUid, k -> new HashSet<>()));
            toLinkLock.writeLock().unlock();
    private void toLinkRemoved(final L element) {
            set = itemNameToLink.get(itemName);
            if (set != null) {
                set.remove(element);
                if (set.isEmpty()) {
                    itemNameToLink.remove(itemName);
            set = linkedUidToLink.get(linkedUid);
                    linkedUidToLink.remove(linkedUid);
     * Returns if an item for a given item name is linked to a channel or thing for a
     * given UID.
     * @return true if linked, false otherwise
    public boolean isLinked(final String itemName, final UID uid) {
        toLinkLock.readLock().lock();
            final Set<L> forItemName = itemNameToLink.get(itemName);
            final Set<L> forLinkedUID = linkedUidToLink.get(uid);
            if (forItemName == null || forLinkedUID == null) {
                return !Collections.disjoint(forItemName, forLinkedUID);
            toLinkLock.readLock().unlock();
     * Returns if a link for the given item name exists.
     * @return true if a link exists, otherwise false
    public boolean isLinked(final String itemName) {
            return itemNameToLink.get(itemName) != null; // if present the set is not empty by definition
     * Returns if a link for the given UID exists.
    public boolean isLinked(final UID uid) {
            return linkedUidToLink.get(uid) != null; // if present the set is not empty by definition
     * Returns the item names, which are bound to the given UID.
     * @return a non-null unmodifiable collection of item names that are linked to the given UID.
    public Set<String> getLinkedItemNames(final UID uid) {
            if (forLinkedUID == null) {
            return forLinkedUID.stream().map(AbstractLink::getItemName).collect(Collectors.toSet());
     * Returns all links for a given UID.
     * @param uid a channel UID
     * @return an unmodifiable set of links for the given UID
    public Set<L> getLinks(final UID uid) {
            return forLinkedUID != null ? new HashSet<>(forLinkedUID) : Set.of();
     * Returns all links for a given item name.
     * @param itemName the name of the item
     * @return an unmodifiable set of links for the given item name
    public Set<L> getLinks(final String itemName) {
            final Set<L> forLinkedUID = itemNameToLink.get(itemName);
