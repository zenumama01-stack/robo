 * The {@link AbstractYamlRuleProviderTest} contains tests for the {@link AbstractYamlRuleProvider} class.
public class AbstractYamlRuleProviderTest {
    public void testExtractModuleIds() {
        AbstractYamlRuleProvider<Rule> provider = new AbstractYamlRuleProvider<Rule>() {
        Set<String> ids = provider.extractModuleIds();
        assertThat(ids, is(empty()));
        ids = provider.extractModuleIds(List.of(TriggerBuilder.create().withTypeUID("testUID").withId("test").build(),
                new Object(), TriggerBuilder.create().withTypeUID("testUID2").withId("   ").build()));
        assertThat(ids, is(hasSize(1)));
    public void testMapModules() throws SerializationException {
        YamlConditionDTO cond1 = new YamlConditionDTO();
        cond1.id = "cond1";
        YamlConditionDTO cond2 = new YamlConditionDTO();
        cond2.id = "cond2";
        assertThrows(SerializationException.class,
                () -> provider.mapModules(List.of(cond1, cond2), Set.of("otherId1", "otherId2"), Condition.class));
        cond1.type = "type1";
        cond2.type = "type2";
        List<Condition> conditions = provider.mapModules(List.of(cond1, cond2), Set.of("otherId1", "otherId2"),
                Condition.class);
        assertThat(conditions, is(hasSize(2)));
        cond1.id = null;
        cond2.id = "   ";
        YamlConditionDTO cond3 = new YamlConditionDTO();
        cond3.id = "4";
        cond3.type = "type3";
        HashSet<String> otherModuleIds = new HashSet<>(Set.of("1", "3", "otherId2"));
        conditions = provider.mapModules(List.of(cond1, cond2, cond3), otherModuleIds, Condition.class);
        assertThat(conditions, is(hasSize(3)));
        assertThat(conditions.get(0).getId(), is("2"));
        assertThat(conditions.get(1).getId(), is("5"));
        assertThat(conditions.get(2).getId(), is("4"));
                () -> provider.mapModules(List.of(cond1, cond2, cond3), otherModuleIds, Action.class));
        YamlModuleDTO trig1 = new YamlModuleDTO();
        YamlModuleDTO trig2 = new YamlModuleDTO();
        trig1.type = "sudden";
        trig2.type = "late";
                () -> provider.mapModules(List.of(trig1, trig2, cond3), otherModuleIds, Condition.class));
