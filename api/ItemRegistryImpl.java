import org.openhab.core.items.MetadataAwareItem;
import org.openhab.core.service.CommandDescriptionService;
import org.openhab.core.service.StateDescriptionService;
 * This is the main implementing class of the {@link ItemRegistry} interface. It
 * keeps track of all declared items of all item providers and keeps their
 * current state in memory. This is the central point where states are kept and
 * thus is a core part for all stateful services.
 * @author Laurent Garnier - handle new DefaultStateDescriptionFragmentProvider
public class ItemRegistryImpl extends AbstractRegistry<Item, String, ItemProvider>
        implements ItemRegistry, RegistryChangeListener<Metadata> {
    private final Logger logger = LoggerFactory.getLogger(ItemRegistryImpl.class);
    private @Nullable StateDescriptionService stateDescriptionService;
    private @Nullable CommandDescriptionService commandDescriptionService;
    private final DefaultStateDescriptionFragmentProvider defaultStateDescriptionFragmentProvider;
    private @Nullable ItemStateConverter itemStateConverter;
    public ItemRegistryImpl(final @Reference MetadataRegistry metadataRegistry,
            final @Reference DefaultStateDescriptionFragmentProvider defaultStateDescriptionFragmentProvider) {
        super(ItemProvider.class);
        this.defaultStateDescriptionFragmentProvider = defaultStateDescriptionFragmentProvider;
    protected void activate(final ComponentContext componentContext) {
        super.activate(componentContext.getBundleContext());
        metadataRegistry.addRegistryChangeListener(this);
        metadataRegistry.removeRegistryChangeListener(this);
        final Item item = get(name);
            throw new ItemNotFoundException(name);
        Collection<Item> items = getItems(name);
        if (items.size() > 1) {
            throw new ItemNotUniqueException(name, items);
        return items.iterator().next();
        Collection<Item> matchedItems = new ArrayList<>();
        for (Item item : getItems()) {
            if (item.getType().equals(type)) {
                matchedItems.add(item);
        return matchedItems;
            if (item.getName().matches(regex)) {
    private void addToGroupItems(Item item, List<String> groupItemNames) {
        for (String groupName : groupItemNames) {
                if (getItem(groupName) instanceof GroupItem groupItem) {
                // the group might not yet be registered, let's ignore this
    private void replaceInGroupItems(Item oldItem, Item newItem, List<String> groupItemNames) {
                    groupItem.replaceMember(oldItem, newItem);
     * An item should be initialized, which means that the event publisher is
     * injected and its implementation is notified that it has just been
     * created, so it can perform any task it needs to do after its creation.
     * @param item the item to initialize
     * @throws IllegalArgumentException if the item has no valid name
    private void initializeItem(Item item) throws IllegalArgumentException {
        ItemUtil.assertValidItemName(item.getName());
        injectServices(item);
            // fill group with its members
            addMembersToGroupItem(groupItem);
        // add the item to all relevant groups
        addToGroupItems(item, item.getGroupNames());
        defaultStateDescriptionFragmentProvider.onItemAdded(item);
    private void injectServices(Item item) {
            genericItem.setEventPublisher(getEventPublisher());
            genericItem.setStateDescriptionService(stateDescriptionService);
            genericItem.setCommandDescriptionService(commandDescriptionService);
            genericItem.setItemStateConverter(itemStateConverter);
        if (item instanceof MetadataAwareItem metadataAwareItem) {
            metadataRegistry.stream().filter(m -> m.getUID().getItemName().equals(item.getName()))
                    .forEach(metadataAwareItem::addedMetadata);
    private void addMembersToGroupItem(GroupItem groupItem) {
        for (Item i : getItems()) {
            if (i.getGroupNames().contains(groupItem.getName())) {
                groupItem.addMember(i);
    private void removeFromGroupItems(Item item, List<String> groupItemNames) {
    protected void onAddElement(Item element) throws IllegalArgumentException {
        initializeItem(element);
    protected void onRemoveElement(Item element) {
        if (element instanceof GenericItem genericItem) {
            genericItem.dispose();
        removeFromGroupItems(element, element.getGroupNames());
        defaultStateDescriptionFragmentProvider.onItemRemoved(element);
    protected void beforeUpdateElement(Item existingElement) {
        if (existingElement instanceof GenericItem genericItem) {
    protected void onUpdateElement(Item oldItem, Item item) {
        // don't use #initialize and retain order of items in groups:
        List<String> oldNames = oldItem.getGroupNames();
        List<String> newNames = item.getGroupNames();
        List<String> commonNames = oldNames.stream().filter(newNames::contains).toList();
        removeFromGroupItems(oldItem, oldNames.stream().filter(name -> !commonNames.contains(name)).toList());
        replaceInGroupItems(oldItem, item, commonNames);
        addToGroupItems(item, newNames.stream().filter(name -> !commonNames.contains(name)).toList());
        defaultStateDescriptionFragmentProvider.onItemRemoved(oldItem);
            ((GenericItem) item).setEventPublisher(eventPublisher);
            ((GenericItem) item).setEventPublisher(null);
    protected void setItemStateConverter(ItemStateConverter itemStateConverter) {
            ((GenericItem) item).setItemStateConverter(itemStateConverter);
    protected void unsetItemStateConverter(ItemStateConverter itemStateConverter) {
        this.itemStateConverter = null;
            ((GenericItem) item).setItemStateConverter(null);
        List<Item> filteredItems = new ArrayList<>();
            if (itemHasTags(item, tags)) {
                filteredItems.add(item);
        return filteredItems;
    private boolean itemHasTags(Item item, String... tags) {
            if (!item.hasTag(tag)) {
        Collection<T> filteredItems = new ArrayList<>();
        Collection<Item> items = getItemsByTag(tags);
            if (typeFilter.isInstance(item)) {
                filteredItems.add((T) item);
        for (Item item : getItemsOfType(type)) {
        ManagedItemProvider mp = (ManagedItemProvider) getManagedProvider();
        return mp.remove(itemName, recursive);
    protected void notifyListenersAboutAddedElement(Item element) {
        postEvent(ItemEventFactory.createAddedEvent(element));
    protected void notifyListenersAboutRemovedElement(Item element) {
        postEvent(ItemEventFactory.createRemovedEvent(element));
    protected void notifyListenersAboutUpdatedElement(Item oldElement, Item element) {
        postEvent(ItemEventFactory.createUpdateEvent(element, oldElement));
    public void removed(Provider<Item> provider, Item element) {
        if (provider instanceof ManagedItemProvider) {
            // remove our metadata for that item
            logger.debug("Item {} was removed, trying to clean up corresponding metadata", element.getUID());
            metadataRegistry.removeItemMetadata(element.getName());
    public void setStateDescriptionService(StateDescriptionService stateDescriptionService) {
        this.stateDescriptionService = stateDescriptionService;
            ((GenericItem) item).setStateDescriptionService(stateDescriptionService);
    public void unsetStateDescriptionService(StateDescriptionService stateDescriptionService) {
        this.stateDescriptionService = null;
            ((GenericItem) item).setStateDescriptionService(null);
    public void setCommandDescriptionService(CommandDescriptionService commandDescriptionService) {
        this.commandDescriptionService = commandDescriptionService;
            ((GenericItem) item).setCommandDescriptionService(commandDescriptionService);
    public void unsetCommandDescriptionService(CommandDescriptionService commandDescriptionService) {
        this.commandDescriptionService = null;
            ((GenericItem) item).setCommandDescriptionService(null);
    protected void setManagedProvider(ManagedItemProvider provider) {
    protected void unsetManagedProvider(ManagedItemProvider provider) {
        String itemName = element.getUID().getItemName();
            metadataAwareItem.addedMetadata(element);
            metadataAwareItem.removedMetadata(element);
            metadataAwareItem.updatedMetadata(oldElement, element);
    public void notifyListenersAboutItemExternalUpdate(Item oldItem, Item newItem) {
