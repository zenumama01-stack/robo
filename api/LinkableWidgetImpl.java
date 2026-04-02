public class LinkableWidgetImpl extends WidgetImpl implements LinkableWidget {
    private List<Widget> widgets = new CopyOnWriteArrayList<>();
    public LinkableWidgetImpl() {
    public LinkableWidgetImpl(Parent parent) {
    public List<Widget> getWidgets() {
    public void setWidgets(List<Widget> widgets) {
        this.widgets = new CopyOnWriteArrayList<>(widgets);
