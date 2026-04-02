public class SelectionImpl extends NonLinkableWidgetImpl implements Selection {
    private List<Mapping> mappings = new CopyOnWriteArrayList<>();
    public SelectionImpl() {
    public SelectionImpl(Parent parent) {
    public List<Mapping> getMappings() {
    public void setMappings(List<Mapping> mappings) {
        this.mappings = new CopyOnWriteArrayList<>(mappings);
