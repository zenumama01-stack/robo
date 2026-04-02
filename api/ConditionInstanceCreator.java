 * This class creates {@link ConditionType} instances.
public class ConditionInstanceCreator implements InstanceCreator<CompositeConditionType> {
    public CompositeConditionType createInstance(@NonNullByDefault({}) Type type) {
        return new CompositeConditionType(null, null, null, null);
