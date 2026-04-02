 * Test cases for {@link GroupStateTriggerHandler}
class GroupStateTriggerHandlerTest extends JavaTest {
        setupInterceptedLogger(GroupStateTriggerHandler.class, LogLevel.WARN);
        GroupStateTriggerHandler unused = new GroupStateTriggerHandler(moduleMock, "ruleId", contextMock,
        stopInterceptedLogger(GroupStateTriggerHandler.class);
        assertLogMessage(GroupStateTriggerHandler.class, LogLevel.WARN,
                "GroupStateTrigger triggerId of rule ruleId has no groupName configured and will not work.");
                .thenReturn(new Configuration(Map.of(GroupStateTriggerHandler.CFG_GROUPNAME, "name")));
