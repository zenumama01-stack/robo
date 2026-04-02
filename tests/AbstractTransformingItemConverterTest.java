import static org.mockito.Mockito.only;
 * The {@link AbstractTransformingItemConverterTest} is a test class for the
 * {@link AbstractTransformingChannelHandler}
public class AbstractTransformingItemConverterTest {
    private @NonNullByDefault({}) Consumer<String> sendValue;
    private @NonNullByDefault({}) Consumer<State> updateState;
    private @NonNullByDefault({}) Consumer<Command> postCommand;
    private @NonNullByDefault({}) AutoCloseable closeable;
    @Spy
    private ChannelTransformation stateChannelTransformation = new ChannelTransformation((String) null);
    private ChannelTransformation commandChannelTransformation = new ChannelTransformation((String) null);
    public void init() {
        closeable = MockitoAnnotations.openMocks(this);
    public void undefOnNullContentTest() {
        TestChannelHandler realConverter = new TestChannelHandler(updateState, postCommand, sendValue,
                stateChannelTransformation, commandChannelTransformation, false);
        TestChannelHandler converter = spy(realConverter);
        converter.process(null);
        // make sure UNDEF is send as state update
        verify(updateState, only()).accept(UnDefType.UNDEF);
        verify(postCommand, never()).accept(any());
        verify(sendValue, never()).accept(any());
        // make sure no other processing applies
        verify(converter, never()).toState(any());
        verify(converter, never()).toCommand(any());
        verify(converter, never()).toString(any());
    public void commandIsPostedAsCommand() {
        TestChannelHandler converter = new TestChannelHandler(updateState, postCommand, sendValue,
                stateChannelTransformation, commandChannelTransformation, true);
        converter.process(new ChannelHandlerContent("TEST".getBytes(StandardCharsets.UTF_8), "", null));
        // check state transformation is applied
        verify(stateChannelTransformation).apply(any());
        verify(commandChannelTransformation, never()).apply(any());
        // check only postCommand is applied
        verify(updateState, never()).accept(any());
        verify(postCommand, only()).accept(new StringType("TEST"));
    public void updateIsPostedAsUpdate() {
        // check only updateState is called
        verify(updateState, only()).accept(new StringType("TEST"));
    public void sendCommandSendsCommand() {
        converter.send(new StringType("TEST"));
        // check command transformation is applied
        verify(stateChannelTransformation, never()).apply(any());
        verify(commandChannelTransformation).apply(any());
        // check only sendValue is applied
        verify(sendValue, only()).accept("TEST");
    private static class TestChannelHandler extends AbstractTransformingChannelHandler {
        private boolean hasCommand;
        public TestChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
                @Nullable Consumer<String> sendValue, ChannelTransformation stateChannelTransformation,
                ChannelTransformation commandChannelTransformation, boolean hasCommand) {
            super(updateState, postCommand, sendValue, stateChannelTransformation, commandChannelTransformation,
            this.hasCommand = hasCommand;
            return hasCommand ? new StringType(value) : null;
            return Optional.of(new StringType(value));
