 * The {@link YamlModuleDTOTest} contains tests for the {@link YamlModuleDTO} class.
public class YamlModuleDTOTest {
        Trigger t = TriggerBuilder.create().withId("trigger1").withTypeUID("type1").build();
        YamlModuleDTO trigger = new YamlModuleDTO(t);
        assertThat(trigger, notNullValue());
        assertThat(trigger.id, is("trigger1"));
        assertThat(trigger.type, is("type1"));
        YamlModuleDTO trigger1 = new YamlModuleDTO(t);
        YamlModuleDTO trigger2 = new YamlModuleDTO();
        assertNotEquals(trigger1, trigger2);
        assertEquals(trigger1, trigger1);
        assertEquals(trigger1, new YamlModuleDTO(t));
        assertFalse(trigger2.equals(new Object()));
        assertEquals("YamlModuleDTO [id=trigger1, type=type1, config={}]", trigger1.toString());
        assertEquals("YamlModuleDTO []", trigger2.toString());
        trigger1.label = "Label1";
        trigger1.description = "Description1";
        assertEquals("YamlModuleDTO [id=trigger1, type=type1, label=Label1, description=Description1, config={}]",
                trigger1.toString());
