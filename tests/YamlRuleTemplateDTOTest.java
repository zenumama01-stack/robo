 * The {@link YamlRuleTemplateDTOTest} contains tests for the {@link YamlRuleTemplateDTO} class.
public class YamlRuleTemplateDTOTest {
        RuleTemplate template = new RuleTemplate("template1", "Foo Template", "Foo rule template", Set.of("test"),
                List.of(), List.of(), List.of(action), List.of(), Visibility.VISIBLE);
        assertNotNull(new YamlRuleTemplateDTO(template));
        template = new RuleTemplate("template1", "Foo Template", "Foo rule template", Set.of("test"), List.of(),
                List.of(condition), List.of(action), List.of(), Visibility.VISIBLE);
        template = new RuleTemplate("template1", "Foo Template", "Foo rule template", Set.of("test"), List.of(trigger),
                List.of(condition), List.of(action),
                List.of(ConfigDescriptionParameterBuilder.create("number", Type.DECIMAL).build()), Visibility.VISIBLE);
        YamlRuleTemplateDTO templateDTO = new YamlRuleTemplateDTO(template);
        assertNotNull(templateDTO);
                "YamlRuleTemplateDTO [uid=template1, label=Foo Template, tags=[test], description=Foo rule template, visibility=VISIBLE, configDescriptions={number=YamlConfigDescriptionParameterDTO [required=false, type=DECIMAL, readOnly=false, multiple=false, advanced=false, verify=false, limitToOptions=true, ]}, conditions=[YamlConditionDTO [inputs={}, id=condition1, type=type1, config={}]], actions=[YamlActionDTO [inputs={}, id=action1, type=type1, config={}]], triggers=[YamlModuleDTO [id=trigger1, type=type1, config={}]]]",
                templateDTO.toString());
        YamlRuleTemplateDTO template = new YamlRuleTemplateDTO();
        assertFalse(template.isValid(null, null));
        template.uid = " ";
        template.label = "Test";
        template.uid = "id";
        template.triggers = new ArrayList<>();
        template.triggers.add(new YamlModuleDTO());
        assertTrue(template.isValid(null, null));
        template.uid = "template:id";
        template.uid = "template:type:@id";
        template.uid = "template:type:id";
        template.uid = "template:type:$subType:id";
        template.uid = "template:type:subType:id";
        template.label = null;
        template.label = "\t";
        template.triggers.clear();
        template.conditions = new ArrayList<>();
        template.conditions.add(new YamlConditionDTO());
        template.conditions.clear();
        template.actions = new ArrayList<>();
        template.actions.add(new YamlActionDTO());
        template.triggers.add(trigger);
        template.triggers.add(trigger2);
        template.conditions.add(condition);
        template.actions.add(action);
        YamlRuleTemplateDTO template1 = new YamlRuleTemplateDTO();
        YamlRuleTemplateDTO template2 = new YamlRuleTemplateDTO();
        assertNotNull(template1);
        assertTrue(template1.equals(template1));
        assertFalse(template1.equals(new Object()));
        assertEquals(template1.hashCode(), template2.hashCode());
        template1.uid = "template:id";
        template2.uid = "template:id2";
        assertFalse(template1.equals(template2));
        assertNotEquals(template1.hashCode(), template2.hashCode());
        template2.uid = "template:id";
        assertTrue(template1.equals(template2));
        template1.label = "A label";
        template2.label = "Another label";
        template2.label = "A label";
        template1.description = "A description";
        template2.description = "Another description";
        template2.description = "A description";
        template1.visibility = Visibility.VISIBLE;
        template2.visibility = Visibility.EXPERT;
        template1.visibility = Visibility.EXPERT;
        template1.tags = new HashSet<>();
        template1.tags.add("Tag1");
        template2.tags = new HashSet<>();
        template2.tags.add("Tag2");
        template2.tags.add("Tag1");
        template2.tags.remove("Tag2");
        template1.actions = new ArrayList<>();
        template1.actions.add(action1);
        template2.actions = new ArrayList<>();
        template2.actions.add(action2);
        template1.conditions = new ArrayList<>();
        template1.conditions.add(condition1);
        template2.conditions = new ArrayList<>();
        template2.conditions.add(condition2);
        template1.triggers = new ArrayList<>();
        template1.triggers.add(trigger1);
        template2.triggers = new ArrayList<>();
        template2.triggers.add(trigger2);
        template1.configDescriptions = Map.of("stateItem", configDescParam1, "iterations", configDescParam2);
        template2.configDescriptions = Map.of("stateItem", configDescParam1, "iterations", configDescParam2);
        template2.configDescriptions = Map.of("stateItem", configDescParam1, "iterations", configDescParam2,
                "assertive", configDescParam3);
