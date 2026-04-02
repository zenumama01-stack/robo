import com.google.gson.InstanceCreator;
 * This class creates {@link ActionType} instances.
public class ActionInstanceCreator implements InstanceCreator<CompositeActionType> {
    public CompositeActionType createInstance(@NonNullByDefault({}) Type type) {
        return new CompositeActionType(null, null, null, null, null);
