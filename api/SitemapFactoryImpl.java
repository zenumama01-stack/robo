package org.openhab.core.sitemap.internal.registry;
import org.openhab.core.sitemap.internal.ButtonDefinitionImpl;
import org.openhab.core.sitemap.internal.ButtonImpl;
import org.openhab.core.sitemap.internal.ButtongridImpl;
import org.openhab.core.sitemap.internal.ChartImpl;
import org.openhab.core.sitemap.internal.ColorpickerImpl;
import org.openhab.core.sitemap.internal.ColortemperaturepickerImpl;
import org.openhab.core.sitemap.internal.ConditionImpl;
import org.openhab.core.sitemap.internal.DefaultImpl;
import org.openhab.core.sitemap.internal.FrameImpl;
import org.openhab.core.sitemap.internal.GroupImpl;
import org.openhab.core.sitemap.internal.ImageImpl;
import org.openhab.core.sitemap.internal.InputImpl;
import org.openhab.core.sitemap.internal.MappingImpl;
import org.openhab.core.sitemap.internal.MapviewImpl;
import org.openhab.core.sitemap.internal.RuleImpl;
import org.openhab.core.sitemap.internal.SelectionImpl;
import org.openhab.core.sitemap.internal.SetpointImpl;
import org.openhab.core.sitemap.internal.SitemapImpl;
import org.openhab.core.sitemap.internal.SliderImpl;
import org.openhab.core.sitemap.internal.SwitchImpl;
import org.openhab.core.sitemap.internal.TextImpl;
import org.openhab.core.sitemap.internal.VideoImpl;
import org.openhab.core.sitemap.internal.WebviewImpl;
 * The {@link SitemapFactoryImpl} implements the {@link SitemapFactory}
@Component(service = SitemapFactory.class, immediate = true)
public class SitemapFactoryImpl implements SitemapFactory {
    // Sitemap widget types
    public static final String BUTTON = "Button";
    public static final String BUTTON_GRID = "Buttongrid";
    public static final String CHART = "Chart";
    public static final String COLOR_PICKER = "Colorpicker";
    public static final String COLOR_TEMPERATURE_PICKER = "Colortemperaturepicker";
    public static final String DEFAULT = "Default";
    public static final String FRAME = "Frame";
    public static final String GROUP = "Group";
    public static final String IMAGE = "Image";
    public static final String INPUT = "Input";
    public static final String MAPVIEW = "Mapview";
    public static final String SELECTION = "Selection";
    public static final String SETPOINT = "Setpoint";
    public static final String SLIDER = "Slider";
    public static final String SWITCH = "Switch";
    public static final String TEXT = "Text";
    public static final String VIDEO = "Video";
    public static final String WEBVIEW = "Webview";
    private static final String[] WIDGET_TYPES = { BUTTON, BUTTON_GRID, CHART, COLOR_PICKER, COLOR_TEMPERATURE_PICKER,
            DEFAULT, FRAME, GROUP, IMAGE, INPUT, MAPVIEW, SELECTION, SETPOINT, SLIDER, SWITCH, TEXT, VIDEO, WEBVIEW };
    public Sitemap createSitemap(String sitemapName) {
        return new SitemapImpl(sitemapName);
    public @Nullable Widget createWidget(String widgetTypeName) {
        return switch (widgetTypeName) {
            case BUTTON -> new ButtonImpl();
            case BUTTON_GRID -> new ButtongridImpl();
            case CHART -> new ChartImpl();
            case COLOR_PICKER -> new ColorpickerImpl();
            case COLOR_TEMPERATURE_PICKER -> new ColortemperaturepickerImpl();
            case DEFAULT -> new DefaultImpl();
            case FRAME -> new FrameImpl();
            case GROUP -> new GroupImpl();
            case IMAGE -> new ImageImpl();
            case INPUT -> new InputImpl();
            case MAPVIEW -> new MapviewImpl();
            case SELECTION -> new SelectionImpl();
            case SETPOINT -> new SetpointImpl();
            case SLIDER -> new SliderImpl();
            case SWITCH -> new SwitchImpl();
            case TEXT -> new TextImpl();
            case VIDEO -> new VideoImpl();
            case WEBVIEW -> new WebviewImpl();
    public @Nullable Widget createWidget(String widgetTypeName, Parent parent) {
            case BUTTON -> new ButtonImpl(parent);
            case BUTTON_GRID -> new ButtongridImpl(parent);
            case CHART -> new ChartImpl(parent);
            case COLOR_PICKER -> new ColorpickerImpl(parent);
            case COLOR_TEMPERATURE_PICKER -> new ColortemperaturepickerImpl(parent);
            case DEFAULT -> new DefaultImpl(parent);
            case FRAME -> new FrameImpl(parent);
            case GROUP -> new GroupImpl(parent);
            case IMAGE -> new ImageImpl(parent);
            case INPUT -> new InputImpl(parent);
            case MAPVIEW -> new MapviewImpl(parent);
            case SELECTION -> new SelectionImpl(parent);
            case SETPOINT -> new SetpointImpl(parent);
            case SLIDER -> new SliderImpl(parent);
            case SWITCH -> new SwitchImpl(parent);
            case TEXT -> new TextImpl(parent);
            case VIDEO -> new VideoImpl(parent);
            case WEBVIEW -> new WebviewImpl(parent);
    public ButtonDefinition createButtonDefinition() {
        return new ButtonDefinitionImpl();
    public Mapping createMapping() {
        return new MappingImpl();
    public Rule createRule() {
        return new RuleImpl();
    public Condition createCondition() {
        return new ConditionImpl();
    public String[] getSupportedWidgetTypes() {
        return WIDGET_TYPES;
