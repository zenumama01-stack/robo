import org.openhab.core.ui.components.UIComponent;
 * This {@link SitemapProvider} provides sitemaps from all well-formed {@link RootUIComponent} found in a specific
 * "system:sitemap" namespace.
 * @author Laurent Garnier - icon color support for all widgets
 * @author Laurent Garnier - Added support for new element Buttongrid
 * @author Laurent Garnier - Added icon field for mappings
 * @author Mark Herwege - Make UI provided sitemaps compatible with enhanced syntax in conditions
 * @author Mark Herwege - Add support for Button element
public class UIComponentSitemapProvider extends AbstractProvider<Sitemap>
        implements SitemapProvider, RegistryChangeListener<RootUIComponent> {
    private final Logger logger = LoggerFactory.getLogger(UIComponentSitemapProvider.class);
    public static final String SITEMAP_NAMESPACE = "system:sitemap";
    private static final String SITEMAP_PREFIX = "uicomponents_";
    private static final Pattern CONDITION_PATTERN = Pattern
            .compile("((?<item>[A-Za-z]\\w*)?\\s*(?<condition>==|!=|<=|>=|<|>))?\\s*(?<value>(\\+|-)?.+)");
    private static final Pattern COMMANDS_PATTERN = Pattern.compile("^(?<cmd1>\"[^\"]*\"|[^\": ]*):(?<cmd2>.*)$");
    private Map<String, Sitemap> sitemaps = new HashMap<>();
    private @Nullable UIComponentRegistryFactory componentRegistryFactory;
    private @Nullable UIComponentRegistry sitemapComponentRegistry;
    public UIComponentSitemapProvider(final @Reference SitemapRegistry sitemapRegistry,
            final @Reference SitemapFactory sitemapFactory) {
        return sitemaps.get(sitemapName);
        return sitemaps.keySet();
    protected Sitemap buildSitemap(RootUIComponent rootComponent) {
        if (!"Sitemap".equals(rootComponent.getType())) {
            throw new IllegalArgumentException("Root component type is not Sitemap");
        Sitemap sitemap = sitemapFactory.createSitemap(SITEMAP_PREFIX + rootComponent.getUID());
        Object label = rootComponent.getConfig().get("label");
            sitemap.setLabel(label.toString());
        if (rootComponent.getSlots() != null && rootComponent.getSlots().containsKey("widgets")) {
            for (UIComponent component : rootComponent.getSlot("widgets")) {
                Widget widget = buildWidget(component, sitemap);
                    sitemap.getWidgets().add(widget);
        sitemaps.put(sitemap.getName(), sitemap);
    protected @Nullable Widget buildWidget(UIComponent component, Parent parent) {
        Widget widget = sitemapFactory.createWidget(component.getType(), parent);
        if (widget == null) {
            logger.warn("Unknown sitemap component type {}", component.getType());
                setWidgetPropertyFromComponentConfig(imageWidget, component, "url");
                setWidgetPropertyFromComponentConfig(imageWidget, component, "refresh");
                setWidgetPropertyFromComponentConfig(videoWidget, component, "url");
                setWidgetPropertyFromComponentConfig(videoWidget, component, "encoding");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "service");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "refresh");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "period");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "legend");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "forceAsItem");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "yAxisDecimalPattern");
                setWidgetPropertyFromComponentConfig(chartWidget, component, "interpolation");
                setWidgetPropertyFromComponentConfig(webviewWidget, component, "height");
                setWidgetPropertyFromComponentConfig(webviewWidget, component, "url");
                addWidgetMappings(switchWidget.getMappings(), component);
                setWidgetPropertyFromComponentConfig(mapviewWidget, component, "height");
                setWidgetPropertyFromComponentConfig(sliderWidget, component, "minValue");
                setWidgetPropertyFromComponentConfig(sliderWidget, component, "maxValue");
                setWidgetPropertyFromComponentConfig(sliderWidget, component, "step");
                setWidgetPropertyFromComponentConfig(sliderWidget, component, "switchEnabled");
                setWidgetPropertyFromComponentConfig(sliderWidget, component, "releaseOnly");
                addWidgetMappings(selectionWidget.getMappings(), component);
                setWidgetPropertyFromComponentConfig(inputWidget, component, "inputHint");
                setWidgetPropertyFromComponentConfig(setpointWidget, component, "minValue");
                setWidgetPropertyFromComponentConfig(setpointWidget, component, "maxValue");
                setWidgetPropertyFromComponentConfig(setpointWidget, component, "step");
                setWidgetPropertyFromComponentConfig(colortemperaturepickerWidget, component, "minValue");
                setWidgetPropertyFromComponentConfig(colortemperaturepickerWidget, component, "maxValue");
                addWidgetButtons(buttongridWidget.getButtons(), component);
                setWidgetPropertyFromComponentConfig(buttonWidget, component, "row");
                setWidgetPropertyFromComponentConfig(buttonWidget, component, "column");
                setWidgetPropertyFromComponentConfig(buttonWidget, component, "stateless");
                setWidgetPropertyFromComponentConfig(buttonWidget, component, "cmd");
                setWidgetPropertyFromComponentConfig(buttonWidget, component, "releaseCmd");
                setWidgetPropertyFromComponentConfig(defaultWidget, component, "height");
        setWidgetPropertyFromComponentConfig(widget, component, "item");
        setWidgetPropertyFromComponentConfig(widget, component, "label");
        setWidgetPropertyFromComponentConfig(widget, component, "icon");
        setWidgetPropertyFromComponentConfig(widget, component, "staticIcon");
            if (component.getSlots() != null && component.getSlots().containsKey("widgets")) {
                for (UIComponent childComponent : component.getSlot("widgets")) {
                    Widget childWidget = buildWidget(childComponent, linkableWidget);
                    if (childWidget != null) {
                        linkableWidget.getWidgets().add(childWidget);
        addWidgetRules(widget.getVisibility(), component, "visibility");
        addWidgetRules(widget.getLabelColor(), component, "labelColor");
        addWidgetRules(widget.getValueColor(), component, "valueColor");
        addWidgetRules(widget.getIconColor(), component, "iconColor");
        addWidgetRules(widget.getIconRules(), component, "iconRules");
    private void setWidgetPropertyFromComponentConfig(Widget widget, @Nullable UIComponent component,
            String configParamName) {
        if (component == null || component.getConfig() == null) {
        Object value = component.getConfig().get(configParamName);
            String setterName = "set" + configParamName.substring(0, 1).toUpperCase() + configParamName.substring(1);
            Object normalizedValue = ConfigUtil.normalizeType(value);
            Class<?> clazz = widget.getClass();
            Method method = List.of(clazz.getMethods()).stream().filter(m -> m.getName().equals(setterName)).findFirst()
            Class<?> argumentType = method.getParameters()[0].getType();
            if (argumentType.equals(Integer.class) || argumentType.equals(int.class)) {
                normalizedValue = (normalizedValue instanceof BigDecimal bd) ? bd.intValue()
                        : Integer.parseInt(normalizedValue.toString());
            } else if ((argumentType.equals(Boolean.class) || argumentType.equals(boolean.class))
                    && !(normalizedValue instanceof Boolean)) {
                normalizedValue = Boolean.valueOf(normalizedValue.toString());
            method.invoke(widget, normalizedValue);
            logger.warn("Cannot set {} parameter for {} widget parameter: {}", configParamName, component.getType(),
    private @Nullable String stripQuotes(@Nullable String input) {
        if ((input != null) && (input.length() >= 2) && (input.charAt(0) == '\"')
                && (input.charAt(input.length() - 1) == '\"')) {
            return input.substring(1, input.length() - 1);
    private void addWidgetMappings(List<Mapping> mappings, UIComponent component) {
        if (component.getConfig() != null && component.getConfig().containsKey("mappings")) {
            Object sourceMappings = component.getConfig().get("mappings");
            if (sourceMappings instanceof Collection<?> sourceMappingsCollection) {
                for (Object sourceMapping : sourceMappingsCollection) {
                    if (sourceMapping instanceof String) {
                        String[] splitMapping = sourceMapping.toString().split("=");
                        String cmd = splitMapping[0].trim();
                        String releaseCmd = null;
                        Matcher matcher = COMMANDS_PATTERN.matcher(cmd);
                            cmd = matcher.group("cmd1");
                            releaseCmd = matcher.group("cmd2");
                        cmd = stripQuotes(cmd);
                        releaseCmd = stripQuotes(releaseCmd);
                        String label = stripQuotes(splitMapping[1].trim());
                        String icon = splitMapping.length < 3 ? null : stripQuotes(splitMapping[2].trim());
                        if (cmd != null) {
                            mapping.setCmd(cmd);
                        mapping.setReleaseCmd(releaseCmd);
                            mapping.setLabel(label);
                        mapping.setIcon(icon);
    private void addWidgetButtons(List<ButtonDefinition> buttons, UIComponent component) {
        if (component.getConfig() != null && component.getConfig().containsKey("buttons")) {
            Object sourceButtons = component.getConfig().get("buttons");
            if (sourceButtons instanceof Collection<?> sourceButtonsCollection) {
                for (Object sourceButton : sourceButtonsCollection) {
                    if (sourceButton instanceof String) {
                        String[] splitted1 = sourceButton.toString().split(":", 3);
                        int row = Integer.parseInt(splitted1[0].trim());
                        int column = Integer.parseInt(splitted1[1].trim());
                        String[] splitted2 = splitted1[2].trim().split("=");
                        String cmd = stripQuotes(splitted2[0].trim());
                        String label = stripQuotes(splitted2[1].trim());
                        String icon = splitted2.length < 3 ? null : stripQuotes(splitted2[2].trim());
                        button.setRow(row);
                        button.setColumn(column);
                            button.setCmd(cmd);
                            button.setLabel(label);
                        button.setIcon(icon);
    private void addWidgetRules(List<Rule> rules, UIComponent component, String key) {
        if (component.getConfig() != null && component.getConfig().containsKey(key)) {
            Object sourceRules = component.getConfig().get(key);
            if (sourceRules instanceof Collection<?> sourceRulesCollection) {
                for (Object sourceRule : sourceRulesCollection) {
                    if (sourceRule instanceof String) {
                        String argument = !key.equals("visibility") ? getRuleArgument(sourceRule.toString()) : null;
                        List<String> conditionsString = getRuleConditions(sourceRule.toString(), argument);
                        Rule rule = sitemapFactory.createRule();
                        List<Condition> conditions = getConditions(conditionsString, component, key);
                        rule.setConditions(conditions);
                        rule.setArgument(argument);
                        rules.add(rule);
    private List<Condition> getConditions(List<String> conditionsString, UIComponent component, String key) {
        for (String conditionString : conditionsString) {
            Matcher matcher = CONDITION_PATTERN.matcher(conditionString);
            String value = null;
                value = stripQuotes(matcher.group("value"));
            if (matcher.matches() && value != null) {
                condition.setItem(matcher.group("item"));
                condition.setCondition(matcher.group("condition"));
                logger.warn("Syntax error in {} rule condition '{}' for widget {}", key, conditionString,
                        component.getType());
    private String getRuleArgument(String rule) {
        int argIndex = rule.lastIndexOf("=") + 1;
        String strippedRule = stripQuotes(rule.substring(argIndex).trim());
        return strippedRule != null ? strippedRule : "";
    private List<String> getRuleConditions(String rule, @Nullable String argument) {
        String conditions = rule;
        if (argument != null) {
            conditions = rule.substring(0, rule.lastIndexOf(argument)).trim();
            if (conditions.endsWith("=")) {
                conditions = conditions.substring(0, conditions.length() - 1);
        List<String> conditionsList = List.of(conditions.split(" AND "));
        return conditionsList.stream().filter(Predicate.not(String::isBlank)).map(String::trim).toList();
    protected void setComponentRegistryFactory(UIComponentRegistryFactory componentRegistryFactory) {
        UIComponentRegistry sitemapComponentRegistry = this.componentRegistryFactory.getRegistry(SITEMAP_NAMESPACE);
        sitemapComponentRegistry.addRegistryChangeListener(this);
        sitemapComponentRegistry.getAll().forEach(element -> added(element));
        this.sitemapComponentRegistry = sitemapComponentRegistry;
    protected void unsetComponentRegistryFactory(UIComponentRegistryFactory componentRegistryFactory) {
        UIComponentRegistry registry = this.sitemapComponentRegistry;
            registry.removeRegistryChangeListener(this);
        this.sitemaps = new HashMap<>();
        this.componentRegistryFactory = null;
        this.sitemapComponentRegistry = null;
        return sitemaps.values();
        if ("Sitemap".equals(element.getType())) {
            String sitemapName = SITEMAP_PREFIX + element.getUID();
            if (sitemaps.get(sitemapName) == null) {
                Sitemap sitemap = buildSitemap(element);
                sitemaps.put(sitemapName, sitemap);
            Sitemap sitemap = sitemaps.remove(sitemapName);
                notifyListenersAboutRemovedElement(sitemap);
        if ("Sitemap".equals(oldElement.getType()) && "Sitemap".equals(element.getType())) {
            String oldSitemapName = SITEMAP_PREFIX + oldElement.getUID();
            Sitemap oldSitemap = sitemaps.get(oldSitemapName);
            if (!oldSitemapName.equals(sitemapName)) {
                sitemaps.remove(oldSitemapName);
