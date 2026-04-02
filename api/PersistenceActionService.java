import org.openhab.core.persistence.extensions.PersistenceExtensions;
 * This class registers an OSGi service for the Persistence action.
public class PersistenceActionService implements ActionService {
        return PersistenceExtensions.class;
