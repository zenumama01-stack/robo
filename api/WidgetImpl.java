public class WidgetImpl implements Widget {
    private @Nullable Parent parent;
    private List<Rule> iconRules = new CopyOnWriteArrayList<>();
    private @Nullable Boolean staticIcon;
    private List<Rule> labelColorRules = new CopyOnWriteArrayList<>();
    private List<Rule> valueColorRules = new CopyOnWriteArrayList<>();
    private List<Rule> iconColorRules = new CopyOnWriteArrayList<>();
    private List<Rule> visibilityRules = new CopyOnWriteArrayList<>();
    public WidgetImpl() {
    public WidgetImpl(Parent parent) {
        this.parent = parent;
    public @Nullable Parent getParent() {
    public void setParent(Parent parent) {
    public List<Rule> getIconRules() {
        return iconRules;
    public void setIconRules(List<Rule> iconRules) {
        this.iconRules = new CopyOnWriteArrayList<>(iconRules);
    public boolean isStaticIcon() {
        return staticIcon == null ? false : staticIcon;
    public void setStaticIcon(@Nullable Boolean staticIcon) {
        this.staticIcon = staticIcon;
    public List<Rule> getLabelColor() {
        return labelColorRules;
    public void setLabelColor(List<Rule> labelColorRules) {
        this.labelColorRules = new CopyOnWriteArrayList<>(labelColorRules);
    public List<Rule> getValueColor() {
        return valueColorRules;
    public void setValueColor(List<Rule> valueColorRules) {
        this.valueColorRules = new CopyOnWriteArrayList<>(valueColorRules);
    public List<Rule> getIconColor() {
        return iconColorRules;
    public void setIconColor(List<Rule> iconColorRules) {
        this.iconColorRules = new CopyOnWriteArrayList<>(iconColorRules);
    public List<Rule> getVisibility() {
        return visibilityRules;
    public void setVisibility(List<Rule> visibilityRules) {
        this.visibilityRules = new CopyOnWriteArrayList<>(visibilityRules);
    public String getWidgetType() {
        return this.getClass().getInterfaces()[0].getSimpleName();
