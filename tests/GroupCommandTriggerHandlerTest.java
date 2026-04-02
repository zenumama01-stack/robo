 * Test cases for {@link GroupCommandTriggerHandler}
class GroupCommandTriggerHandlerTest extends JavaTest {
    private @Mock @NonNullByDefault({}) Trigger moduleMock;
    private @Mock @NonNullByDefault({}) BundleContext contextMock;
    private @Mock @NonNullByDefault({}) ItemRegistry itemRegistryMock;
    public void testWarningLoggedWhenConfigurationInvalid() {
        when(moduleMock.getId()).thenReturn("triggerId");
        setupInterceptedLogger(GroupCommandTriggerHandler.class, LogLevel.WARN);
        GroupCommandTriggerHandler unused = new GroupCommandTriggerHandler(moduleMock, "ruleId", contextMock,
                itemRegistryMock);
        stopInterceptedLogger(GroupCommandTriggerHandler.class);
        assertLogMessage(GroupCommandTriggerHandler.class, LogLevel.WARN,
                "GroupCommandTrigger triggerId of rule ruleId has no groupName configured and will not work.");
    public void testNoWarningLoggedWhenConfigurationValid() {
                .thenReturn(new Configuration(Map.of(GroupCommandTriggerHandler.CFG_GROUPNAME, "name")));
                "Group 'name' needed for rule 'ruleId' is missing. Trigger 'triggerId' will not work.");
