public class VideoImpl extends NonLinkableWidgetImpl implements Video {
    private String url = "";
    private @Nullable String encoding;
    public VideoImpl() {
    public VideoImpl(Parent parent) {
    public String getUrl() {
    public void setUrl(String url) {
    public @Nullable String getEncoding() {
    public void setEncoding(@Nullable String encoding) {
        this.encoding = encoding;
