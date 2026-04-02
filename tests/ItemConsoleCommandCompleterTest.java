import static org.mockito.ArgumentMatchers.anyString;
public class ItemConsoleCommandCompleterTest {
        List<Item> items = List.of(new SwitchItem("Item1"), new SwitchItem("Item2"), new SwitchItem("JItem1"));
        when(itemRegistryMock.getAll()).thenReturn(items);
    private void mockGetItemByPattern() throws ItemNotFoundException, ItemNotUniqueException {
        when(itemRegistryMock.getItemByPattern(anyString())).thenAnswer(invocation -> {
            switch ((String) invocation.getArguments()[0]) {
                case "Item1":
                    return itemRegistryMock.getAll().iterator().next();
                    throw new ItemNotFoundException("It");
    public void completeItems() throws ItemNotFoundException, ItemNotUniqueException {
        var completer = new ItemConsoleCommandCompleter(itemRegistryMock);
        assertTrue(completer.complete(new String[] { "It" }, 0, 2, candidates));
        assertEquals("Item1 ", candidates.getFirst());
        assertEquals("Item2 ", candidates.get(1));
        assertTrue(completer.complete(new String[] { "JI" }, 0, 2, candidates));
        assertEquals("JItem1 ", candidates.getFirst());
        // case sensitive
        assertFalse(completer.complete(new String[] { "it" }, 0, 2, candidates));
        // doesn't complete anything when we're not referring to the current argument
        assertFalse(completer.complete(new String[] { "It", "It" }, 1, 2, candidates));
        // doesn't complete anything for the second argument
        assertFalse(completer.complete(new String[] { "Item1", "" }, 1, 0, candidates));
    public void completeSend() throws ItemNotFoundException, ItemNotUniqueException {
        var completer = new ItemConsoleCommandCompleter(itemRegistryMock,
                i -> i.getAcceptedCommandTypes().toArray(Class<?>[]::new));
        mockGetItemByPattern();
        // Can't find the item; no commands at all
        assertFalse(completer.complete(new String[] { "It", "O" }, 1, 1, candidates));
        assertTrue(completer.complete(new String[] { "Item1", "" }, 1, 0, candidates));
        assertEquals("OFF ", candidates.getFirst());
        assertEquals("ON ", candidates.get(1));
        assertEquals("REFRESH ", candidates.get(2));
        assertTrue(completer.complete(new String[] { "Item1", "o" }, 1, 1, candidates));
    public void completeUpdate() throws ItemNotFoundException, ItemNotUniqueException {
                i -> i.getAcceptedDataTypes().toArray(Class<?>[]::new));
        assertEquals(4, candidates.size());
        assertEquals("NULL ", candidates.getFirst());
        assertEquals("OFF ", candidates.get(1));
        assertEquals("ON ", candidates.get(2));
        assertEquals("UNDEF ", candidates.get(3));
