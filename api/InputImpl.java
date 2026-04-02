public class InputImpl extends NonLinkableWidgetImpl implements Input {
    private @Nullable String inputHint;
    public InputImpl() {
    public InputImpl(Parent parent) {
    public @Nullable String getInputHint() {
        return inputHint;
    public void setInputHint(@Nullable String inputHint) {
        this.inputHint = inputHint;
