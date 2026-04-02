 * managing add-on suggestion finders, such as installing and uninstalling them.
public interface AddonFinderService {
     * Installs the given add-on suggestion finder.
     * This can be a long running process. The framework makes sure that this is called within a separate thread.
     * @param id the id of the add-on suggestion finder to install
     * Uninstalls the given add-on suggestion finder.
     * @param id the id of the add-on suggestion finder to uninstall
