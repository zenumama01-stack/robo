import org.openhab.core.events.EventSubscriber;
import org.openhab.core.thing.ThingProvider;
import org.openhab.core.thing.ThingStatus;
import org.openhab.core.thing.events.ThingStatusInfoEvent;
 * This {@link ThingProvider} keeps things provided by scripts during runtime.
 * This ensures that things are not kept on reboot, but have to be provided by the scripts again.
@Component(immediate = true, service = { ScriptedThingProvider.class, ThingProvider.class, EventSubscriber.class })
public class ScriptedThingProvider extends AbstractProvider<Thing>
        implements ThingProvider, ManagedProvider<Thing, ThingUID>, EventSubscriber {
    private final Logger logger = LoggerFactory.getLogger(ScriptedThingProvider.class);
    private final Map<ThingUID, Thing> things = new HashMap<>();
        return things.values();
        return things.get(uid);
    public void add(Thing thing) {
        if (things.get(thing.getUID()) != null) {
                    "Cannot add thing, because a thing with same UID (" + thing.getUID() + ") already exists.");
        things.put(thing.getUID(), thing);
        notifyListenersAboutAddedElement(thing);
    public @Nullable Thing update(Thing thing) {
        Thing oldThing = things.get(thing.getUID());
        if (oldThing != null) {
            notifyListenersAboutUpdatedElement(oldThing, thing);
            logger.warn("Cannot update thing with UID '{}', because it does not exist.", thing.getUID());
        return oldThing;
    public @Nullable Thing remove(ThingUID uid) {
        Thing thing = things.remove(uid);
        if (thing != null) {
            notifyListenersAboutRemovedElement(thing);
        return thing;
    public Set<String> getSubscribedEventTypes() {
        return Set.of(ThingStatusInfoEvent.TYPE);
    public void receive(Event event) {
        if (event instanceof ThingStatusInfoEvent thingStatusInfoEvent) {
            if (thingStatusInfoEvent.getStatusInfo().getStatus() == ThingStatus.REMOVED) {
                remove(thingStatusInfoEvent.getThingUID());
