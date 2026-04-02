 * The {@link YamlConditionDTOTest} contains tests for the {@link YamlConditionDTO} class.
public class YamlConditionDTOTest {
        Condition c = ConditionBuilder.create().withId("condition1").withTypeUID("type1").build();
        YamlConditionDTO condition = new YamlConditionDTO(c);
        assertThat(condition, notNullValue());
        assertThat(condition.id, is("condition1"));
        assertThat(condition.type, is("type1"));
        YamlConditionDTO condition1 = new YamlConditionDTO(c);
        YamlConditionDTO condition2 = new YamlConditionDTO();
        assertNotEquals(condition1, condition2);
        assertEquals(condition1, condition1);
        assertEquals(condition1, new YamlConditionDTO(c));
        assertFalse(condition2.equals(new YamlModuleDTO()));
        assertEquals("YamlConditionDTO [inputs={}, id=condition1, type=type1, config={}]", condition1.toString());
        assertEquals("YamlConditionDTO []", condition2.toString());
        condition1.label = "Label1";
        condition1.description = "Description1";
                "YamlConditionDTO [inputs={}, id=condition1, type=type1, label=Label1, description=Description1, config={}]",
                condition1.toString());
