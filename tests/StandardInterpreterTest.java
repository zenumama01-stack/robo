import static org.mockito.Mockito.reset;
import static org.openhab.core.voice.internal.text.StandardInterpreter.VOICE_SYSTEM_NAMESPACE;
import static org.openhab.core.voice.text.AbstractRuleBasedInterpreter.IS_FORCED_CONFIGURATION;
import static org.openhab.core.voice.text.AbstractRuleBasedInterpreter.IS_SILENT_CONFIGURATION;
import static org.openhab.core.voice.text.AbstractRuleBasedInterpreter.IS_TEMPLATE_CONFIGURATION;
 * Test the standard interpreter
public class StandardInterpreterTest {
    private @NonNullByDefault({}) StandardInterpreter standardInterpreter;
    private @NonNullByDefault({}) STTService sttService;
    private @NonNullByDefault({}) TTSService ttsService;
    private @NonNullByDefault({}) AudioSink audioSink;
    private static final String OK_RESPONSE = "Ok.";
        this.standardInterpreter = new StandardInterpreter(eventPublisherMock, itemRegistryMock, metadataRegistryMock);
    public void noNameCollisionOnSingleExactMatch() throws InterpretationException {
        var computerItem = new SwitchItem("computer");
        computerItem.setLabel("Computer");
        var computerScreenItem = new SwitchItem("screen");
        computerScreenItem.setLabel("Computer Screen");
        List<Item> items = List.of(computerItem, computerScreenItem);
        when(itemRegistryMock.getItems()).thenReturn(items);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "turn off computer"));
        verify(eventPublisherMock, times(1))
                .post(ItemEventFactory.createCommandEvent(computerItem.getName(), OnOffType.OFF));
    public void noNameCollisionOnSingleExactMatchForGroups() throws InterpretationException {
        var computerItem = Mockito.spy(new GroupItem("computer"));
        var computerSwitchItem = new SwitchItem("computer_power");
        computerSwitchItem.setLabel("Power");
        var screenItem = Mockito.spy(new GroupItem("screen"));
        screenItem.setLabel("Computer Screen");
        var screenSwitchItem = new SwitchItem("screen_power");
        screenSwitchItem.setLabel("Power");
        when(computerItem.getMembers()).thenReturn(Set.of(computerSwitchItem));
        when(screenItem.getMembers()).thenReturn(Set.of(screenSwitchItem));
        List<Item> items = List.of(computerItem, computerSwitchItem, screenItem, screenSwitchItem);
                .post(ItemEventFactory.createCommandEvent(computerSwitchItem.getName(), OnOffType.OFF));
    public void noNameCollisionWhenDialogContext() throws InterpretationException {
        var locationItem = Mockito.spy(new GroupItem("livingroom"));
        locationItem.setLabel("Living room");
        var computerItem2 = new SwitchItem("computer2");
        computerItem2.setLabel("Computer");
        when(locationItem.getMembers()).thenReturn(Set.of(computerItem));
        var dialogContext = new DialogContext(null, null, sttService, ttsService, null, List.of(), audioSource,
                audioSink, Locale.ENGLISH, "", locationItem.getName(), null, null);
        List<Item> items = List.of(computerItem2, locationItem, computerItem);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "turn off computer", dialogContext));
    public void allowUseItemSynonyms() throws InterpretationException {
        MetadataKey computerMetadataKey = new MetadataKey("synonyms", computerItem.getName());
        when(metadataRegistryMock.get(computerMetadataKey))
                .thenReturn(new Metadata(computerMetadataKey, "PC,Bedroom PC", null));
        when(metadataRegistryMock.get(new MetadataKey(VOICE_SYSTEM_NAMESPACE, computerItem.getName())))
        List<Item> items = List.of(computerItem);
        reset(eventPublisherMock);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "turn off pc"));
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "turn off bedroom pc"));
    public void allowUseItemDescription() throws InterpretationException {
        var brightness = new DimmerItem("brightness") {
                return () -> List.of(new CommandOption("10", "low"), new CommandOption("50", "medium"),
                        new CommandOption("90", "high"), new CommandOption("100", "high two"));
            public @Nullable CommandDescription getCommandDescription(@Nullable Locale locale) {
                return getCommandDescription();
        brightness.setLabel("Brightness");
        List<Item> items = List.of(brightness);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "set the brightness to low"));
                .post(ItemEventFactory.createCommandEvent(brightness.getName(), new PercentType(10)));
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "set brightness to medium"));
                .post(ItemEventFactory.createCommandEvent(brightness.getName(), new PercentType(50)));
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "set brightness high"));
                .post(ItemEventFactory.createCommandEvent(brightness.getName(), new PercentType(90)));
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "set brightness high two"));
                .post(ItemEventFactory.createCommandEvent(brightness.getName(), new PercentType(100)));
    public void allowUseCustomItemCommands() throws InterpretationException {
        var tvItem = new StringItem("virtual") {
                return () -> List.of(new CommandOption("KEY_4", "channel 4"));
        tvItem.setLabel("tv");
        MetadataKey voiceMetadataKey = new MetadataKey(VOICE_SYSTEM_NAMESPACE, tvItem.getName());
        when(metadataRegistryMock.get(voiceMetadataKey))
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $cmd$ on|at the? $name$", null));
        List<Item> items = List.of(tvItem);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "watch channel 4 on the tv"));
                .post(ItemEventFactory.createCommandEvent(tvItem.getName(), new StringType("KEY_4")));
    public void allowUseCustomCommandCommands() throws InterpretationException {
        var tvItem = new StringItem("tv") {
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $cmd$ on|at the? tv", null));
    public void allowHandleQuestionWithCustomCommand() throws InterpretationException {
        var triggerItem = new StringItem("trigger_item") {
                return () -> List.of(new CommandOption("day", "day"), new CommandOption("time", "time"));
        MetadataKey voiceMetadataKey = new MetadataKey(VOICE_SYSTEM_NAMESPACE, triggerItem.getName());
                .thenReturn(new Metadata(voiceMetadataKey, "what $cmd$ is it", null));
        List<Item> items = List.of(triggerItem);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "what time is it?"));
                .post(ItemEventFactory.createCommandEvent(triggerItem.getName(), new StringType("time")));
    public void allowForceCustomCommand() throws InterpretationException {
            public <T extends State> @Nullable T getStateAs(Class<T> typeClass) {
                return (T) new StringType("time");
        HashMap<String, Object> configuration = new HashMap<>();
        configuration.put(IS_FORCED_CONFIGURATION, true);
                .thenReturn(new Metadata(voiceMetadataKey, "what $cmd$ is it", configuration));
    public void allowUseCustomItemDynamicCommands() throws InterpretationException {
        var tvItem = new StringItem("tv");
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $*$ on|at the? $name$", null));
                .post(ItemEventFactory.createCommandEvent(tvItem.getName(), new StringType("channel 4")));
    public void allowUseCommandsWithoutAnswer() throws InterpretationException {
        configuration.put(IS_SILENT_CONFIGURATION, true);
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $*$ on|at the? $name$", configuration));
        assertEquals("", standardInterpreter.interpret(Locale.ENGLISH, "watch channel 4 on the tv"));
    public void allowUseCommandsFromTemplate() throws InterpretationException {
        var virtualItem = new StringItem("virtual");
        virtualItem.setLabel("tv rule");
        virtualItem.addTag("tv");
        tvItem.addTag("tv");
        MetadataKey voiceMetadataKey = new MetadataKey(VOICE_SYSTEM_NAMESPACE, virtualItem.getName());
        configuration.put(IS_TEMPLATE_CONFIGURATION, true);
        List<Item> items = List.of(virtualItem, tvItem);
    public void allowUseCustomDynamicCommands() throws InterpretationException {
        var virtualItem = new StringItem("tv");
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $*$", null));
        List<Item> items = List.of(virtualItem);
        assertEquals(OK_RESPONSE, standardInterpreter.interpret(Locale.ENGLISH, "watch channel 4"));
                .post(ItemEventFactory.createCommandEvent(virtualItem.getName(), new StringType("channel 4")));
    public void allowUseItemDescriptionOnCustomCommands() throws InterpretationException {
        var cmdDescription = new CommandDescription() {
            public List<CommandOption> getCommandOptions() {
                return List.of(new CommandOption("KEY_4", "channel 4"));
        var virtualItem = new StringItem("virtual") {
                return cmdDescription;
                .thenReturn(new Metadata(voiceMetadataKey, "watch|play $*$ on|at? the? tv", null));
                .post(ItemEventFactory.createCommandEvent(virtualItem.getName(), new StringType("KEY_4")));
