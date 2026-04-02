public class SliderImpl extends NonLinkableWidgetImpl implements Slider {
    private @Nullable Boolean switchEnabled;
    private @Nullable Boolean releaseOnly;
    public SliderImpl() {
    public SliderImpl(Parent parent) {
    public boolean isSwitchEnabled() {
        return switchEnabled != null ? switchEnabled : false;
    public void setSwitchEnabled(@Nullable Boolean switchEnabled) {
        this.switchEnabled = switchEnabled;
    public boolean isReleaseOnly() {
        return releaseOnly != null ? releaseOnly : false;
    public void setReleaseOnly(@Nullable Boolean releaseOnly) {
        this.releaseOnly = releaseOnly;
