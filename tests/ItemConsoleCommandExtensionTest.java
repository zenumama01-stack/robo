public class ItemConsoleCommandExtensionTest {
    private @Mock @NonNullByDefault({}) ManagedItemProvider managedItemProviderMock;
    private @NonNullByDefault({}) ConsoleCommandCompleter completer;
        completer = new ItemConsoleCommandExtension(itemRegistryMock, managedItemProviderMock).getCompleter();
    public void completeSubcommands() {
        assertTrue(completer.complete(new String[] { "" }, 0, 0, candidates));
        assertEquals("addTag ", candidates.getFirst());
        assertEquals("clear ", candidates.get(1));
        assertEquals("list ", candidates.get(2));
        assertEquals("remove ", candidates.get(3));
        assertEquals("rmTag ", candidates.get(4));
        assertTrue(completer.complete(new String[] { "A", "Item1" }, 0, 1, candidates));
    public void completeManagedItems() {
        List<Item> items = List.of(new SwitchItem("Item1"));
        when(managedItemProviderMock.getAll()).thenReturn(items);
        assertFalse(completer.complete(new String[] { "bogus", "I" }, 1, 1, candidates));
        assertTrue(completer.complete(new String[] { "addTag", "I" }, 0, 6, candidates));
        assertTrue(completer.complete(new String[] { "addTag", "I" }, 1, 1, candidates));
        assertTrue(completer.complete(new String[] { "rmTag", "I" }, 1, 1, candidates));
    public void completeAllItems() {
        List<Item> items = List.of(new SwitchItem("Item2"));
        assertTrue(completer.complete(new String[] { "remove", "I" }, 0, 6, candidates));
        assertEquals("remove ", candidates.getFirst());
        assertTrue(completer.complete(new String[] { "remove", "I" }, 1, 1, candidates));
        assertEquals("Item2 ", candidates.getFirst());
    public void completeRmTag() {
        var item3 = new SwitchItem("Item3");
        var item4 = new SwitchItem("Item4");
        item3.addTag("Tag1");
        when(managedItemProviderMock.get(anyString())).thenAnswer(invocation -> {
            return switch ((String) invocation.getArguments()[0]) {
                case "Item3" -> item3;
                case "Item4" -> item4;
        // wrong sub-command
        assertFalse(completer.complete(new String[] { "addTag", "Item3", "" }, 2, 0, candidates));
        // Item doesn't exist
        assertFalse(completer.complete(new String[] { "rmTag", "Item2", "" }, 2, 0, candidates));
        // Item has no tags
        assertFalse(completer.complete(new String[] { "rmTag", "Item4", "" }, 2, 0, candidates));
        assertTrue(completer.complete(new String[] { "rmTag", "Item3", "" }, 2, 0, candidates));
        assertEquals("Tag1 ", candidates.getFirst());
