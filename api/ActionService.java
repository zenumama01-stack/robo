 * This interface must be implemented by services that want to contribute script actions.
public interface ActionService {
     * returns the FQCN of the action class.
     * @return the FQCN of the action class
    default String getActionClassName() {
        return getActionClass().getCanonicalName();
     * Returns the action class itself
     * @return the action class
    Class<?> getActionClass();
