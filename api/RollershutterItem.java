 * A RollershutterItem allows the control of roller shutters, i.e.
 * moving them up, down, stopping or setting it to close to a certain percentage.
public class RollershutterItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(PercentType.class, UpDownType.class,
            UpDownType.class, StopMoveType.class, RefreshType.class);
    public RollershutterItem(String name) {
        super(CoreItemFactory.ROLLERSHUTTER, name);
     * Send an UP/DOWN command to the item.
    public void send(UpDownType command) {
    public void send(UpDownType command, @Nullable String source) {
     * Send a STOP/MOVE command to the item.
    public void send(StopMoveType command) {
    public void send(StopMoveType command, @Nullable String source) {
