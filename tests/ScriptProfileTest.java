import static org.openhab.core.automation.module.script.profile.ScriptProfile.*;
 * The {@link ScriptProfileTest} contains tests for the {@link ScriptProfile}
public class ScriptProfileTest extends JavaTest {
    private @Mock @NonNullByDefault({}) ProfileCallback profileCallback;
    private @Mock @NonNullByDefault({}) TransformationService transformationServiceMock;
    public void setUp() throws TransformationException {
        when(transformationServiceMock.transform(any(), any())).thenReturn("");
    public void testScriptNotExecutedAndNoValueForwardedToCallbackIfNoScriptDefined() throws TransformationException {
        ProfileContext profileContext = ProfileContextBuilder.create().build();
        ItemChannelLink link = new ItemChannelLink("DummyItem", new ChannelUID("foo:bar:baz:qux"));
        when(profileCallback.getItemChannelLink()).thenReturn(link);
        setupInterceptedLogger(ScriptProfile.class, LogLevel.ERROR);
        ScriptProfile scriptProfile = new ScriptProfile(mock(ProfileTypeUID.class), profileCallback, profileContext,
                transformationServiceMock);
        scriptProfile.onCommandFromHandler(OnOffType.ON);
        scriptProfile.onStateUpdateFromHandler(OnOffType.ON);
        scriptProfile.onTimeSeriesFromHandler(createTimeSeries(OnOffType.ON));
        scriptProfile.onCommandFromItem(OnOffType.ON);
        verify(transformationServiceMock, never()).transform(any(), any());
        verify(profileCallback, never()).handleCommand(any());
        verify(profileCallback, never()).sendTimeSeries(any());
        verify(profileCallback, never()).sendUpdate(any());
        verify(profileCallback, never()).sendCommand(any());
        assertLogMessage(ScriptProfile.class, LogLevel.ERROR,
                "Neither 'toItemScript', 'commandFromItemScript' nor 'stateFromItemScript' defined in link '"
                        + link.toString() + "'. Profile will discard all states and commands.");
    public void fallsBackToToHandlerScriptIfCommandFromItemScriptNotDefined() throws TransformationException {
        ProfileContext profileContext = ProfileContextBuilder.create().withToItemScript("inScript")
                .withToHandlerScript("outScript").withAcceptedCommandTypes(List.of(OnOffType.class))
                .withAcceptedDataTypes(List.of(OnOffType.class))
                .withHandlerAcceptedCommandTypes(List.of(OnOffType.class)).build();
        when(transformationServiceMock.transform(any(), any())).thenReturn(OnOffType.OFF.toString());
        setupInterceptedLogger(ScriptProfile.class, LogLevel.WARN);
        scriptProfile.onCommandFromHandler(DecimalType.ZERO);
        scriptProfile.onStateUpdateFromHandler(DecimalType.ZERO);
        scriptProfile.onCommandFromItem(DecimalType.ZERO);
        verify(transformationServiceMock, times(3)).transform(any(), any());
        verify(profileCallback, times(1)).handleCommand(OnOffType.OFF);
        verify(profileCallback).sendUpdate(OnOffType.OFF);
        verify(profileCallback).sendCommand(OnOffType.OFF);
        assertLogMessage(ScriptProfile.class, LogLevel.WARN,
                "'toHandlerScript' has been deprecated! Please use 'commandFromItemScript' instead in link '"
                        + link.toString() + "'.");
    public void scriptExecutionErrorForwardsNoValueToCallback() throws TransformationException {
                .withCommandFromItemScript("outScript").withStateFromItemScript("outScript").build();
        when(transformationServiceMock.transform(any(), any()))
                .thenThrow(new TransformationException("intentional failure"));
        scriptProfile.onStateUpdateFromItem(OnOffType.ON);
        verify(transformationServiceMock, times(5)).transform(any(), any());
    public void scriptExecutionResultNullForwardsNoValueToCallback() throws TransformationException {
        when(transformationServiceMock.transform(any(), any())).thenReturn(null);
    public void scriptExecutionResultForwardsTransformedValueToCallback() throws TransformationException {
                .withCommandFromItemScript("outScript").withStateFromItemScript("outScript")
                .withAcceptedCommandTypes(List.of(OnOffType.class)).withAcceptedDataTypes(List.of(OnOffType.class))
        scriptProfile.onStateUpdateFromItem(DecimalType.ZERO);
        TimeSeries timeSeries = createTimeSeries(DecimalType.ZERO);
        scriptProfile.onTimeSeriesFromHandler(timeSeries);
        verify(profileCallback, times(2)).handleCommand(OnOffType.OFF);
        verify(profileCallback).sendTimeSeries(replaceTimeSeries(timeSeries, OnOffType.OFF));
    public void onlyToItemScriptDoesNotForwardOutboundCommands() throws TransformationException {
                .withHandlerAcceptedCommandTypes(List.of(DecimalType.class)).build();
    public void onlyToHandlerCommandScriptDoesNotForwardInboundCommands() throws TransformationException {
        ProfileContext profileContext = ProfileContextBuilder.create().withCommandFromItemScript("outScript")
                .withAcceptedCommandTypes(List.of(DecimalType.class)).withAcceptedDataTypes(List.of(DecimalType.class))
        scriptProfile.onTimeSeriesFromHandler(createTimeSeries(DecimalType.ZERO));
        verify(transformationServiceMock, times(1)).transform(any(), any());
    public void onlyToHandlerStateScriptDoesNotForwardInboundCommands() throws TransformationException {
        ProfileContext profileContext = ProfileContextBuilder.create().withStateFromItemScript("outScript")
    public void incompatibleStateOrCommandNotForwardedToCallback() throws TransformationException {
                .withAcceptedCommandTypes(List.of(DecimalType.class)).withAcceptedDataTypes(List.of(PercentType.class))
                .withHandlerAcceptedCommandTypes(List.of(HSBType.class)).build();
    public void fallbackToToHandlerScriptIfNotToHandlerCommandScript() throws TransformationException {
        ProfileContext profileContext = ProfileContextBuilder.create().withToHandlerScript("outScript")
    public void filteredTimeSeriesTest() throws TransformationException {
        when(transformationServiceMock.transform(any(), eq("0"))).thenReturn(OnOffType.OFF.toString());
        when(transformationServiceMock.transform(any(), eq("1"))).thenReturn(null);
        TimeSeries timeSeries = createTimeSeries(DecimalType.ZERO, DecimalType.valueOf("1"), DecimalType.ZERO);
            if (entry.state().equals(DecimalType.ZERO)) {
                transformedTimeSeries.add(entry.timestamp(), OnOffType.OFF);
        verify(profileCallback).sendTimeSeries(transformedTimeSeries);
    private TimeSeries createTimeSeries(State... states) {
        TimeSeries timeSeries = new TimeSeries(TimeSeries.Policy.ADD);
        Instant instant = Instant.now();
        for (State state : states) {
            timeSeries.add(instant, state);
            instant = instant.plusMillis(100);
        return timeSeries;
    private TimeSeries replaceTimeSeries(TimeSeries timeSeries, State state) {
        TimeSeries newTimeSeries = new TimeSeries(timeSeries.getPolicy());
        timeSeries.getStates().forEach(entry -> newTimeSeries.add(entry.timestamp(), state));
        return newTimeSeries;
    private static class ProfileContextBuilder {
        private final Map<String, Object> configuration = new HashMap<>();
        private List<Class<? extends State>> acceptedDataTypes = List.of();
        private List<Class<? extends Command>> acceptedCommandTypes = List.of();
        private List<Class<? extends Command>> handlerAcceptedCommandTypes = List.of();
        public static ProfileContextBuilder create() {
            return new ProfileContextBuilder();
        public ProfileContextBuilder withToItemScript(String toItem) {
            configuration.put(CONFIG_TO_ITEM_SCRIPT, toItem);
        public ProfileContextBuilder withToHandlerScript(String toHandlerScript) {
            configuration.put(CONFIG_TO_HANDLER_SCRIPT, toHandlerScript);
        public ProfileContextBuilder withCommandFromItemScript(String commandFromItemScript) {
            configuration.put(CONFIG_COMMAND_FROM_ITEM_SCRIPT, commandFromItemScript);
        public ProfileContextBuilder withStateFromItemScript(String stateFromItemScript) {
            configuration.put(CONFIG_STATE_FROM_ITEM_SCRIPT, stateFromItemScript);
        public ProfileContextBuilder withAcceptedDataTypes(List<Class<? extends State>> acceptedDataTypes) {
            this.acceptedDataTypes = acceptedDataTypes;
        public ProfileContextBuilder withAcceptedCommandTypes(List<Class<? extends Command>> acceptedCommandTypes) {
            this.acceptedCommandTypes = acceptedCommandTypes;
        public ProfileContextBuilder withHandlerAcceptedCommandTypes(
                List<Class<? extends Command>> handlerAcceptedCommandTypes) {
            this.handlerAcceptedCommandTypes = handlerAcceptedCommandTypes;
        public ProfileContext build() {
            ProfileContext mockedProfileContext = mock(ProfileContext.class);
            when(mockedProfileContext.getConfiguration()).thenReturn(new Configuration(configuration));
            when(mockedProfileContext.getAcceptedDataTypes()).thenReturn(acceptedDataTypes);
            when(mockedProfileContext.getAcceptedCommandTypes()).thenReturn(acceptedCommandTypes);
            when(mockedProfileContext.getHandlerAcceptedCommandTypes()).thenReturn(handlerAcceptedCommandTypes);
            return mockedProfileContext;
