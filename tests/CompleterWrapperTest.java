import static org.mockito.Mockito.verifyNoInteractions;
public class CompleterWrapperTest {
    private @Mock @NonNullByDefault({}) ConsoleCommandExtension commandExtension;
    private @Mock @NonNullByDefault({}) ConsoleCommandCompleter completer;
    private @Mock @NonNullByDefault({}) Session session;
    private @Mock @NonNullByDefault({}) CommandLine commandLine;
    private @NonNullByDefault({}) CommandWrapper commandWrapper;
    private @NonNullByDefault({}) Completer completerWrapper;
        when(commandExtension.getCommand()).thenReturn("command");
        when(commandExtension.getCompleter()).thenReturn(completer);
        when(commandExtension.getDescription()).thenReturn("description");
        commandWrapper = new CommandWrapper(commandExtension);
    public void fillsCommandDescriptionsLocalOnly() {
        completerWrapper = commandWrapper.getCompleter(true);
        var candidates = new ArrayList<Candidate>();
        when(commandLine.getArguments()).thenReturn(new String[] { "" });
        when(commandLine.getCursorArgumentIndex()).thenReturn(0);
        when(commandLine.getArgumentPosition()).thenReturn(0);
        completerWrapper.completeCandidates(session, commandLine, candidates);
        assertEquals(1, candidates.size());
        assertEquals("command", candidates.getFirst().value());
        assertEquals("description", candidates.getFirst().descr());
    public void fillsCommandDescriptionsLocalAndGlobal() {
        completerWrapper = commandWrapper.getCompleter(false);
        assertEquals(2, candidates.size());
        assertEquals("openhab:command", candidates.get(1).value());
        assertEquals("description", candidates.get(1).descr());
    public void completeCandidatesCompletesArguments() {
        when(commandLine.getArguments()).thenReturn(new String[] { "command", "subcmd" });
        when(commandLine.getCursorArgumentIndex()).thenReturn(1);
        when(commandLine.getArgumentPosition()).thenReturn(6);
        when(commandLine.getBufferPosition()).thenReturn(14);
        when(completer.complete(new String[] { "subcmd" }, 0, 6, new ArrayList<>())).thenReturn(false);
        assertTrue(candidates.isEmpty());
    public void doesntCallCompleterForOtherCommands() {
        var candidates = new ArrayList<String>();
        when(commandLine.getArguments()).thenReturn(new String[] { "somethingElse", "" });
        verifyNoInteractions(completer);
        assertEquals(-1, completerWrapper.complete(session, commandLine, candidates));
    public void callsCompleterWithProperlyScopedArguments() {
    public void callsCompleterForGlobalForm() {
        when(commandLine.getArguments()).thenReturn(new String[] { "openhab:command", "subcmd" });
