public class ButtonImpl extends NonLinkableWidgetImpl implements Button {
    private @Nullable Boolean stateless;
    private @Nullable String releaseCmd;
    public ButtonImpl() {
    public ButtonImpl(Parent parent) {
        super(parent);
    public boolean isStateless() {
        return stateless != null ? stateless : false;
    public void setStateless(@Nullable Boolean stateless) {
        this.stateless = stateless;
    public @Nullable String getReleaseCmd() {
        return releaseCmd;
    public void setReleaseCmd(@Nullable String releaseCmd) {
        this.releaseCmd = releaseCmd;
