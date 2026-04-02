 * Tests parsing of specific JSON rule template files.
public class TemplateGSONParserTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/rule-templates");
    public void basicTemplateTest() throws Exception {
        TemplateGSONParser parser = new TemplateGSONParser();
        Set<Template> templates;
                Files.newInputStream(SOURCE_PATH.resolve("BasicRuleTemplate.json"), StandardOpenOption.READ))) {
            templates = parser.parse(isr);
        assertThat(templates, hasSize(1));
        RuleTemplate template = (RuleTemplate) templates.iterator().next();
        assertThat(template.getUID(), is("basic:json-rule-template"));
        assertThat(template.getLabel(), is("Basic JSON Rule Template"));
        assertThat(template.getDescription(), is("A basic JSON rule template."));
        assertThat(template.getVisibility(), is(Visibility.VISIBLE));
        List<ConfigDescriptionParameter> configDescriptions = template.getConfigurationDescriptions();
        assertThat(configDescriptions, hasSize(1));
        assertThat(parameter.getName(), is("startLevel"));
        assertThat(parameter.getLabel(), is("Start Level"));
        assertThat(parameter.getDescription(), is("The start level which will trigger the rule."));
        assertThat(parameter.getDefault(), is("80"));
        assertThat(parameter.getMultipleLimit(), is(nullValue()));
        assertThat(parameter.getGroupName(), is(emptyOrNullString()));
        assertTrue(parameter.getLimitToOptions());
        assertThat(parameter.getOptions(), is(empty()));
        assertThat(parameter.getFilterCriteria(), is(empty()));
        List<Trigger> triggers = template.getTriggers();
        assertThat(trigger.getId(), is("3"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("startlevel", "{{startLevel}}"));
        assertThat(trigger.getId(), is("timeofday"));
        List<Condition> conditions = template.getConditions();
        assertThat(condition.getId(), is("5"));
        List<Action> actions = template.getActions();
        assertThat(action.getId(), is("2"));
    public void fullTemplateTest() throws Exception {
                Files.newInputStream(SOURCE_PATH.resolve("FullRuleTemplate.json"), StandardOpenOption.READ))) {
        assertThat(template.getUID(), is("test:json-full-rule-template"));
        assertThat(template.getLabel(), is("JSON Full Rule Template"));
        assertThat(template.getDescription(), is("The description of the JSON template-based full rule"));
        assertThat(parameter.getPattern(), is(nullValue()));
        assertThat(options.getFirst().getValue(), is("Welcome"));
        assertThat(options.get(1).getValue(), is("Willkommen"));
        assertThat(parameter.getDefault(), is("70"));
        assertThat(parameter.getMinimum(), is(BigDecimal.valueOf(60L)));
        assertThat(parameter.getMaximum(), is(BigDecimal.valueOf(100L)));
        assertThat(parameter.getUnit(), is("%"));
        assertThat(parameter.getUnitLabel(), is("Start Level"));
        assertThat(parameter.getDefault(), is("CurrentPower"));
        assertThat(template.getTags(), hasSize(2));
        assertThat(template.getTags(), hasItem("First Tag"));
        assertThat(template.getTags(), hasItem("Second Tag"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("startlevel", "{{integerParam}}"));
        assertThat(condition.getConfiguration().getProperties(), hasEntry("itemName", "{{itemParam}}"));
        assertThat(action.getConfiguration().getProperties(), hasEntry("text", "{{textParam}}"));
    public void multipleTemplatesTest() throws Exception {
                Files.newInputStream(SOURCE_PATH.resolve("MultipleRuleTemplates.json"), StandardOpenOption.READ))) {
        assertThat(templates, hasSize(2));
        Iterator<Template> iterator = templates.iterator();
        RuleTemplate template = (RuleTemplate) iterator.next();
        assertThat(template.getUID(), is("kaikreuzer:energymeter-json"));
        assertThat(template.getLabel(), is("Energy Meter JSON"));
        assertThat(template.getDescription(), is("Visualizes the current energy consumption."));
        assertThat(configDescriptions, hasSize(3));
        assertThat(parameter.getName(), is("consumption"));
        assertThat(parameter.getLabel(), is("Consumption Item"));
        assertThat(parameter.getDescription(), is("Data source for current consumption"));
        assertThat(options, is(empty()));
        assertThat(filterCriterias, hasSize(1));
        assertThat(filterCriterias.getFirst().getName(), is("type"));
        assertThat(filterCriterias.getFirst().getValue(), is("Number"));
        assertThat(parameter.getName(), is("light"));
        assertThat(parameter.getLabel(), is("Color Item"));
        assertThat(parameter.getDescription(), is("Color light to use for visualisation"));
        assertThat(parameter.getDefault(), is(nullValue()));
        assertThat(parameter.getMinimum(), is(nullValue()));
        assertThat(parameter.getMaximum(), is(nullValue()));
        assertThat(parameter.getStepSize(), is(nullValue()));
        assertThat(parameter.getUnit(), is(nullValue()));
        assertThat(parameter.getUnitLabel(), is(nullValue()));
        assertThat(parameter.getName(), is("max"));
        assertThat(parameter.getLabel(), is("Max. consumption"));
        assertThat(parameter.getDescription(), is("Maximum value for red light"));
        assertThat(parameter.getDefault(), is("1500"));
        assertThat(parameter.getContext(), is(nullValue()));
        assertThat(template.getTags(), is(empty()));
        assertThat(triggers, hasSize(1));
        assertThat(trigger.getId(), is("trigger"));
        assertThat(trigger.getLabel(), is("Current consumption changes"));
        assertThat(trigger.getDescription(), is("Triggers whenever the current consumption changes its value"));
        assertThat(trigger.getTypeUID(), is("core.ItemStateChangeTrigger"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("itemName", "{{consumption}}"));
        assertThat(actions, hasSize(1));
        assertThat(action.getId(), is("setcolor"));
        assertThat(action.getLabel(), is("Change the light color"));
        assertThat(action.getDescription(),
                is("Sets the color to a value in the range from green (low consumption) to red (high consumption)"));
        assertThat(action.getConfiguration().getProperties(), hasEntry("type", "application/vnd.openhab.dsl.rule"));
        assertThat(action.getConfiguration().getProperties(),
                hasEntry("script",
                        "var power = (newState as Number).intValue\n" + "var percent = power / (30.0 / 100.0)\n"
                                + "if(percent < 0) percent = 0\n" + "var hue = 120 - percent * 1.2\n"
                                + "sendCommand({{light}}, hue +',100,100')"));
        template = (RuleTemplate) iterator.next();
        assertThat(template.getUID(), is("ysc:simulate_sunrise_json"));
        assertThat(template.getLabel(), is("Simulate Sunrise JSON"));
        assertThat(template.getDescription(), is(
                "This rule will gradually increase a Dimmer or Color item to the target brightness and time over a configurable period."));
        assertThat(template.getVisibility(), is(Visibility.EXPERT));
        configDescriptions = template.getConfigurationDescriptions();
        assertThat(configDescriptions, hasSize(6));
        parameter = configDescriptions.getFirst();
        assertThat(parameter.getName(), is("itemTargetTime"));
        assertThat(parameter.getLabel(), is("Target Time (DateTime Item)"));
        assertThat(parameter.getDescription(), is(
                "DateTime Item that holds the target time (for instance, linked to the Sunrise End Time channel of an Astro Sun Thing). Set either this or a fixed target time below."));
        assertThat(parameter.getName(), is("fixedTargetTime"));
        assertThat(parameter.getLabel(), is("Fixed Target Time"));
        assertThat(parameter.getDescription(),
                is("Set a fixed target time - ignored if Target Time (DateTime Item) is set above."));
        assertThat(parameter.getContext(), is("time"));
        assertThat(parameter.getName(), is("targetBrightness"));
        assertThat(parameter.getLabel(), is("Target Brightness"));
        assertThat(parameter.getDescription(), is("Brightness to reach at the target time."));
        assertThat(parameter.getDefault(), is("100"));
        assertThat(parameter.getName(), is("sunriseDuration"));
        assertThat(parameter.getLabel(), is("Sunrise Duration"));
                "Duration of the sunrise in minutes (The brightness will be set to 0 at the start of the period and gradually every minute to the target brightness until the end)."));
        assertThat(parameter.getDefault(), is("60"));
        assertThat(parameter.getName(), is("brightnessItem"));
        assertThat(parameter.getLabel(), is("Brightness Item"));
        assertThat(parameter.getDescription(), is("Dimmer or Color Item to use to control the brightness."));
        parameter = configDescriptions.get(5);
        assertThat(parameter.getName(), is("colorPrefix"));
        assertThat(parameter.getLabel(), is("Color Prefix"));
                "In case of a Color Item set above, prefix the command with the comma-separated Hue,Saturation components to send to the item (a separator comma and the brightness will be appended)."));
        assertThat(template.getTags(), hasSize(1));
        assertThat(template.getTags(), hasItem("Astro"));
        triggers = template.getTriggers();
        trigger = triggers.getFirst();
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("cronExpression", "0 * * * * ? *"));
        conditions = template.getConditions();
        actions = template.getActions();
        action = actions.getFirst();
        assertThat(action.getLabel(), is("Calculate & set the target brightness"));
                is("Sets the brightness appropriately or do nothing if outside the sunrise time"));
        assertThat(action.getConfiguration().getProperties(), hasEntry("type", "application/javascript"));
        assertThat(action.getConfiguration().getProperties(), hasEntry("script",
                "// set by the rule template\nvar itemTargetTime = \"{{itemTargetTime}}\";\nvar fixedTargetTime = \"{{fixedTargetTime}}\";\nvar sunriseDuration = {{sunriseDuration}};\nvar targetBrightness = {{targetBrightness}};\nvar brightnessItem = \"{{brightnessItem}}\";\nvar colorPrefix = \"{{colorPrefix}}\";\n\nvar openhab = (typeof(require) === \"function\") ? require(\"@runtime\") : {\n  ir: ir, events: events\n};\n\nvar logger = Java.type(\"org.slf4j.LoggerFactory\").getLogger(\"org.openhab.rule.\" + this.ctx.ruleUID);\n\n// returns the number of minutes past midnight for a Date object\nfunction getMinutesPastMidnight(date) {\n  return date.getHours() * 60 + date.getMinutes();\n}\n\n\n// returns the brightness to set at the current time (Date), given the target time (Date),\n// target brightness (int) & desired sunrise duration (int)\n\nfunction getBrightnessAtTime(currentTime, targetTime, targetBrightness, sunriseDuration) {\n  var currentMinutes = getMinutesPastMidnight(now);\n  var targetMinutes = getMinutesPastMidnight(targetTime);\n  if (currentMinutes > targetMinutes) return null;\n  if (currentMinutes < targetMinutes - sunriseDuration) return null;\n  var minutesToGo = targetMinutes - currentMinutes;\n  return parseInt(parseInt(targetBrightness) * ((sunriseDuration - minutesToGo) / sunriseDuration));\n}\n\nvar now = new Date();\nvar targetTime = null;\n\nif (itemTargetTime) {\n  targetTime = new Date(openhab.ir.getItem(itemTargetTime).getState());\n} else if (fixedTargetTime.match(/\\d\\d:\\d\\d/)) {\n  targetTime = new Date();\n  targetTime.setHours(parseInt(fixedTargetTime.split(\":\")[0]));\n  targetTime.setMinutes(parseInt(fixedTargetTime.split(\":\")[1]));\n  targetTime.setSeconds(0);\n} else {\n  logger.warn(\"Invalid target time\");\n}\n\nif (targetTime != null) {\n  var brightness = getBrightnessAtTime(now, targetTime, targetBrightness, sunriseDuration);\n  if (brightness != null) {\n    openhab.events.sendCommand(brightnessItem, (colorPrefix ? colorPrefix + \",\" : \"\") + brightness.toString());\n  }\n}\n"));
