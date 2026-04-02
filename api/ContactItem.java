 * A ContactItem can be used for sensors that return an "open" or "close" as a state.
 * This is useful for doors, windows, etc.
public class ContactItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(OpenClosedType.class,
    public ContactItem(String name) {
        super(CoreItemFactory.CONTACT, name);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof OpenClosedType)) {
