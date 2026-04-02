import java.text.DecimalFormatSymbols;
 * @author Laurent Garnier - Tests updated to consider multiple AND conditions + tests added for getVisiblity
 * @author Laurent Garnier - Tests added for getCategory
public class ItemUIRegistryImplTest {
    private static TimeZone initialTimeZone;
    // we need to get the decimal separator of the default locale for our tests
    private static final char SEP = (new DecimalFormatSymbols().getDecimalSeparator());
    private static final String ITEM_NAME = "Item";
    private static final String SITEMAP_NAME = "Sitemap";
    private static final String DEFAULT_TIME_ZONE = "GMT-6";
    private @NonNullByDefault({}) ItemUIRegistryImpl uiRegistry;
    private @Mock @NonNullByDefault({}) ItemRegistry registryMock;
    private @Mock @NonNullByDefault({}) SitemapFactory sitemapFactoryMock;
    private @Mock @NonNullByDefault({}) Sitemap sitemapMock;
    private @Mock @NonNullByDefault({}) Widget widgetMock;
    public static void setUpClass() {
        initialTimeZone = TimeZone.getDefault();
    @SuppressWarnings("PMD.SetDefaultTimeZone")
    public static void tearDownClass() {
        // Set the default time zone to its initial value.
        TimeZone.setDefault(initialTimeZone);
    private @Mock @NonNullByDefault({}) Group groupMock;
    private @Mock @NonNullByDefault({}) Text textMock;
    private @Mock @NonNullByDefault({}) Colorpicker colorpickerMock;
    private @Mock @NonNullByDefault({}) Image imageMock;
    private @Mock @NonNullByDefault({}) Mapview mapviewMock;
    private @Mock @NonNullByDefault({}) Slider sliderMock;
    private @Mock @NonNullByDefault({}) Switch switchMock;
    private @Mock @NonNullByDefault({}) Selection selectionMock;
    private @Mock @NonNullByDefault({}) Mapping mappingMock;
        uiRegistry = new ItemUIRegistryImpl(registryMock, sitemapFactoryMock, timeZoneProviderMock);
        when(widgetMock.getItem()).thenReturn(ITEM_NAME);
        when(registryMock.getItem(ITEM_NAME)).thenReturn(itemMock);
        when(timeZoneProviderMock.getTimeZone()).thenReturn(ZoneId.of(DEFAULT_TIME_ZONE));
        TimeZone.setDefault(TimeZone.getTimeZone(DEFAULT_TIME_ZONE));
        setupSitemapFactoryMock();
    private void setupSitemapFactoryMock() {
        when(sitemapFactoryMock.createWidget("Group")).thenReturn(groupMock);
        when(sitemapFactoryMock.createWidget("Text")).thenReturn(textMock);
        when(sitemapFactoryMock.createWidget("Colorpicker")).thenReturn(colorpickerMock);
        when(sitemapFactoryMock.createWidget("Image")).thenReturn(imageMock);
        when(sitemapFactoryMock.createWidget("Mapview")).thenReturn(mapviewMock);
        when(sitemapFactoryMock.createWidget("Slider")).thenReturn(sliderMock);
        when(sitemapFactoryMock.createWidget("Switch")).thenReturn(switchMock);
        when(sitemapFactoryMock.createWidget("Selection")).thenReturn(selectionMock);
        when(sitemapFactoryMock.createMapping()).thenReturn(mappingMock);
    public void getLabelPlainLabel() {
        String testLabel = "This is a plain text";
        when(widgetMock.getLabel()).thenReturn(testLabel);
        assertEquals(testLabel, uiRegistry.getLabel(widgetMock));
        assertEquals(WidgetLabelSource.SITEMAP_WIDGET, uiRegistry.getLabelSource(widgetMock));
    public void getLabelLabelWithStaticValue() {
        String testLabel = "Label [value]";
        String label = uiRegistry.getLabel(widgetMock);
        assertEquals("Label [value]", label);
    public void getLabelLabelWithStringValue() {
        String testLabel = "Label [%s]";
        when(itemMock.getState()).thenReturn(new StringType("State"));
        assertEquals("Label [State]", label);
    public void getLabelLabelWithStringValueFunction() {
        when(itemMock.getState()).thenReturn(new StringType("foo(x):y"));
        assertEquals("Label [foo(x):y]", label);
    public void getLabelLabelWithoutPatterAndIntegerValue() {
        String testLabel = "Label";
        when(itemMock.getState()).thenReturn(new DecimalType(20));
        when(itemMock.getStateAs(DecimalType.class)).thenReturn(new DecimalType(20));
        when(itemMock.getStateDescription())
                .thenReturn(StateDescriptionFragmentBuilder.create().withPattern("%d").build().toStateDescription());
        assertEquals("Label [20]", label);
    public void getLabelLabelWithoutPatterAndFractionalDigitsValue() {
        when(itemMock.getState()).thenReturn(new DecimalType(20.5));
        when(itemMock.getStateAs(DecimalType.class)).thenReturn(new DecimalType(20.5));
        assertEquals("Label [21]", label);
    public void getLabelLabelWithIntegerValue() {
        String testLabel = "Label [%d]";
    public void getLabelLabelWithFractionalDigitsValue() {
    public void getLabelLabelWithIntegerValueAndWidth() {
        String testLabel = "Label [%3d]";
        assertEquals("Label [ 20]", label);
    public void getLabelLabelWithHexValueAndWidth() {
        String testLabel = "Label [%3x]";
        assertEquals("Label [ 14]", label);
    public void getLabelLabelWithDecimalValue() {
        String testLabel = "Label [%.3f]";
        when(itemMock.getState()).thenReturn(new DecimalType(10f / 3f));
        when(itemMock.getStateAs(DecimalType.class)).thenReturn(new DecimalType(10f / 3f));
        assertEquals("Label [3" + SEP + "333]", label);
    public void getLabelLabelWithDecimalValueAndUnitUpdatedWithQuantityType() {
        String testLabel = "Label [%.3f " + UnitUtils.UNIT_PLACEHOLDER + "]";
        when(itemMock.getState()).thenReturn(new QuantityType<>(10f / 3f + " °C"));
        assertEquals("Label [3" + SEP + "333 °C]", label);
    public void getLabelLabelWithDecimalValueAndUnitUpdatedWithDecimalType() {
    public void getLabelLabelWithDecimalValueAndUnit2() {
        String testLabel = "Label [%.0f " + UnitUtils.UNIT_PLACEHOLDER + "]";
        assertEquals("Label [3 °C]", label);
    public void getLabelLabelWithDecimalValueAndUnit3() {
        String testLabel = "Label [%d %%]";
        when(itemMock.getState()).thenReturn(new QuantityType<>(10f / 3f + " %"));
        assertEquals("Label [3 %]", label);
    public void getLabelLabelWithDecimalValueAndUnit4() {
        String testLabel = "Label [%.0f %%]";
    public void getLabelLabelWithDecimalValueAndUnit5() {
        String testLabel = "Label [%d " + UnitUtils.UNIT_PLACEHOLDER + "]";
        when(itemMock.getState()).thenReturn(new QuantityType<>("33 %"));
        assertEquals("Label [33 %]", label);
    public void getLabelLabelWithFractionalDigitsValueAndUnit5() {
    public void getLabelLabelWithDecimalValueAndUnit6() {
    public void getLabelLabelWithDecimalValueAndUnit7() {
    public void getLabelLabelWithDecimalValueAndUnitConversion() {
        String testLabel = "Label [%.2f °F]";
        when(itemMock.getState()).thenReturn(new QuantityType<>("22 °C"));
        assertEquals("Label [71" + SEP + "60 °F]", label);
    public void getLabelLabelWithPercent() {
        String testLabel = "Label [%.1f %%]";
        assertEquals("Label [3" + SEP + "3 %]", label);
    public void getLabelLabelWithPercentType() {
        when(itemMock.getState()).thenReturn(new PercentType(42));
        assertEquals("Label [42 %]", label);
    public void getLabelLabelWithDate() {
        String testLabel = "Label [%1$td.%1$tm.%1$tY]";
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00"));
        assertEquals("Label [01.06.2011]", label);
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00Z"));
        label = uiRegistry.getLabel(widgetMock);
        assertEquals("Label [31.05.2011]", label);
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00+02"));
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00-06"));
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00-07"));
    public void getLabelLabelWithZonedDate() throws ItemNotFoundException {
        when(w.getLabel()).thenReturn(testLabel);
        when(registryMock.getItem(ITEM_NAME)).thenReturn(item);
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00"));
        String label = uiRegistry.getLabel(w);
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00Z"));
        label = uiRegistry.getLabel(w);
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00+02"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00-06"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T00:00:00-07"));
    public void getLabelLabelWithTime() {
        String testLabel = "Label [%1$tT]";
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59"));
        assertEquals("Label [15:30:59]", label);
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59Z"));
        assertEquals("Label [09:30:59]", label);
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59+02"));
        assertEquals("Label [07:30:59]", label);
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59-06"));
        when(itemMock.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59-07"));
        assertEquals("Label [16:30:59]", label);
    public void getLabelLabelWithZonedTime() throws ItemNotFoundException {
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59Z"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59+02"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59-06"));
        when(item.getState()).thenReturn(new DateTimeType("2011-06-01T15:30:59-07"));
    public void getLabelWidgetWithoutLabelAndItem() {
        assertEquals("", uiRegistry.getLabel(w));
        assertEquals(WidgetLabelSource.NONE, uiRegistry.getLabelSource(w));
    public void getLabelWidgetWithoutLabel() {
        assertEquals(ITEM_NAME, uiRegistry.getLabel(widgetMock));
        assertEquals(WidgetLabelSource.ITEM_NAME, uiRegistry.getLabelSource(widgetMock));
    public void getLabelLabelFromUIProvider() {
        ItemUIProvider provider = mock(ItemUIProvider.class);
        uiRegistry.addItemUIProvider(provider);
        when(provider.getLabel(anyString())).thenReturn("ProviderLabel");
        assertEquals("ProviderLabel", uiRegistry.getLabel(widgetMock));
        assertEquals(WidgetLabelSource.ITEM_LABEL, uiRegistry.getLabelSource(widgetMock));
        uiRegistry.removeItemUIProvider(provider);
    public void getLabelLabelForUndefinedStringItemState() {
        assertEquals("Label [-]", label);
    public void getLabelLabelForUndefinedIntegerItemState() {
    public void getLabelLabelForUndefinedDecimalItemState() {
        String testLabel = "Label [%.2f]";
    public void getLabelLabelForUndefinedDateItemState() {
        assertEquals("Label [-.-.-]", label);
    public void getLabelLabelForUndefinedQuantityItemState() {
        String testLabel = "Label [%.2f " + UnitUtils.UNIT_PLACEHOLDER + "]";
        assertEquals("Label [- -]", label);
    public void getLabelItemNotFound() throws ItemNotFoundException {
        when(widgetMock.getWidgetType()).thenReturn("Text");
        when(registryMock.getItem(ITEM_NAME)).thenThrow(new ItemNotFoundException(ITEM_NAME));
    public void getLabelLabelWithFunctionValue() {
        String testLabel = "Label [MAP(de.map):%s]";
    public void getLabelGroupLabelWithValue() {
        when(itemMock.getState()).thenReturn(OnOffType.ON);
        when(itemMock.getStateAs(DecimalType.class)).thenReturn(new DecimalType(5));
        assertEquals("Label [5]", label);
    public void getWidgetUnknownPageId() throws ItemNotFoundException {
        when(sitemapFactoryMock.createSitemap(SITEMAP_NAME)).thenReturn(sitemapMock);
        when(sitemapMock.getWidgets()).thenReturn(List.of());
        Sitemap sitemap = sitemapFactoryMock.createSitemap(SITEMAP_NAME);
        when(registryMock.getItem("unknown")).thenThrow(new ItemNotFoundException("unknown"));
        Widget w = uiRegistry.getWidget(sitemap, "unknown");
        assertNull(w);
    public void testFormatDefault() {
        assertEquals("Server [(-)]", uiRegistry.formatUndefined("Server [(%d)]"));
        assertEquals("Anruf [von - an -]", uiRegistry.formatUndefined("Anruf [von %2$s an %1$s]"));
        assertEquals("Zeit [-.-.- -]", uiRegistry.formatUndefined("Zeit [%1$td.%1$tm.%1$tY %1$tT]"));
        assertEquals("Temperatur [- °C]", uiRegistry.formatUndefined("Temperatur [%.1f °C]"));
        assertEquals("Luftfeuchte [- %]", uiRegistry.formatUndefined("Luftfeuchte [%.1f %%]"));
    public void testStateConversionForSwitchWidgetThroughGetState() throws ItemNotFoundException {
        State colorState = new HSBType("23,42,50");
        ColorItem colorItem = new ColorItem("myItem");
        colorItem.setLabel("myItem");
        colorItem.setState(colorState);
        when(registryMock.getItem("myItem")).thenReturn(colorItem);
        Switch switchWidget = mock(Switch.class);
        when(switchWidget.getItem()).thenReturn("myItem");
        when(switchWidget.getMappings()).thenReturn(new ArrayList<>());
        State stateForSwitch = uiRegistry.getState(switchWidget);
        assertEquals(OnOffType.ON, stateForSwitch);
    public void testStateConversionForSwitchWidgetWithMappingThroughGetState() throws ItemNotFoundException {
        Mapping mapping = mock(Mapping.class);
        List<Mapping> mappings = new ArrayList<>();
        when(switchWidget.getMappings()).thenReturn(mappings);
        assertEquals(colorState, stateForSwitch);
    public void testStateConversionForSliderWidgetThroughGetState() throws ItemNotFoundException {
        State colorState = new HSBType("23,42,75");
        Slider sliderWidget = mock(Slider.class);
        when(sliderWidget.getItem()).thenReturn("myItem");
        State stateForSlider = uiRegistry.getState(sliderWidget);
        assertInstanceOf(PercentType.class, stateForSlider);
        PercentType pt = (PercentType) stateForSlider;
        assertEquals(75, pt.longValue());
    public void getLabelLabelWithoutStateDescription() {
        when(itemMock.getStateDescription()).thenReturn(null);
        assertEquals("Label", label);
    public void getLabelLabelWithoutPatternInStateDescription() {
        StateDescription stateDescription = mock(StateDescription.class);
        when(itemMock.getStateDescription()).thenReturn(stateDescription);
        when(stateDescription.getPattern()).thenReturn(null);
    public void getLabelLabelWithPatternInStateDescription() {
        when(stateDescription.getPattern()).thenReturn("%s");
    public void getLabelLabelWithEmptyPattern() {
        String testLabel = "Label []";
    public void getLabelStringItemLabelWithMappedOption() {
        List<StateOption> options = new ArrayList<>();
        options.add(new StateOption("State0", "This is the state 0"));
        options.add(new StateOption("State1", "This is the state 1"));
        when(stateDescription.getOptions()).thenReturn(options);
        when(itemMock.getState()).thenReturn(new StringType("State1"));
        assertEquals("Label [This is the state 1]", label);
    public void getLabelStringItemLabelWithUnmappedOption() {
    public void getLabelStringItemLabelWithMappedOptionButInappropriatePattern() {
        when(stateDescription.getPattern()).thenReturn("Value: %f");
        when(itemMock.getState()).thenReturn(new StringType("State0"));
        assertEquals("Label [This is the state 0]", label);
    public void getLabelNumberItemLabelWithMappedOption() {
        options.add(new StateOption("0", "This is the state number 0"));
        options.add(new StateOption("1", "This is the state number 1"));
        when(itemMock.getState()).thenReturn(new DecimalType(1));
        assertEquals("Label [This is the state number 1]", label);
    public void getLabelNumberItemLabelWithUnmappedOption() {
        when(itemMock.getState()).thenReturn(new DecimalType(2));
        assertEquals("Label [2]", label);
    public void getLabelNumberItemLabelWithMappedOptionButInappropriatePattern() {
        when(itemMock.getState()).thenReturn(new DecimalType(0));
        assertEquals("Label [This is the state number 0]", label);
        when(stateDescription.getPattern()).thenReturn("Value: %d");
    public void getLabelTransformationContainingPercentS() throws ItemNotFoundException {
        // It doesn't matter that "FOO" doesn't exist - this is to assert it doesn't fail before because of the two "%s"
        String testLabel = "Memory [FOO(echo %s):%s]";
        when(item.getState()).thenReturn(new StringType("State"));
        assertEquals("Memory [State]", label);
    public void getLabelFailingTransformation() throws ItemNotFoundException {
        String testLabel = "Memory [FOO(echo %s):__%d__]";
        when(item.getState()).thenReturn(new DecimalType(11));
        assertEquals("Memory [11]", label);
    public void getLabelFailingTransformationWithNullState() throws ItemNotFoundException {
        when(item.getState()).thenReturn(UnDefType.NULL);
        assertEquals("Memory [-]", label);
    public void getLabelFailingTransformationWithUndefState() throws ItemNotFoundException {
        when(item.getState()).thenReturn(UnDefType.UNDEF);
    public void getLabelColorLabelWithDecimalValue() {
        Condition condition = mock(Condition.class);
        when(condition.getValue()).thenReturn("21");
        when(condition.getCondition()).thenReturn("<");
        Rule rule = mock(Rule.class);
        when(rule.getConditions()).thenReturn(conditions);
        when(rule.getArgument()).thenReturn("yellow");
        List<Rule> rules = new ArrayList<>();
        when(widgetMock.getLabelColor()).thenReturn(rules);
        String color = uiRegistry.getLabelColor(widgetMock);
        assertEquals("yellow", color);
        when(itemMock.getState()).thenReturn(new DecimalType(21f));
        color = uiRegistry.getLabelColor(widgetMock);
        assertNull(color);
    public void getLabelColorLabelWithUnitValue() {
        when(condition.getValue()).thenReturn("20");
        when(condition.getCondition()).thenReturn("==");
        when(itemMock.getState()).thenReturn(new QuantityType<>("20 °C"));
        when(itemMock.getState()).thenReturn(new QuantityType<>("20.1 °C"));
    public void getDefaultWidgets() {
        Widget defaultWidget = uiRegistry.getDefaultWidget(GroupItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Group.class)));
        defaultWidget = uiRegistry.getDefaultWidget(CallItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Text.class)));
        defaultWidget = uiRegistry.getDefaultWidget(ColorItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Colorpicker.class)));
        defaultWidget = uiRegistry.getDefaultWidget(ContactItem.class, ITEM_NAME);
        defaultWidget = uiRegistry.getDefaultWidget(DateTimeItem.class, ITEM_NAME);
        defaultWidget = uiRegistry.getDefaultWidget(DimmerItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Slider.class)));
        verify(sliderMock).setSwitchEnabled(true);
        verify(sliderMock).setReleaseOnly(true);
        defaultWidget = uiRegistry.getDefaultWidget(ImageItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Image.class)));
        defaultWidget = uiRegistry.getDefaultWidget(LocationItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Mapview.class)));
        defaultWidget = uiRegistry.getDefaultWidget(RollershutterItem.class, ITEM_NAME);
        assertThat(defaultWidget, is(instanceOf(Switch.class)));
        defaultWidget = uiRegistry.getDefaultWidget(SwitchItem.class, ITEM_NAME);
        when(switchMock.getMappings()).thenReturn(new ArrayList<Mapping>());
        defaultWidget = uiRegistry.getDefaultWidget(PlayerItem.class, ITEM_NAME);
        assertThat(((Switch) defaultWidget).getMappings(), hasSize(4));
    public void getDefaultWidgetsForNumberItem() {
        // NumberItem without CommandOptions or StateOptions should return Text element
        Widget defaultWidget = uiRegistry.getDefaultWidget(NumberItem.class, ITEM_NAME);
        // NumberItem with one to four CommandOptions should return Switch element
        final CommandDescriptionBuilder builder = CommandDescriptionBuilder.create().withCommandOptions(
                List.of(new CommandOption("command1", "label1"), new CommandOption("command2", "label2"),
                        new CommandOption("command3", "label3"), new CommandOption("command4", "label4")));
        when(itemMock.getCommandDescription()).thenReturn(builder.build());
        defaultWidget = uiRegistry.getDefaultWidget(NumberItem.class, ITEM_NAME);
        // NumberItem with more than four CommandOptions should return Selection element
        builder.withCommandOption(new CommandOption("command5", "label5"));
        assertThat(defaultWidget, is(instanceOf(Selection.class)));
        // NumberItem with one or more StateOptions should return Selection element
        when(itemMock.getStateDescription()).thenReturn(StateDescriptionFragmentBuilder.create()
                .withOptions(List.of(new StateOption("value1", "label1"), new StateOption("value2", "label2"))).build()
                .toStateDescription());
        // Read-only NumberItem with one or more StateOptions should return Text element
                .thenReturn(StateDescriptionFragmentBuilder.create().withReadOnly(Boolean.TRUE)
                        .withOptions(List.of(new StateOption("value1", "label1"), new StateOption("value2", "label2")))
                        .build().toStateDescription());
    public void getDefaultWidgetsForStringItem() {
        // StringItem without CommandOptions or StateOptions should return Text element
        Widget defaultWidget = uiRegistry.getDefaultWidget(StringItem.class, ITEM_NAME);
        // StringItem with one to four CommandOptions should return Switch element
        defaultWidget = uiRegistry.getDefaultWidget(StringItem.class, ITEM_NAME);
        // StringItem with more than four CommandOptions should return Selection element
        // StringItem with one or more StateOptions should return Selection element
        // Read-only StringItem with one or more StateOptions should return Text element
    public void getUnitForWidgetForNonNumberItem() throws Exception {
        String unit = uiRegistry.getUnitForWidget(widgetMock);
        assertThat(unit, is(""));
    public void getUnitForWidgetWithWidgetLabel() throws Exception {
        // a NumberItem having a Dimension must be returned
        NumberItem item = mock(NumberItem.class);
        doReturn(Temperature.class).when(item).getDimension();
        // we set the Label on the widget itself
        when(widgetMock.getLabel()).thenReturn("Label [%.1f °C]");
        assertThat(unit, is(equalTo("°C")));
    public void getUnitForWidgetWithItemLabelAndWithoutWidgetLabel() throws Exception {
        // we set the UnitSymbol on the item, this must be used as a fallback if no Widget label was used
        when(item.getUnitSymbol()).thenReturn("°C");
    public void getLabelColorDefaultColor() {
        when(condition.getValue()).thenReturn("18");
        when(condition.getCondition()).thenReturn(">=");
        Condition condition2 = mock(Condition.class);
        when(condition2.getValue()).thenReturn("21");
        when(condition2.getCondition()).thenReturn("<");
        conditions.add(condition2);
        Condition condition3 = mock(Condition.class);
        when(condition3.getValue()).thenReturn("21");
        when(condition3.getCondition()).thenReturn(">=");
        Condition condition4 = mock(Condition.class);
        when(condition4.getValue()).thenReturn("24");
        when(condition4.getCondition()).thenReturn("<");
        conditions2.add(condition3);
        conditions2.add(condition4);
        Rule rule2 = mock(Rule.class);
        when(rule2.getConditions()).thenReturn(conditions2);
        when(rule2.getArgument()).thenReturn("red");
        rules.add(rule2);
        List<Condition> conditions5 = new ArrayList<>();
        Rule rule3 = mock(Rule.class);
        when(rule3.getConditions()).thenReturn(conditions5);
        when(rule3.getArgument()).thenReturn("blue");
        rules.add(rule3);
        when(itemMock.getState()).thenReturn(new DecimalType(20.9));
        when(itemMock.getState()).thenReturn(new DecimalType(23.5));
        assertEquals("red", color);
        when(itemMock.getState()).thenReturn(new DecimalType(24.0));
        assertEquals("blue", color);
        when(itemMock.getState()).thenReturn(new DecimalType(17.5));
    public void getValueColor() {
        when(widgetMock.getValueColor()).thenReturn(rules);
        String color = uiRegistry.getValueColor(widgetMock);
        color = uiRegistry.getValueColor(widgetMock);
    public void getIconColor() {
        when(widgetMock.getIconColor()).thenReturn(rules);
        String color = uiRegistry.getIconColor(widgetMock);
        color = uiRegistry.getIconColor(widgetMock);
    public void getVisibility() {
        when(condition2.getValue()).thenReturn("24");
        when(widgetMock.getVisibility()).thenReturn(rules);
        assertFalse(uiRegistry.getVisiblity(widgetMock));
        when(itemMock.getState()).thenReturn(new DecimalType(21.0));
        assertTrue(uiRegistry.getVisiblity(widgetMock));
    public void getCategoryWhenIconSetWithoutRules() {
        when(widgetMock.getIcon()).thenReturn("temperature");
        when(widgetMock.isStaticIcon()).thenReturn(false);
        when(widgetMock.getIconRules()).thenReturn(List.of());
        String icon = uiRegistry.getCategory(widgetMock);
        assertEquals("temperature", icon);
    public void getCategoryWhenIconSetWithRules() {
        when(widgetMock.getIcon()).thenReturn(null);
        when(rule.getArgument()).thenReturn("temperature");
        when(rule2.getArgument()).thenReturn("humidity");
        when(widgetMock.getIconRules()).thenReturn(rules);
        assertEquals("humidity", icon);
        icon = uiRegistry.getCategory(widgetMock);
    public void getCategoryWhenStaticIconSet() {
        when(widgetMock.isStaticIcon()).thenReturn(true);
    public void getCategoryWhenIconSetOnItem() {
        when(itemMock.getCategory()).thenReturn("temperature");
    public void getCategoryDefaultIcon() {
        when(itemMock.getCategory()).thenReturn(null);
        assertEquals("text", icon);
