import org.openhab.core.items.events.AbstractItemEventSubscriber;
 * The ItemUpdater listens on the event bus and passes any received status update
 * to the item registry.
@Component(immediate = true, service = EventSubscriber.class)
public class ItemUpdater extends AbstractItemEventSubscriber {
    private final Logger logger = LoggerFactory.getLogger(ItemUpdater.class);
    public ItemUpdater(final @Reference ItemRegistry itemRegistry) {
    protected void receiveUpdate(ItemStateEvent updateEvent) {
        String itemName = updateEvent.getItemName();
        State newState = updateEvent.getItemState();
            GenericItem item = (GenericItem) itemRegistry.getItem(itemName);
                for (Class<? extends State> state : item.getAcceptedDataTypes()) {
                    } catch (ReflectiveOperationException e) {
                        // Should never happen
                        logger.warn("{} while creating {} instance: {}", e.getClass().getSimpleName(),
                                state.getSimpleName(), e.getMessage());
                item.setState(newState, updateEvent.getSource());
                logger.debug("Received update of a not accepted type ({}) for item {}",
                        newState.getClass().getSimpleName(), itemName);
            logger.debug("Received update for non-existing item: {}", e.getMessage());
    protected void receiveCommand(ItemCommandEvent commandEvent) {
            Item item = itemRegistry.getItem(commandEvent.getItemName());
                groupItem.send(commandEvent.getItemCommand(), commandEvent.getSource());
            logger.debug("Received command for non-existing item: {}", e.getMessage());
    protected void receiveTimeSeries(ItemTimeSeriesEvent timeSeriesEvent) {
            Item item = itemRegistry.getItem(timeSeriesEvent.getItemName());
            if (!(item instanceof GroupItem) && item instanceof GenericItem genericItem) {
                genericItem.setTimeSeries(timeSeriesEvent.getTimeSeries());
