 * @author Simon Lamon - Initial contribution
public class SemanticsMetadataProviderTest extends JavaTest {
    private static final String ITEM_NAME = "switchItem";
    private static final String GROUP_ITEM_NAME = "groupItem";
    private @Mock @NonNullByDefault({}) ProviderChangeListener<@NonNull Metadata> changeListener;
    private @NonNullByDefault({}) SemanticsMetadataProvider semanticsMetadataProvider;
    public void beforeEach() throws Exception {
        setupInterceptedLogger(SemanticsMetadataProvider.class, LogLevel.DEBUG);
        SemanticTagRegistry semanticTagRegistry = new SemanticTagRegistryImpl(new DefaultSemanticTagProvider(),
        semanticsMetadataProvider = new SemanticsMetadataProvider(itemRegistry, semanticTagRegistry) {
                addProviderChangeListener(changeListener);
        semanticsMetadataProvider.activate();
    private void assertCorrectAddedEvents(Metadata expected) {
        ArgumentCaptor<Metadata> eventCaptor = ArgumentCaptor.forClass(Metadata.class);
        verify(changeListener, atLeastOnce()).added(eq(semanticsMetadataProvider), eventCaptor.capture());
        Metadata event = eventCaptor.getAllValues().stream().findFirst().get();
        assertEquals(expected, event);
    private void assertCorrectUpdatedEvents(Metadata oldExpected, Metadata expected) {
        ArgumentCaptor<Metadata> oldEventCaptor = ArgumentCaptor.forClass(Metadata.class);
        verify(changeListener, atLeastOnce()).updated(eq(semanticsMetadataProvider), oldEventCaptor.capture(),
                eventCaptor.capture());
        Metadata oldEvent = oldEventCaptor.getAllValues().stream().findFirst().get();
        assertEquals(oldExpected, oldEvent);
    private void assertCorrectRemoveEvents(Metadata expected) {
        verify(changeListener, atLeastOnce()).removed(eq(semanticsMetadataProvider), eventCaptor.capture());
    public void testItemAdded() {
        GenericItem item = new SwitchItem(ITEM_NAME);
        item.addTag("Door");
        semanticsMetadataProvider.added(item);
        Metadata metadata = Objects.requireNonNull(getMetadata(item));
        assertEquals("Equipment_Door", metadata.getValue());
        assertCorrectAddedEvents(metadata);
    public void testItemUpdatedToAnotherTag() {
        GenericItem oldItem = new SwitchItem(ITEM_NAME);
        oldItem.addTag("Door");
        semanticsMetadataProvider.added(oldItem);
        Metadata oldMetadata = Objects.requireNonNull(getMetadata(oldItem));
        GenericItem newItem = new SwitchItem(ITEM_NAME);
        newItem.addTag("Indoor");
        semanticsMetadataProvider.updated(oldItem, newItem);
        Metadata metadata = Objects.requireNonNull(getMetadata(newItem));
        assertEquals("Location_Indoor", metadata.getValue());
        assertCorrectUpdatedEvents(oldMetadata, metadata);
    public void testItemUpdatedToNoTag() {
        Metadata metadata = getMetadata(newItem);
        assertNull(metadata);
        assertCorrectRemoveEvents(oldMetadata);
    public void testItemRemoved() {
        Metadata oldMetadata = Objects.requireNonNull(getMetadata(item));
        semanticsMetadataProvider.removed(item);
        Metadata metadata = getMetadata(item);
    public void testGroupItemAddedBeforeMemberItemAdded() {
        item.addGroupName(GROUP_ITEM_NAME);
        GroupItem groupItem = new GroupItem(GROUP_ITEM_NAME);
        groupItem.addMember(item);
        groupItem.addTag("LivingRoom");
        when(itemRegistry.get(GROUP_ITEM_NAME)).thenReturn(groupItem);
        semanticsMetadataProvider.added(groupItem);
        assertEquals(GROUP_ITEM_NAME, metadata.getConfiguration().get("hasLocation"));
    public void testGroupItemAddedAfterMemberItemAdded() {
        assertEquals("Equipment_Door", oldMetadata.getValue());
        assertNull(oldMetadata.getConfiguration().get("hasLocation"));
    public void testGroupItemRemovedAfterMemberItemAdded() {
        assertEquals(GROUP_ITEM_NAME, oldMetadata.getConfiguration().get("hasLocation"));
        when(itemRegistry.get(GROUP_ITEM_NAME)).thenReturn(null);
        semanticsMetadataProvider.removed(groupItem);
        assertNull(metadata.getConfiguration().get("hasLocation"));
    public void testRecursiveGroupMembershipDoesNotResultInStackOverflowError() {
        assertDoesNotThrow(() -> semanticsMetadataProvider.added(groupItem1));
        assertLogMessage(SemanticsMetadataProvider.class, LogLevel.ERROR,
    public void testIndirectRecursiveMembershipDoesNotThrowStackOverflowError() {
        semanticsMetadataProvider.added(groupItem1);
        assertNoLogMessage(SemanticsMetadataProvider.class);
    private @Nullable Metadata getMetadata(Item item) {
        return semanticsMetadataProvider.getAll().stream() //
                .filter(metadata -> metadata.getUID().getItemName().equals(item.getName())) //
                .findFirst() //
