 * @author Denis Nobel - Initial contribution
public class BridgeImpl extends ThingImpl implements Bridge {
    private final transient Logger logger = LoggerFactory.getLogger(BridgeImpl.class);
    private transient Map<ThingUID, Thing> things = new ConcurrentHashMap<>();
    BridgeImpl() {
    public BridgeImpl(ThingTypeUID thingTypeUID, String bridgeId) {
        super(thingTypeUID, bridgeId);
     * @param thingTypeUID
    public BridgeImpl(ThingTypeUID thingTypeUID, ThingUID thingUID) throws IllegalArgumentException {
    public void addThing(Thing thing) {
    public void removeThing(Thing thing) {
        things.remove(thing.getUID());
    public @Nullable Thing getThing(ThingUID thingUID) {
        return things.get(thingUID);
    public List<Thing> getThings() {
        return List.copyOf(things.values());
    public @Nullable BridgeHandler getHandler() {
        BridgeHandler bridgeHandler = null;
        ThingHandler thingHandler = super.getHandler();
        if (thingHandler instanceof BridgeHandler handler) {
            bridgeHandler = handler;
        } else if (thingHandler != null) {
            logger.warn("Handler of bridge '{}' must implement BridgeHandler interface.", getUID());
        return bridgeHandler;
        return super.toString().replace("Bridge=False", "Bridge=True");
