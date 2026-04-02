import org.openhab.core.model.script.actions.Ephemeris;
 * This class registers an OSGi service for the ephemeris action.
public class EphemerisActionService implements ActionService {
    public static @Nullable EphemerisManager ephemerisManager;
    public EphemerisActionService(final @Reference EphemerisManager ephemerisManager) {
        EphemerisActionService.ephemerisManager = ephemerisManager;
        return Ephemeris.class;
