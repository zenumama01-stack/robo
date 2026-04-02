public class DefaultImpl extends NonLinkableWidgetImpl implements Default {
    private @Nullable Integer height;
    public DefaultImpl() {
    public DefaultImpl(Parent parent) {
    public int getHeight() {
        return height != null ? height : 0;
    public void setHeight(@Nullable Integer height) {
