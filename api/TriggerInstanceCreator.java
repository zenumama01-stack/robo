 * This class creates {@link TriggerType} instances.
public class TriggerInstanceCreator implements InstanceCreator<CompositeTriggerType> {
    public CompositeTriggerType createInstance(@NonNullByDefault({}) Type type) {
        return new CompositeTriggerType(null, null, null, null);
