 * The {@link YamlActionDTOTest} contains tests for the {@link YamlActionDTO} class.
public class YamlActionDTOTest {
    public void testConstructor() {
        Action a = ActionBuilder.create().withId("action1").withTypeUID("type1").build();
        YamlActionDTO action = new YamlActionDTO(a);
        assertThat(action, notNullValue());
        assertThat(action.id, is("action1"));
        assertThat(action.type, is("type1"));
        YamlActionDTO action1 = new YamlActionDTO(a);
        YamlActionDTO action2 = new YamlActionDTO();
        assertNotEquals(action1, action2);
        assertEquals(action1, action1);
        assertEquals(action1, new YamlActionDTO(a));
        assertFalse(action2.equals(new YamlModuleDTO()));
    public void testToString() {
        assertEquals("YamlActionDTO [inputs={}, id=action1, type=type1, config={}]", action1.toString());
        assertEquals("YamlActionDTO []", action2.toString());
        action1.label = "Label1";
        action1.description = "Description1";
                "YamlActionDTO [inputs={}, id=action1, type=type1, label=Label1, description=Description1, config={}]",
                action1.toString());
