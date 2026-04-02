 * This class registers an OSGi service for the BusEvent action.
public class BusEventActionService implements ActionService {
    private static @Nullable BusEvent busEvent;
    public BusEventActionService(final @Reference BusEvent busEvent) {
        BusEventActionService.busEvent = busEvent;
        return BusEvent.class;
    public static BusEvent getBusEvent() {
        return Objects.requireNonNull(busEvent);
