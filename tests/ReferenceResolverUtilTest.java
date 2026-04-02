public class ReferenceResolverUtilTest {
    private static final String CONTEXT_PROPERTY1 = "contextProperty1";
    private static final String CONTEXT_PROPERTY2 = "contextProperty2";
    private static final String CONTEXT_PROPERTY3 = "contextProperty3";
    private static final String CONTEXT_PROPERTY4 = "contextProperty4";
    private static final Map<String, Object> CONTEXT = new HashMap<>();
    private static final Map<String, Object> MODULE_CONFIGURATION = new HashMap<>();
    private static final Map<String, @Nullable Object> EXPECTED_MODULE_CONFIGURATION = new HashMap<>();
    private static final Map<String, String> COMPOSITE_CHILD_MODULE_INPUTS_REFERENCES = new HashMap<>();
    private static final Map<String, @Nullable Object> EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT = new HashMap<>();
    private final Logger logger = LoggerFactory.getLogger(ReferenceResolverUtilTest.class);
        // context from where references will be taken
        CONTEXT.put(CONTEXT_PROPERTY1, "value1");
        CONTEXT.put(CONTEXT_PROPERTY2, "value2");
        CONTEXT.put(CONTEXT_PROPERTY3, "value3");
        CONTEXT.put(CONTEXT_PROPERTY4, new BigDecimal(12345));
        // module configuration with references
        MODULE_CONFIGURATION.put("simpleReference", String.format("{{%s}}", CONTEXT_PROPERTY4));
        MODULE_CONFIGURATION.put("complexReference",
                String.format("Hello {{%s}} {{%s}}", CONTEXT_PROPERTY1, CONTEXT_PROPERTY4));
        MODULE_CONFIGURATION.put("complexReferenceWithMissing",
                String.format("Testing {{UNKNOWN}}, {{%s}}", CONTEXT_PROPERTY4));
        MODULE_CONFIGURATION.put("complexReferenceArray",
                String.format("[{{%s}}, {{%s}}, staticText]", CONTEXT_PROPERTY2, CONTEXT_PROPERTY3));
        MODULE_CONFIGURATION.put("complexReferenceArrayWithMissing",
                String.format("[{{UNKNOWN}}, {{%s}}, staticText]", CONTEXT_PROPERTY3));
        MODULE_CONFIGURATION.put("complexReferenceObj",
                String.format("{key1: {{%s}}, key2: staticText, key3: {{%s}}}", CONTEXT_PROPERTY1, CONTEXT_PROPERTY4));
        MODULE_CONFIGURATION.put("complexReferenceObjWithMissing",
                String.format("{key1: {{UNKNOWN}}, key2: {{%s}}, key3: {{UNKNOWN2}}}", CONTEXT_PROPERTY2));
        // expected resolved module configuration
        EXPECTED_MODULE_CONFIGURATION.put("simpleReference", CONTEXT.get(CONTEXT_PROPERTY4));
        EXPECTED_MODULE_CONFIGURATION.put("complexReference",
                String.format("Hello %s %s", CONTEXT.get(CONTEXT_PROPERTY1), CONTEXT.get(CONTEXT_PROPERTY4)));
        EXPECTED_MODULE_CONFIGURATION.put("complexReferenceWithMissing",
                String.format("Testing {{UNKNOWN}}, %s", CONTEXT.get(CONTEXT_PROPERTY4)));
        EXPECTED_MODULE_CONFIGURATION.put("complexReferenceArray",
                String.format("[%s, %s, staticText]", CONTEXT.get(CONTEXT_PROPERTY2), CONTEXT.get(CONTEXT_PROPERTY3)));
        EXPECTED_MODULE_CONFIGURATION.put("complexReferenceArrayWithMissing",
                String.format("[{{UNKNOWN}}, %s, staticText]", CONTEXT.get(CONTEXT_PROPERTY3)));
        EXPECTED_MODULE_CONFIGURATION.put("complexReferenceObj", String.format("{key1: %s, key2: staticText, key3: %s}",
                CONTEXT.get(CONTEXT_PROPERTY1), CONTEXT.get(CONTEXT_PROPERTY4)));
        EXPECTED_MODULE_CONFIGURATION.put("complexReferenceObjWithMissing",
                String.format("{key1: {{UNKNOWN}}, key2: %s, key3: {{UNKNOWN2}}}", CONTEXT.get(CONTEXT_PROPERTY2)));
        // composite child module input with references
        COMPOSITE_CHILD_MODULE_INPUTS_REFERENCES.put("moduleInput", String.format("{{%s}}", CONTEXT_PROPERTY1));
        COMPOSITE_CHILD_MODULE_INPUTS_REFERENCES.put("moduleInputMissing", "{{UNKNOWN}}");
        COMPOSITE_CHILD_MODULE_INPUTS_REFERENCES.put("moduleInput2", String.format("{{%s}}", CONTEXT_PROPERTY2));
        // expected resolved child module context
        EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT.put("moduleInput", CONTEXT.get(CONTEXT_PROPERTY1));
        EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT.put("moduleInputMissing", CONTEXT.get("UNKNOWN"));
        EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT.put("moduleInput2", CONTEXT.get(CONTEXT_PROPERTY2));
    public void testModuleConfigurationResolving() {
        // test trigger configuration.
        Module trigger = ModuleBuilder.createTrigger().withId("id1").withTypeUID("typeUID1")
                .withConfiguration(new Configuration(MODULE_CONFIGURATION)).build();
        ReferenceResolver.updateConfiguration(trigger.getConfiguration(), CONTEXT, logger);
        assertEquals(new Configuration(EXPECTED_MODULE_CONFIGURATION), trigger.getConfiguration());
        // test condition configuration.
        Module condition = ModuleBuilder.createCondition().withId("id2").withTypeUID("typeUID2")
        ReferenceResolver.updateConfiguration(condition.getConfiguration(), CONTEXT, logger);
        assertEquals(new Configuration(EXPECTED_MODULE_CONFIGURATION), condition.getConfiguration());
        // test action configuration.
        Module action = ModuleBuilder.createAction().withId("id3").withTypeUID("typeUID3")
        ReferenceResolver.updateConfiguration(action.getConfiguration(), CONTEXT, logger);
        assertEquals(new Configuration(EXPECTED_MODULE_CONFIGURATION), action.getConfiguration());
    public void testModuleInputResolving() {
        // test Composite child ModuleImpl(condition) context
        Module condition = ModuleBuilder.createCondition().withId("id1").withTypeUID("typeUID1")
                .withInputs(COMPOSITE_CHILD_MODULE_INPUTS_REFERENCES).build();
        Map<String, Object> conditionContext = ReferenceResolver.getCompositeChildContext(condition, CONTEXT);
        assertEquals(EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT, conditionContext);
        // test Composite child ModuleImpl(action) context
        Module action = ModuleBuilder.createAction().withId("id2").withTypeUID("typeUID2")
        Map<String, Object> actionContext = ReferenceResolver.getCompositeChildContext(action, CONTEXT);
        assertEquals(EXPECTED_COMPOSITE_CHILD_MODULE_CONTEXT, actionContext);
    public void testSplitReferenceToTokens() {
        assertNull(ReferenceResolver.splitReferenceToTokens(null));
        assertEquals(0, ReferenceResolver.splitReferenceToTokens("").length);
        final String[] referenceTokens = ReferenceResolver
                .splitReferenceToTokens(".module.array[\".na[m}.\"e\"][1].values1");
        assertEquals("module", referenceTokens[0]);
        assertEquals("array", referenceTokens[1]);
        assertEquals(".na[m}.\"e", referenceTokens[2]);
        assertEquals("1", referenceTokens[3]);
        assertEquals("values1", referenceTokens[4]);
    public void testResolvingFromNull() {
        String ken = "Ken";
        assertEquals(ken,
                ReferenceResolver.resolveComplexDataReference(ken, ReferenceResolver.splitReferenceToTokens(null)));
    public void testResolvingFromEmptyString() {
                ReferenceResolver.resolveComplexDataReference(ken, ReferenceResolver.splitReferenceToTokens("")));
    public void testGetFromList() {
        List<String> names = List.of("John", ken, "Sue");
                ReferenceResolver.resolveComplexDataReference(names, ReferenceResolver.splitReferenceToTokens("[1]")));
    public void testGetFromListInvalidIndexFormat() {
        List<String> names = List.of("John", "Ken", "Sue");
        assertThrows(NumberFormatException.class, () -> ReferenceResolver.resolveComplexDataReference(names,
                ReferenceResolver.splitReferenceToTokens("[Ten]")));
    public void getFromMap() {
        String phone = "0331 1387 121";
        Map<String, String> phones = Map.of("John", phone, "Sue", "0222 2184 121", "Mark", "0222 5641 121");
        assertEquals(phone, ReferenceResolver.resolveComplexDataReference(phones,
                ReferenceResolver.splitReferenceToTokens("[\"John\"]")));
    public void getFromMapWithKeyThatContainsSpecialCharacters() {
        Map<String, String> phones = Map.of("John[].Smi\"th].", phone, "Sue", "0222 2184 121", "Mark", "0222 5641 121");
                ReferenceResolver.splitReferenceToTokens("[\"John[].Smi\"th].\"]")));
    public void getFromMapUnExistingKey() {
        Map<String, String> phones = Map.of("Sue", "0222 2184 121", "Mark", "0222 5641 121");
        assertNull(ReferenceResolver.resolveComplexDataReference(phones,
    public void getFromList() {
    public void testGetFromListInvalidIndex() {
        assertThrows(ArrayIndexOutOfBoundsException.class, () -> ReferenceResolver.resolveComplexDataReference(names,
                ReferenceResolver.splitReferenceToTokens("[10]")));
    public void testGetFromInvalidIndexFormat() {
    public void testGetFromBean() {
        String name = "John";
        B1<String> b3 = new B1<>(name);
        assertEquals(name,
                ReferenceResolver.resolveComplexDataReference(b3, ReferenceResolver.splitReferenceToTokens("value")));
    public void testGetFromBeanWithPrivateField() {
        B2<String> b4 = new B2<>(name);
                ReferenceResolver.resolveComplexDataReference(b4, ReferenceResolver.splitReferenceToTokens("value")));
    public void testBeanFromBean() {
        Map<String, String> phones = Map.of("John", phone);
        B1<Map<String, String>> b3 = new B1<>(phones);
        B2<B1<Map<String, String>>> b4 = new B2<>(b3);
        assertEquals(phone, ReferenceResolver.resolveComplexDataReference(b4,
                ReferenceResolver.splitReferenceToTokens("value.value[\"John\"]")));
    @Test()
    public void testGetBeanFieldFromList() {
        B1<String> b31 = new B1<>("Ken");
        B1<String> b32 = new B1<>("Sue");
        B1<String> b33 = new B1<>(name);
        List<B1<String>> b = List.of(b31, b32, b33);
        assertEquals(name, ReferenceResolver.resolveComplexDataReference(b,
                ReferenceResolver.splitReferenceToTokens("[2].value")));
    public static class B1<T> {
        @SuppressWarnings("unused")
        private final T value;
        public B1(T value) {
    public static class B2<T> {
        public T value;
        public B2(T value) {
