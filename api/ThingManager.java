 * {@link ThingManager} interface defines methods for managing a {@link Thing}.
 * @author Yordan Zhelev - Initial contribution
public interface ThingManager {
     * This method gets the <b>enabled</b> status for a {@link Thing}.
     * The only {@link ThingStatus} which is NOT enabled is {@link ThingStatus} with
     * {@link ThingStatusDetail#DISABLED}.
     * @param thingUID UID of the {@link Thing}.
     * @return {@code false} when the {@link Thing} has {@link ThingStatus} with {@link ThingStatusDetail#DISABLED}.
     *         Returns {@code true} in all other cases.
    boolean isEnabled(ThingUID thingUID);
     * This method is used for changing <b>enabled</b> state of the {@link Thing}
     * @param isEnabled a new <b>enabled / disabled</b> state of the {@link Thing}.
    void setEnabled(ThingUID thingUID, boolean isEnabled);
