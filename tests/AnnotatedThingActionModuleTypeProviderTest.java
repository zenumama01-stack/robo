 * Tests for the {@link AnnotatedThingActionModuleTypeProvider}
public class AnnotatedThingActionModuleTypeProviderTest extends JavaTest {
    private static final ThingTypeUID TEST_THING_TYPE_UID = new ThingTypeUID("binding", "thing-type");
        Method method = Arrays.stream(TestThingActionProvider.class.getDeclaredMethods())
    private static final String TEST_ACTION_TYPE_ID = "test.testMethod#" + TEST_ACTION_SIGNATURE_HASH;
    private @Mock @NonNullByDefault({}) ThingHandler handler1Mock;
    private @Mock @NonNullByDefault({}) ThingHandler handler2Mock;
    private ThingActions actionProviderConf1 = new TestThingActionProvider();
    private ThingActions actionProviderConf2 = new TestThingActionProvider();
        when(handler1Mock.getThing()).thenReturn(ThingBuilder.create(TEST_THING_TYPE_UID, "test1").build());
        actionProviderConf1 = new TestThingActionProvider();
        actionProviderConf1.setThingHandler(handler1Mock);
        when(handler2Mock.getThing()).thenReturn(ThingBuilder.create(TEST_THING_TYPE_UID, "test2").build());
        actionProviderConf2 = new TestThingActionProvider();
        actionProviderConf2.setThingHandler(handler2Mock);
        AnnotatedThingActionModuleTypeProvider prov = new AnnotatedThingActionModuleTypeProvider(
                moduleTypeI18nServiceMock, new AnnotationActionModuleTypeHelper(actionInputsHelperMock),
                actionInputsHelperMock);
        prov.addAnnotatedThingActions(actionProviderConf1);
        prov.addAnnotatedThingActions(actionProviderConf2);
        prov.removeAnnotatedThingActions(actionProviderConf1);
                assertEquals("binding:thing-type:test2", po.getValue());
        prov.removeAnnotatedThingActions(actionProviderConf2);
    @ThingActionsScope(name = "test")
    private static class TestThingActionProvider implements ThingActions {
        private @Nullable ThingHandler handler;
        public @ActionOutput(name = ACTION_OUTPUT1, type = ACTION_OUTPUT1_TYPE, description = ACTION_OUTPUT1_DESCRIPTION, label = ACTION_OUTPUT1_LABEL, defaultValue = ACTION_OUTPUT1_DEFAULT_VALUE, reference = ACTION_OUTPUT1_REFERENCE, tags = {
                "tagOut11",
                "tagOut12" }) @ActionOutput(name = ACTION_OUTPUT2, type = ACTION_OUTPUT2_TYPE) Map<String, Object> testMethod(
        public void setThingHandler(ThingHandler handler) {
        public @Nullable ThingHandler getThingHandler() {
