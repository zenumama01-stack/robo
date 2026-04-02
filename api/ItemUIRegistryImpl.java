package org.openhab.core.ui.internal.items;
import java.util.WeakHashMap;
import org.openhab.core.library.items.CallItem;
import org.openhab.core.library.items.ColorItem;
import org.openhab.core.library.items.ContactItem;
import org.openhab.core.library.items.ImageItem;
import org.openhab.core.library.items.LocationItem;
import org.openhab.core.ui.items.ItemUIProvider;
 * This class provides a simple way to ask different item providers by a
 * single method call, i.e. the consumer does not need to iterate over all
 * registered providers as this is done inside this class.
 * @author Stefan Triller - Method to convert a state into something a sitemap entity can understand
 * @author Erdoan Hadzhiyusein - Adapted the class to work with the new DateTimeType
 * @author Laurent Garnier - new method getIconColor
 * @author Mark Herwege - new method getFormatPattern(widget), clean pattern
 * @author Laurent Garnier - new icon parameter based on conditional rules
 * @author Danny Baumann - widget label source support
 * @author Laurent Garnier - Consider Colortemperaturepicker element as possible default widget
@Component(immediate = true, configurationPid = "org.openhab.sitemap", //
        property = Constants.SERVICE_PID + "=org.openhab.sitemap")
@ConfigurableService(category = "system", label = "Sitemap", description_uri = ItemUIRegistryImpl.CONFIG_URI)
public class ItemUIRegistryImpl implements ItemUIRegistry {
    protected static final String CONFIG_URI = "system:sitemap";
    protected static final Pattern EXTRACT_TRANSFORM_FUNCTION_PATTERN = Pattern.compile("(.*?)\\((.*)\\):(.*)");
    /* RegEx to identify format patterns. See java.util.Formatter#formatSpecifier (without the '%' at the very end). */
    protected static final String IDENTIFY_FORMAT_PATTERN_PATTERN = "%(?:(unit%)|(?:(?:\\d+\\$)?(?:[-#+ 0,(<]*)?(?:\\d+)?(?:\\.\\d+)?(?:[tT])?(?:[a-zA-Z])))";
    private static final Pattern FORMAT_PATTERN = Pattern.compile("(?:^|[^%])" + IDENTIFY_FORMAT_PATTERN_PATTERN);
    private static final int MAX_BUTTONS = 4;
    private static final String DEFAULT_SORTING = "NONE";
    private final Logger logger = LoggerFactory.getLogger(ItemUIRegistryImpl.class);
    protected final Set<ItemUIProvider> itemUIProviders = new HashSet<>();
    private final Map<Widget, Widget> defaultWidgets = Collections.synchronizedMap(new WeakHashMap<>());
    private String groupMembersSorting = DEFAULT_SORTING;
    private static class WidgetLabelWithSource {
        public final String label;
        public final WidgetLabelSource source;
        public WidgetLabelWithSource(String l, WidgetLabelSource s) {
            label = l;
            source = s;
    public ItemUIRegistryImpl(final @Reference ItemRegistry itemRegistry,
            final @Reference SitemapFactory sitemapFactory, final @Reference TimeZoneProvider timeZoneProvider) {
    public void addItemUIProvider(ItemUIProvider itemUIProvider) {
        itemUIProviders.add(itemUIProvider);
    public void removeItemUIProvider(ItemUIProvider itemUIProvider) {
        itemUIProviders.remove(itemUIProvider);
            final String groupMembersSortingString = Objects.toString(config.get("groupMembersSorting"), null);
            if (groupMembersSortingString != null) {
                groupMembersSorting = groupMembersSortingString;
    public @Nullable String getCategory(String itemName) {
        for (ItemUIProvider provider : itemUIProviders) {
            String currentCategory = provider.getCategory(itemName);
            if (currentCategory != null) {
                return currentCategory;
        // use the category, if defined
        String category = getItemCategory(itemName);
            return category.toLowerCase();
        // do some reasonable default
        // try to get the item type from the item name
        Class<? extends Item> itemType = getItemType(itemName);
        // we handle items here that have no specific widget,
        // e.g. the default widget of a rollerblind is "Switch".
        // We want to provide a dedicated default category for it
        // like "rollerblind".
        if (NumberItem.class.equals(itemType) || ContactItem.class.equals(itemType)
                || RollershutterItem.class.equals(itemType)) {
            return itemType.getSimpleName().replace("Item", "").toLowerCase();
    public @Nullable String getLabel(String itemName) {
            String currentLabel = provider.getLabel(itemName);
            if (currentLabel != null) {
                return currentLabel;
            if (item.getLabel() != null) {
                return item.getLabel();
        } catch (ItemNotFoundException ignored) {
    public @Nullable Widget getWidget(String itemName) {
            Widget currentWidget = provider.getWidget(itemName);
            if (currentWidget != null) {
                return resolveDefault(currentWidget);
    public @Nullable Widget getDefaultWidget(@Nullable Class<? extends Item> targetItemType, String itemName) {
            Widget widget = provider.getDefaultWidget(targetItemType, itemName);
        // do some reasonable default, if no provider had an answer
        // if the itemType is not defined, try to get it from the item name
        Class<? extends Item> itemType = targetItemType;
            itemType = getItemType(itemName);
        if (GroupItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Group");
        } else if (CallItem.class.equals(itemType) //
                || ContactItem.class.equals(itemType) //
                || DateTimeItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Text");
        } else if (ColorItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Colorpicker");
        } else if (DimmerItem.class.equals(itemType)) {
            Slider slider = (Slider) sitemapFactory.createWidget("Slider");
            slider.setSwitchEnabled(true);
            slider.setReleaseOnly(true);
            return slider;
        } else if (ImageItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Image");
        } else if (LocationItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Mapview");
        } else if (NumberItem.class.isAssignableFrom(itemType) //
                || StringItem.class.equals(itemType)) {
            boolean isReadOnly = isReadOnly(itemName);
            int commandOptionsSize = getCommandOptionsSize(itemName);
            if (!isReadOnly && commandOptionsSize > 0) {
                return commandOptionsSize <= MAX_BUTTONS ? sitemapFactory.createWidget("Switch")
                        : sitemapFactory.createWidget("Selection");
            if (!isReadOnly && hasStateOptions(itemName)) {
                return sitemapFactory.createWidget("Selection");
            if (!isReadOnly && NumberItem.class.isAssignableFrom(itemType) && hasItemTag(itemName, "Setpoint")) {
                return sitemapFactory.createWidget("Setpoint");
            } else if (!isReadOnly && NumberItem.class.isAssignableFrom(itemType)
                    && hasItemTag(itemName, "ColorTemperature")) {
                return sitemapFactory.createWidget("Colortemperaturepicker");
        } else if (PlayerItem.class.equals(itemType)) {
            return createPlayerButtons();
        } else if (RollershutterItem.class.equals(itemType) //
                || SwitchItem.class.equals(itemType)) {
            return sitemapFactory.createWidget("Switch");
    private Switch createPlayerButtons() {
        final Switch playerItemSwitch = (Switch) sitemapFactory.createWidget("Switch");
        final List<Mapping> mappings = playerItemSwitch.getMappings();
        Mapping commandMapping;
        mappings.add(commandMapping = sitemapFactory.createMapping());
        commandMapping.setCmd(NextPreviousType.PREVIOUS.name());
        commandMapping.setLabel("<<");
        commandMapping.setCmd(PlayPauseType.PAUSE.name());
        commandMapping.setLabel("||");
        commandMapping.setCmd(PlayPauseType.PLAY.name());
        commandMapping.setLabel(">");
        commandMapping.setCmd(NextPreviousType.NEXT.name());
        commandMapping.setLabel(">>");
        return playerItemSwitch;
    public @Nullable String getLabel(Widget w) {
        String label = getLabelFromWidget(w).label;
        String itemName = w.getItem();
        if (itemName == null || itemName.isBlank()) {
            return transform(label, true, null, null);
        String labelMappedOption = null;
        String formatPattern = getFormatPattern(w);
        if (formatPattern != null && label.indexOf("[") < 0) {
            label = label + " [" + formatPattern + "]";
        // now insert the value, if the state is a string or decimal value and there is some formatting pattern defined
        // in the label or state description (i.e. it contains at least a %)
            // There is a known issue in the implementation of the method getStateDescription() of class Item
            // in the following case:
            // - the item provider returns as expected a state description without pattern but with for
            // example a min value because a min value is set in the item definition but no label with
            // pattern is set.
            // - the channel state description provider returns as expected a state description with a pattern
            // In this case, the result is no display of value by UIs because no pattern is set in the
            // returned StateDescription. What is expected is the display of a value using the pattern
            // provided by the channel state description provider.
            stateDescription = item.getStateDescription();
            if (formatPattern != null) {
                state = item.getState();
                if (formatPattern.contains("%d")) {
                    if (!(state instanceof UnDefType) && !(state instanceof Number)) {
                        // States which do not provide a Number will be converted to DecimalType.
                        // e.g.: GroupItem can provide a count of items matching the active state
                        // for some group functions.
                        state = item.getStateAs(DecimalType.class);
                    // for fraction digits in state we don't want to risk format exceptions,
                    // so treat everything as floats:
                    formatPattern = formatPattern.replace("%d", "%.0f");
            logger.warn("Cannot retrieve item '{}' for widget {}", itemName, w.getClass().getSimpleName());
        boolean considerTransform = false;
        String transformFailbackValue = null;
            if (formatPattern.isEmpty()) {
                    formatPattern = formatUndefined(formatPattern);
                    considerTransform = true;
                } else if (state instanceof UnDefType) {
                    Matcher matcher = EXTRACT_TRANSFORM_FUNCTION_PATTERN.matcher(formatPattern);
                        formatPattern = type + "(" + function + "):" + state.toString();
                        transformFailbackValue = "-";
                    // if the channel contains options, we build a label with the mapped option value
                            String optionLabel = option.getLabel();
                            if (option.getValue().equals(state.toString()) && optionLabel != null) {
                                String formatPatternOption;
                                    formatPatternOption = String.format(formatPattern, optionLabel);
                                            "Mapping option value '{}' for item {} using format '{}' failed ({}); format is ignored and option label is used",
                                            optionLabel, itemName, formatPattern, e.getMessage());
                                    formatPatternOption = optionLabel;
                                labelMappedOption = label.trim();
                                labelMappedOption = labelMappedOption.substring(0, labelMappedOption.indexOf("[") + 1)
                                        + formatPatternOption + "]";
                    if (state instanceof DecimalType) {
                        // for DecimalTypes we don't want to risk format exceptions, if pattern contains unit
                        // placeholder
                        if (formatPattern.contains(UnitUtils.UNIT_PLACEHOLDER)) {
                            formatPattern = formatPattern.replaceAll(UnitUtils.UNIT_PLACEHOLDER, "").stripTrailing();
                    } else if (state instanceof QuantityType quantityState) {
                        // sanity convert current state to the item state description unit in case it was updated in the
                        // meantime. The item state is still in the "original" unit while the state description will
                        // display the new unit:
                        Unit<?> patternUnit = UnitUtils.parseUnit(formatPattern);
                        // The widget may define its own unit in the widget label. Convert to this unit:
                            quantityState = convertStateToWidgetUnit(quantityState, w);
                    // Without this catch, the whole sitemap, or page can not be displayed!
                    // This also handles IllegalFormatConversionException, which is a subclass of IllegalArgument.
                            formatPattern = type + "(" + function + "):";
                                formatPattern += dateTimeState.format(value, timeZoneProvider.getTimeZone());
                                transformFailbackValue = dateTimeState.toFullString(timeZoneProvider.getTimeZone());
                                formatPattern += state.format(value);
                                transformFailbackValue = state.toString();
                                formatPattern = dateTimeState.format(formatPattern, timeZoneProvider.getTimeZone());
                                formatPattern = state.format(formatPattern);
                        logger.warn("Exception while formatting value '{}' of item {} with format '{}': {}", state,
                                itemName, formatPattern, e.getMessage());
                        formatPattern = "Err";
                label = label.trim();
                int index = label.indexOf("[");
                    label = label.substring(0, index + 1) + formatPattern + "]";
        return transform(label, considerTransform, transformFailbackValue, labelMappedOption);
    public WidgetLabelSource getLabelSource(Widget w) {
        return getLabelFromWidget(w).source;
    private QuantityType<?> convertStateToWidgetUnit(QuantityType<?> quantityState, Widget w) {
        Unit<?> widgetUnit = UnitUtils.parseUnit(getFormatPattern(w));
        if (widgetUnit != null && !widgetUnit.equals(quantityState.getUnit())) {
            return Objects.requireNonNullElse(quantityState.toInvertibleUnit(widgetUnit), quantityState);
        return quantityState;
    public @Nullable String getFormatPattern(Widget w) {
        String pattern = getFormatPattern(label);
            Item item = null;
            if (itemName != null && !itemName.isBlank()) {
                item = getItem(itemName);
            if (item != null && pattern == null) {
                    pattern = stateDescription.getPattern();
            // remove last part of pattern, after unit, if it exists, as this is not valid and creates problems with
            // updates
            if (item instanceof NumberItem numberItem && numberItem.getDimension() != null) {
                Matcher m = FORMAT_PATTERN.matcher(pattern);
                int matcherEnd = 0;
                if (m.find() && m.group(1) == null) {
                    matcherEnd = m.end();
                String unit = pattern.substring(matcherEnd).trim();
                String postfix = "";
                int unitEnd = unit.indexOf(" ");
                if (unitEnd > -1) {
                    postfix = unit.substring(unitEnd + 1).trim();
                    unit = unit.substring(0, unitEnd);
                if (!postfix.isBlank()) {
                            "Item '{}' with unit, nothing allowed after unit in label pattern '{}', dropping postfix",
                            itemName, pattern);
                pattern = unit.isBlank() ? pattern.substring(0, matcherEnd)
                        : pattern.substring(0, pattern.indexOf(unit, matcherEnd) + unit.length());
    private @Nullable String getFormatPattern(@Nullable String label) {
        String pattern = label.trim();
        int indexOpenBracket = pattern.indexOf("[");
        int indexCloseBracket = pattern.endsWith("]") ? pattern.length() - 1 : -1;
        if ((indexOpenBracket >= 0) && (indexCloseBracket > indexOpenBracket)) {
            return pattern.substring(indexOpenBracket + 1, indexCloseBracket);
    private WidgetLabelWithSource getLabelFromWidget(Widget w) {
        WidgetLabelSource source = WidgetLabelSource.NONE;
        if (w.getLabel() != null) {
            // if there is a label defined for the widget, use this
            label = w.getLabel();
            source = WidgetLabelSource.SITEMAP_WIDGET;
                // check if any item ui provider provides a label for this item
                label = getLabel(itemName);
                // if there is no item ui provider saying anything, simply use the name as a label
                    source = WidgetLabelSource.ITEM_LABEL;
                    label = itemName;
                    source = WidgetLabelSource.ITEM_NAME;
        // use an empty string, if no label could be found
        return new WidgetLabelWithSource(label != null ? label : "", source);
     * Takes the given <code>formatPattern</code> and replaces it with an analog
     * String-based pattern to replace all value Occurrences with a dash ("-")
     * @param formatPattern the original pattern which will be replaces by a
     *            String pattern.
     * @return a formatted String with dashes ("-") as value replacement
    protected String formatUndefined(String formatPattern) {
        String undefinedFormatPattern = formatPattern.replaceAll(IDENTIFY_FORMAT_PATTERN_PATTERN, "%1\\$s");
            return String.format(undefinedFormatPattern, "-");
                    "Exception while formatting undefined value [sourcePattern={}, targetPattern={}, exceptionMessage={}]",
                    formatPattern, undefinedFormatPattern, e.getMessage());
            return "Err";
    private String insertInLabel(String label, Object o) {
        return label.substring(0, label.indexOf("[") + 1) + o + "]";
     * check if there is a status value being displayed on the right side of the
     * label (the right side is signified by being enclosed in square brackets [].
     * If so, check if the value starts with the call to a transformation service
     * (e.g. "[MAP(en.map):%s]") and execute the transformation in this case.
     * If the value does not start with the call to a transformation service,
     * we return the label with the mapped option value if provided (not null).
    private String transform(String label, boolean matchTransform, @Nullable String transformFailbackValue,
            @Nullable String labelMappedOption) {
        String ret = label;
        String formatPattern = getFormatPattern(label);
            if (matchTransform && matcher.find()) {
                String failbackValue = transformFailbackValue != null ? transformFailbackValue : value;
                            String transformationResult = transformation.transform(function, value);
                            if (transformationResult != null) {
                                ret = insertInLabel(label, transformationResult);
                                logger.warn("Transformation of type {} did not return a valid result", type);
                                ret = insertInLabel(label, failbackValue);
                    Throwable cause = e.getCause();
                    logger.warn("Failed transforming the value '{}' with pattern '{}': {}", value, formatPattern,
                            cause instanceof ScriptException ? cause.getMessage() : e.getMessage());
            } else if (labelMappedOption != null) {
                ret = labelMappedOption;
    public @Nullable String getCategory(Widget w) {
        String widgetTypeName = w.getWidgetType();
        // the default is the widget type name, e.g. "switch"
        String category = widgetTypeName.toLowerCase();
        String conditionalIcon = getConditionalIcon(w);
        // if an icon is defined for the widget, use it
        if (w.getIcon() != null) {
            category = w.getIcon();
        } else if (conditionalIcon != null) {
            category = conditionalIcon;
            // otherwise check if any item ui provider provides an icon for this item
                String result = getCategory(itemName);
                    category = result;
    public @Nullable State getState(Widget w) {
                return convertState(w, item, item.getState());
        return UnDefType.UNDEF;
     * Converts an item state to the type the widget supports (if possible)
     * @param w Widget in sitemap that shows the state
     * @param i item
     * @param state the state
     * @return the converted state or the original if conversion was not possible
    public @Nullable State convertState(Widget w, Item i, State state) {
        State returnState = null;
        State itemState = i.getState();
        if (itemState instanceof QuantityType<?> quantityTypeState) {
            itemState = convertStateToWidgetUnit(quantityTypeState, w);
        if (w instanceof Switch && i instanceof RollershutterItem) {
            // RollerShutter are represented as Switch in a Sitemap but need a PercentType state
            returnState = itemState.as(PercentType.class);
        } else if (w instanceof Slider) {
            if (i.getAcceptedDataTypes().contains(PercentType.class)) {
            } else if (!(itemState instanceof QuantityType<?>)) {
                returnState = itemState.as(DecimalType.class);
        } else if (w instanceof Switch sw) {
            StateDescription stateDescr = i.getStateDescription();
            if (sw.getMappings().isEmpty() && (stateDescr == null || stateDescr.getOptions().isEmpty())) {
                returnState = itemState.as(OnOffType.class);
        // we return the original state to not break anything
        return Objects.requireNonNullElse(returnState, itemState);
    public @Nullable Widget getWidget(Sitemap sitemap, String id) {
        if (!id.isEmpty()) {
            // see if the id is an itemName and try to get the widget for it
            Widget w = getWidget(id);
            if (w == null) {
                // try to get the default widget instead
                w = getDefaultWidget(null, id);
                w.setItem(id);
                    int widgetID = Integer.parseInt(id.substring(0, 2));
                    if (widgetID < sitemap.getWidgets().size()) {
                        w = sitemap.getWidgets().get(widgetID);
                        for (int i = 2; i < id.length(); i += 2) {
                            int childWidgetID = Integer.parseInt(id.substring(i, i + 2));
                            if (childWidgetID < ((LinkableWidget) w).getWidgets().size()) {
                                w = ((LinkableWidget) w).getWidgets().get(childWidgetID);
                    // no valid number, so the requested page id does not exist
            return resolveDefault(w);
        logger.warn("Cannot find page for id '{}'.", id);
    public List<Widget> getChildren(Sitemap sitemap) {
        List<Widget> result = new ArrayList<>();
        widgets.stream().map(this::resolveDefault).filter(Objects::nonNull).map(Objects::requireNonNull)
                .forEach(result::add);
    public List<Widget> getChildren(LinkableWidget w) {
        if (w instanceof Group group && w.getWidgets().isEmpty()) {
            widgets = getDynamicGroupChildren(group);
            widgets = w.getWidgets();
    public @Nullable Parent getParent(Widget w) {
        Widget w2 = defaultWidgets.get(w);
        return (w2 == null) ? w.getParent() : w2.getParent();
    private @Nullable Widget resolveDefault(@Nullable Widget widget) {
        if (!(widget instanceof Default)) {
                    Widget defaultWidget = getDefaultWidget(item.getClass(), item.getName());
                    if (defaultWidget != null) {
                        copyProperties(widget, defaultWidget);
                        defaultWidgets.put(defaultWidget, widget);
                        return defaultWidget;
    private void copyProperties(Widget source, Widget target) {
        target.setItem(source.getItem());
        target.setIcon(source.getIcon());
        target.setStaticIcon(source.isStaticIcon());
        target.setLabel(source.getLabel());
        target.getVisibility().addAll(copyAll(source.getVisibility()));
        target.getLabelColor().addAll(copyAll(source.getLabelColor()));
        target.getValueColor().addAll(copyAll(source.getValueColor()));
        target.getIconColor().addAll(copyAll(source.getIconColor()));
        target.getIconRules().addAll(copyAll(source.getIconRules()));
    private Collection<? extends Rule> copyAll(List<Rule> rules) {
        return rules.stream().map(rule -> {
            Rule ruleCopy = sitemapFactory.createRule();
            ruleCopy.getConditions().addAll(rule.getConditions().stream().map(condition -> {
                org.openhab.core.sitemap.Condition conditionCopy = sitemapFactory.createCondition();
                conditionCopy.setItem(condition.getItem());
                conditionCopy.setCondition(condition.getCondition());
                conditionCopy.setValue(condition.getValue());
                return conditionCopy;
            }).toList());
            ruleCopy.setArgument(rule.getArgument());
     * This method creates a list of children for a group dynamically.
     * If there are no explicit children defined in a sitemap, the children
     * can thus be created on the fly by iterating over the members of the group item.
     * @param group The group widget to get children for
     * @return a list of default widgets provided for the member items
    private List<Widget> getDynamicGroupChildren(Group group) {
        List<Widget> children = new ArrayList<>();
        String itemName = group.getItem();
                    List<Item> members = new ArrayList<>(groupItem.getMembers());
                    switch (groupMembersSorting) {
                        case "LABEL":
                            members.sort((u1, u2) -> {
                                String u1Label = u1.getLabel();
                                String u2Label = u2.getLabel();
                                if (u1Label != null && u2Label != null) {
                                    return u1Label.compareTo(u2Label);
                                    return u1.getName().compareTo(u2.getName());
                        case "NAME":
                            members.sort(Comparator.comparing(Item::getName));
                        Widget widget = getDefaultWidget(member.getClass(), member.getName());
                            widget.setItem(member.getName());
                            children.add(widget);
                    logger.warn("Item '{}' is not a group.", item.getName());
                logger.warn("Dynamic group with label '{}' does not specify an associated item - ignoring it.",
                        group.getLabel());
            logger.warn("Dynamic group with label '{}' will be ignored, because its item '{}' does not exist.",
                    group.getLabel(), itemName);
    private boolean isReadOnly(String itemName) {
            return stateDescription != null && stateDescription.isReadOnly();
    private boolean hasStateOptions(String itemName) {
            return stateDescription != null && !stateDescription.getOptions().isEmpty();
    private int getCommandOptionsSize(String itemName) {
            CommandDescription commandDescription = item.getCommandDescription();
            return commandDescription != null ? commandDescription.getCommandOptions().size() : 0;
    private @Nullable Class<? extends Item> getItemType(String itemName) {
            return item.getClass();
    public @Nullable State getItemState(String itemName) {
    public @Nullable String getItemCategory(String itemName) {
            return item.getCategory();
    private boolean hasItemTag(String itemName, String tag) {
            return item.hasTag(tag);
    public String getWidgetId(Widget widget) {
        Widget w2 = defaultWidgets.get(widget);
        if (w2 != null) {
            return getWidgetId(w2);
        Widget w = widget;
        while (w.getParent() instanceof LinkableWidget parent) {
            String index = String.valueOf(parent.getWidgets().indexOf(w));
            if (index.length() == 1) {
                index = "0" + index; // make it two digits
            id = index + id;
            w = parent;
        if (w.getParent() instanceof Sitemap sitemap) {
            String index = String.valueOf(sitemap.getWidgets().indexOf(w));
        // if the widget is dynamically created and not available in the sitemap,
        // use the item name as the id
        if (w.getParent() == null) {
            String item = w.getItem();
            id = item != null ? item : id;
    private boolean matchStateToValue(State state, String value, @Nullable String matchCondition) {
        // Remove quotes - this occurs in some instances where multiple types
        // are defined in the xtext definitions
        String unquotedValue = value;
        if (unquotedValue.startsWith("\"") && unquotedValue.endsWith("\"")) {
            unquotedValue = unquotedValue.substring(1, unquotedValue.length() - 1);
        // Convert the condition string into enum
        Condition condition = Condition.EQUAL;
        if (matchCondition != null) {
            condition = Condition.fromString(matchCondition);
            if (condition == null) {
                logger.warn("matchStateToValue: unknown match condition '{}'", matchCondition);
        // Check if the value is equal to the supplied value
        boolean matched = false;
        if (UnDefType.NULL.toString().equals(unquotedValue) || UnDefType.UNDEF.toString().equals(unquotedValue)) {
                    if (unquotedValue.equals(state.toString())) {
                case NOT:
                case NOTEQUAL:
                    if (!unquotedValue.equals(state.toString())) {
            if (state instanceof DecimalType || state instanceof QuantityType<?>) {
                    double compareDoubleValue = Double.parseDouble(unquotedValue);
                    double stateDoubleValue;
                        stateDoubleValue = type.doubleValue();
                        stateDoubleValue = ((QuantityType<?>) state).doubleValue();
                            if (stateDoubleValue == compareDoubleValue) {
                        case LTE:
                            if (stateDoubleValue <= compareDoubleValue) {
                        case GTE:
                            if (stateDoubleValue >= compareDoubleValue) {
                        case GREATER:
                            if (stateDoubleValue > compareDoubleValue) {
                        case LESS:
                            if (stateDoubleValue < compareDoubleValue) {
                            if (stateDoubleValue != compareDoubleValue) {
                    logger.debug("matchStateToValue: Decimal format exception: ", e);
            } else if (state instanceof DateTimeType dateTimeState) {
                Instant val = dateTimeState.getInstant();
                long secsDif = ChronoUnit.SECONDS.between(val, now);
                            if (secsDif == Integer.parseInt(unquotedValue)) {
                            if (secsDif <= Integer.parseInt(unquotedValue)) {
                            if (secsDif >= Integer.parseInt(unquotedValue)) {
                            if (secsDif > Integer.parseInt(unquotedValue)) {
                            if (secsDif < Integer.parseInt(unquotedValue)) {
                            if (secsDif != Integer.parseInt(unquotedValue)) {
                // Strings only allow = and !=
        return matched;
    private @Nullable String processColorDefinition(Widget w, @Nullable List<Rule> colorList, String colorType) {
        // Sanity check
        if (colorList == null || colorList.isEmpty()) {
        logger.debug("Checking {} color for widget '{}'.", colorType, w.getLabel());
        String colorString = null;
        // Loop through all elements looking for the definition associated
        // with the supplied value
        for (Rule rule : colorList) {
            if (allConditionsOk(rule.getConditions(), w)) {
                // We have the color for this value - break!
                colorString = rule.getArgument();
        if (colorString == null) {
            logger.debug("No {} color found for widget '{}'.", colorType, w.getLabel());
        // Remove quotes off the colour - if they exist
        if (colorString.startsWith("\"") && colorString.endsWith("\"")) {
            colorString = colorString.substring(1, colorString.length() - 1);
        logger.debug("{} color for widget '{}' is '{}'.", colorType, w.getLabel(), colorString);
        return colorString;
    public @Nullable String getLabelColor(Widget w) {
        return processColorDefinition(w, w.getLabelColor(), "label");
    public @Nullable String getValueColor(Widget w) {
        return processColorDefinition(w, w.getValueColor(), "value");
    public @Nullable String getIconColor(Widget w) {
        return processColorDefinition(w, w.getIconColor(), "icon");
    public boolean getVisiblity(Widget w) {
        // Default to visible if parameters not set
        List<Rule> ruleList = w.getVisibility();
        if (ruleList.isEmpty()) {
        logger.debug("Checking visiblity for widget '{}'.", w.getLabel());
        logger.debug("Widget {} is not visible.", w.getLabel());
    public @Nullable String getConditionalIcon(Widget w) {
        List<Rule> ruleList = w.getIconRules();
        logger.debug("Checking icon for widget '{}'.", w.getLabel());
        String icon = null;
                // We have the icon for this value - break!
                icon = rule.getArgument();
        if (icon == null) {
            logger.debug("No icon found for widget '{}'.", w.getLabel());
        // Remove quotes off the icon - if they exist
        if (icon.startsWith("\"") && icon.endsWith("\"")) {
            icon = icon.substring(1, icon.length() - 1);
        logger.debug("icon for widget '{}' is '{}'.", w.getLabel(), icon);
    private boolean allConditionsOk(@Nullable List<org.openhab.core.sitemap.Condition> conditions, Widget w) {
        boolean allConditionsOk = true;
            State defaultState = getState(w);
            // Go through all AND conditions
            for (org.openhab.core.sitemap.Condition condition : conditions) {
                // Use a local state variable in case it gets overridden below
                State state = defaultState;
                // If there's an item defined here, get its state
                    // Try and find the item to test.
                        // Get the item state
                        logger.warn("Cannot retrieve item {} for widget {}", itemName, w.getClass().getSimpleName());
                String value = condition.getValue();
                if (state == null || !matchStateToValue(state, value, condition.getCondition())) {
                    allConditionsOk = false;
        return allConditionsOk;
    enum Condition {
        EQUAL("=="),
        LTE("<="),
        NOTEQUAL("!="),
        GREATER(">"),
        LESS("<"),
        NOT("!");
        Condition(String value) {
        public static @Nullable Condition fromString(String text) {
            for (Condition c : Condition.values()) {
                if (text.equalsIgnoreCase(c.value)) {
        return itemRegistry.remove(itemName, recursive);
    public @Nullable String getUnitForWidget(Widget w) {
                // we require the item to define a dimension, otherwise no unit will be reported to the UIs.
                    String pattern = getFormatPattern(w);
                    if (pattern == null || pattern.isBlank()) {
                        // if no Label was assigned to the Widget we fallback to the items unit
                        return numberItem.getUnitSymbol();
                    String unit = getUnitFromPattern(pattern);
                    if (!UnitUtils.UNIT_PLACEHOLDER.equals(unit)) {
                logger.warn("Failed to retrieve item during widget rendering, item does not exist: {}", e.getMessage());
    public @Nullable State convertStateToLabelUnit(QuantityType<?> state, String label) {
        int index = label.lastIndexOf(" ");
        String labelUnit = index > 0 ? label.substring(index) : null;
        if (labelUnit != null && !state.getUnit().toString().equals(labelUnit)) {
            return state.toInvertibleUnit(labelUnit);
    private @Nullable String getUnitFromPattern(@Nullable String format) {
        if (format == null || format.isBlank()) {
        int index = format.lastIndexOf(" ");
        String unit = index > 0 ? format.substring(index + 1) : null;
        unit = UnitUtils.UNIT_PERCENT_FORMAT_STRING.equals(unit) ? "%" : unit;
