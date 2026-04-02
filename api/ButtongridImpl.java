public class ButtongridImpl extends LinkableWidgetImpl implements Buttongrid {
    private List<ButtonDefinition> buttons = new CopyOnWriteArrayList<>();
    public ButtongridImpl() {
    public ButtongridImpl(Parent parent) {
    public List<ButtonDefinition> getButtons() {
        return buttons;
    public void setButtons(List<ButtonDefinition> buttons) {
        this.buttons = new CopyOnWriteArrayList<>(buttons);
