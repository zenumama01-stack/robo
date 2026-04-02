package org.openhab.core.model.sitemap.internal;
import org.openhab.core.model.sitemap.sitemap.ModelButton;
import org.openhab.core.model.sitemap.sitemap.ModelButtonDefinition;
import org.openhab.core.model.sitemap.sitemap.ModelButtonDefinitionList;
import org.openhab.core.model.sitemap.sitemap.ModelButtongrid;
import org.openhab.core.model.sitemap.sitemap.ModelChart;
import org.openhab.core.model.sitemap.sitemap.ModelColorArray;
import org.openhab.core.model.sitemap.sitemap.ModelColorArrayList;
import org.openhab.core.model.sitemap.sitemap.ModelColortemperaturepicker;
import org.openhab.core.model.sitemap.sitemap.ModelCondition;
import org.openhab.core.model.sitemap.sitemap.ModelDefault;
import org.openhab.core.model.sitemap.sitemap.ModelIconRule;
import org.openhab.core.model.sitemap.sitemap.ModelIconRuleList;
import org.openhab.core.model.sitemap.sitemap.ModelImage;
import org.openhab.core.model.sitemap.sitemap.ModelInput;
import org.openhab.core.model.sitemap.sitemap.ModelLinkableWidget;
import org.openhab.core.model.sitemap.sitemap.ModelMapping;
import org.openhab.core.model.sitemap.sitemap.ModelMappingList;
import org.openhab.core.model.sitemap.sitemap.ModelMapview;
import org.openhab.core.model.sitemap.sitemap.ModelSelection;
import org.openhab.core.model.sitemap.sitemap.ModelSetpoint;
import org.openhab.core.model.sitemap.sitemap.ModelSitemap;
import org.openhab.core.model.sitemap.sitemap.ModelSlider;
import org.openhab.core.model.sitemap.sitemap.ModelSwitch;
import org.openhab.core.model.sitemap.sitemap.ModelVideo;
import org.openhab.core.model.sitemap.sitemap.ModelVisibilityRule;
import org.openhab.core.model.sitemap.sitemap.ModelVisibilityRuleList;
import org.openhab.core.model.sitemap.sitemap.ModelWebview;
import org.openhab.core.model.sitemap.sitemap.ModelWidget;
import org.openhab.core.sitemap.Default;
import org.openhab.core.sitemap.registry.SitemapFactory;
import org.openhab.core.sitemap.registry.SitemapProvider;
 * This class provides access to the sitemap model files.
 * @author Mark Herwege - Separate registry from model
@Component(service = SitemapProvider.class, immediate = true)
public class SitemapProviderImpl extends AbstractProvider<Sitemap>
        implements SitemapProvider, ModelRepositoryChangeListener {
    private static final String SITEMAP_MODEL_NAME = "sitemap";
    protected static final String SITEMAP_FILEEXT = "." + SITEMAP_MODEL_NAME;
    protected static final String MODEL_TYPE_PREFIX = "Model";
    private final Logger logger = LoggerFactory.getLogger(SitemapProviderImpl.class);
    private final ModelRepository modelRepo;
    private final SitemapFactory sitemapFactory;
    private final Map<String, Sitemap> sitemapCache = new ConcurrentHashMap<>();
    public SitemapProviderImpl(final @Reference ModelRepository modelRepo,
            final @Reference SitemapRegistry sitemapRegistry, final @Reference SitemapFactory sitemapFactory) {
        this.modelRepo = modelRepo;
        this.sitemapFactory = sitemapFactory;
        refreshSitemapModels();
        sitemapRegistry.addSitemapProvider(this);
        modelRepo.addModelRepositoryChangeListener(this);
        modelRepo.removeModelRepositoryChangeListener(this);
        sitemapRegistry.removeSitemapProvider(this);
        sitemapCache.clear();
    public @Nullable Sitemap getSitemap(String sitemapName) {
        Sitemap sitemap = sitemapCache.get(sitemapName);
            logger.trace("Sitemap {} cannot be found", sitemapName);
        return sitemap;
    private void refreshSitemapModels() {
        Iterable<String> sitemapFilenames = modelRepo.getAllModelNamesOfType(SITEMAP_MODEL_NAME);
        for (String filename : sitemapFilenames) {
            ModelSitemap modelSitemap = (ModelSitemap) modelRepo.getModel(filename);
            if (modelSitemap != null) {
                String sitemapFileName = filename.substring(0, filename.length() - SITEMAP_FILEEXT.length());
                String sitemapName = modelSitemap.getName();
                if (!sitemapFileName.equals(sitemapName)) {
                    logger.warn("Filename '{}' does not match the name '{}' of the sitemap - ignoring sitemap.",
                            filename, sitemapName);
                    Sitemap sitemap = parseModelSitemap(modelSitemap);
                    sitemapCache.put(sitemapName, sitemap);
    private Sitemap parseModelSitemap(ModelSitemap modelSitemap) {
        Sitemap sitemap = sitemapFactory.createSitemap(modelSitemap.getName());
        sitemap.setLabel(modelSitemap.getLabel());
        sitemap.setIcon(modelSitemap.getIcon());
        List<Widget> widgets = sitemap.getWidgets();
        modelSitemap.getChildren().forEach(child -> addWidget(widgets, child, sitemap));
    private void addWidget(List<Widget> widgets, ModelWidget modelWidget, Parent parent) {
        String widgetType = getWidgetType(modelWidget);
        Widget widget = sitemapFactory.createWidget(widgetType, parent);
        if (widget != null) {
            switch (widget) {
                case Image imageWidget:
                    ModelImage modelImage = (ModelImage) modelWidget;
                    imageWidget.setUrl(modelImage.getUrl());
                    imageWidget.setRefresh(modelImage.getRefresh());
                case Video videoWidget:
                    ModelVideo modelVideo = (ModelVideo) modelWidget;
                    videoWidget.setUrl(modelVideo.getUrl());
                    videoWidget.setEncoding(modelVideo.getEncoding());
                case Chart chartWidget:
                    ModelChart modelChart = (ModelChart) modelWidget;
                    chartWidget.setService(modelChart.getService());
                    chartWidget.setRefresh(modelChart.getRefresh());
                    chartWidget.setPeriod(modelChart.getPeriod());
                    chartWidget.setLegend(modelChart.getLegend());
                    chartWidget.setForceAsItem(modelChart.getForceAsItem());
                    chartWidget.setYAxisDecimalPattern(modelChart.getYAxisDecimalPattern());
                    chartWidget.setInterpolation(modelChart.getInterpolation());
                case Webview webviewWidget:
                    ModelWebview modelWebview = (ModelWebview) modelWidget;
                    webviewWidget.setHeight(modelWebview.getHeight());
                    webviewWidget.setUrl(modelWebview.getUrl());
                case Switch switchWidget:
                    ModelSwitch modelSwitch = (ModelSwitch) modelWidget;
                    addWidgetMappings(switchWidget.getMappings(), modelSwitch.getMappings());
                case Mapview mapviewWidget:
                    ModelMapview modelMapview = (ModelMapview) modelWidget;
                    mapviewWidget.setHeight(modelMapview.getHeight());
                case Slider sliderWidget:
                    ModelSlider modelSlider = (ModelSlider) modelWidget;
                    sliderWidget.setMinValue(modelSlider.getMinValue());
                    sliderWidget.setMaxValue(modelSlider.getMaxValue());
                    sliderWidget.setStep(modelSlider.getStep());
                    sliderWidget.setSwitchEnabled(modelSlider.isSwitchEnabled());
                    sliderWidget.setReleaseOnly(modelSlider.isReleaseOnly());
                case Selection selectionWidget:
                    ModelSelection modelSelection = (ModelSelection) modelWidget;
                    addWidgetMappings(selectionWidget.getMappings(), modelSelection.getMappings());
                case Input inputWidget:
                    ModelInput modelInput = (ModelInput) modelWidget;
                    inputWidget.setInputHint(modelInput.getInputHint());
                case Setpoint setpointWidget:
                    ModelSetpoint modelSetpoint = (ModelSetpoint) modelWidget;
                    setpointWidget.setMinValue(modelSetpoint.getMinValue());
                    setpointWidget.setMaxValue(modelSetpoint.getMaxValue());
                    setpointWidget.setStep(modelSetpoint.getStep());
                case Colortemperaturepicker colortemperaturepickerWidget:
                    ModelColortemperaturepicker modelColortemperaturepicker = (ModelColortemperaturepicker) modelWidget;
                    colortemperaturepickerWidget.setMinValue(modelColortemperaturepicker.getMinValue());
                    colortemperaturepickerWidget.setMaxValue(modelColortemperaturepicker.getMaxValue());
                case Buttongrid buttongridWidget:
                    ModelButtongrid modelButtongrid = (ModelButtongrid) modelWidget;
                    addWidgetButtons(buttongridWidget.getButtons(), modelButtongrid.getButtons());
                case Button buttonWidget:
                    ModelButton modelButton = (ModelButton) modelWidget;
                    buttonWidget.setRow(modelButton.getRow());
                    buttonWidget.setColumn(modelButton.getColumn());
                    buttonWidget.setStateless(modelButton.isStateless());
                    buttonWidget.setCmd(modelButton.getCmd());
                    buttonWidget.setReleaseCmd(modelButton.getReleaseCmd());
                case Default defaultWidget:
                    ModelDefault modelDefault = (ModelDefault) modelWidget;
                    defaultWidget.setHeight(modelDefault.getHeight());
            widget.setItem(modelWidget.getItem());
            widget.setLabel(modelWidget.getLabel());
            String staticIcon = modelWidget.getStaticIcon();
            if (staticIcon != null && !staticIcon.isEmpty()) {
                widget.setIcon(staticIcon);
                widget.setStaticIcon(true);
                widget.setIcon(modelWidget.getIcon());
            if (modelWidget instanceof ModelLinkableWidget modelLinkableWidget) {
                LinkableWidget linkableWidget = (LinkableWidget) widget;
                List<Widget> childWidgets = linkableWidget.getWidgets();
                modelLinkableWidget.getChildren()
                        .forEach(childModelWidget -> addWidget(childWidgets, childModelWidget, linkableWidget));
            addWidgetVisibilityRules(widget.getVisibility(), modelWidget.getVisibility());
            addWidgetColorRules(widget.getLabelColor(), modelWidget.getLabelColor());
            addWidgetColorRules(widget.getValueColor(), modelWidget.getValueColor());
            addWidgetColorRules(widget.getIconColor(), modelWidget.getIconColor());
            addWidgetIconRules(widget.getIconRules(), modelWidget.getIconRules());
            widgets.add(widget);
    private String getWidgetType(ModelWidget modelWidget) {
        String instanceTypeName = modelWidget.eClass().getInstanceTypeName();
        String widgetType = instanceTypeName
                .substring(instanceTypeName.lastIndexOf("." + MODEL_TYPE_PREFIX) + MODEL_TYPE_PREFIX.length() + 1);
        return widgetType;
    private void addWidgetMappings(List<Mapping> mappings, @Nullable ModelMappingList modelMappingList) {
        if (modelMappingList != null) {
            EList<ModelMapping> modelMappings = modelMappingList.getElements();
            modelMappings.forEach(modelMapping -> {
                Mapping mapping = sitemapFactory.createMapping();
                mapping.setCmd(modelMapping.getCmd());
                mapping.setReleaseCmd(modelMapping.getReleaseCmd());
                mapping.setLabel(modelMapping.getLabel());
                mapping.setIcon(modelMapping.getIcon());
                mappings.add(mapping);
    private void addWidgetButtons(List<ButtonDefinition> buttons, @Nullable ModelButtonDefinitionList modelButtonList) {
        if (modelButtonList != null) {
            EList<ModelButtonDefinition> modelButtons = modelButtonList.getElements();
            modelButtons.forEach(modelButton -> {
                ButtonDefinition button = sitemapFactory.createButtonDefinition();
                button.setRow(modelButton.getRow());
                button.setColumn(modelButton.getColumn());
                button.setCmd(modelButton.getCmd());
                button.setLabel(modelButton.getLabel());
                button.setIcon(modelButton.getIcon());
                buttons.add(button);
    private void addWidgetVisibilityRules(List<Rule> visibilityRules,
            @Nullable ModelVisibilityRuleList modelVisibilityRuleList) {
        if (modelVisibilityRuleList != null) {
            EList<ModelVisibilityRule> modelVisibilityRules = modelVisibilityRuleList.getElements();
            modelVisibilityRules.forEach(modelVisibilityRule -> {
                Rule visibilityRule = sitemapFactory.createRule();
                addRuleConditions(visibilityRule.getConditions(), modelVisibilityRule.getConditions());
                visibilityRules.add(visibilityRule);
    private void addWidgetColorRules(List<Rule> colorRules, @Nullable ModelColorArrayList modelColorRuleList) {
        if (modelColorRuleList != null) {
            EList<ModelColorArray> modelColorRules = modelColorRuleList.getElements();
            modelColorRules.forEach(modelColorRule -> {
                Rule colorRule = sitemapFactory.createRule();
                addRuleConditions(colorRule.getConditions(), modelColorRule.getConditions());
                colorRule.setArgument(modelColorRule.getArg());
                colorRules.add(colorRule);
    private void addWidgetIconRules(List<Rule> iconRules, @Nullable ModelIconRuleList modelIconRuleList) {
        if (modelIconRuleList != null) {
            EList<ModelIconRule> modelIconRules = modelIconRuleList.getElements();
            modelIconRules.forEach(modelIconRule -> {
                Rule iconRule = sitemapFactory.createRule();
                addRuleConditions(iconRule.getConditions(), modelIconRule.getConditions());
                iconRule.setArgument(modelIconRule.getArg());
                iconRules.add(iconRule);
    private void addRuleConditions(List<Condition> conditions, EList<ModelCondition> modelConditions) {
        modelConditions.forEach(modelCondition -> {
            Condition condition = sitemapFactory.createCondition();
            condition.setItem(modelCondition.getItem());
            condition.setCondition(modelCondition.getCondition());
            String sign = modelCondition.getSign();
            String value = (sign != null ? sign : "") + modelCondition.getState();
            condition.setValue(value);
            conditions.add(condition);
    public Set<String> getSitemapNames() {
        return sitemapCache.keySet();
        if (!modelName.endsWith(SITEMAP_FILEEXT)) {
        Sitemap sitemap = null;
        String sitemapFileName = modelName.substring(0, modelName.length() - SITEMAP_FILEEXT.length());
            Sitemap oldSitemap = sitemapCache.remove(sitemapFileName);
            if (oldSitemap != null) {
                notifyListenersAboutRemovedElement(oldSitemap);
            EObject modelSitemapObject = modelRepo.getModel(modelName);
            // if the sitemap file is empty it will not be in the repo and thus there is no need to cache it here
            if (modelSitemapObject instanceof ModelSitemap modelSitemap) {
                            sitemapFileName, sitemapName);
                    sitemap = parseModelSitemap(modelSitemap);
                    Sitemap oldSitemap = sitemapCache.put(sitemapName, sitemap);
                        notifyListenersAboutUpdatedElement(oldSitemap, sitemap);
                        notifyListenersAboutAddedElement(sitemap);
                    // Previously valid sitemap is now invalid, so no ModelSitemap was created
    public Collection<Sitemap> getAll() {
        return sitemapCache.values();
