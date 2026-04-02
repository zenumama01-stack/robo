 * Classes implementing this interface can be registered as an OSGi service in order to provide functionality for
 * managing add-ons, such as listing, installing and uninstalling them.
 * The REST API offers a uri that exposes this functionality.
 * @author Yannick Schaus - Add id, name and refreshSource
public interface AddonService {
     * Returns the ID of the service.
     * @return the service identifier
    String getId();
     * Returns the name of the service.
     * @return the service name
    String getName();
     * Refreshes the source used for providing the add-ons.
     * This can be called before getAddons to ensure the add-on information is up-to-date; otherwise they might be
     * retrieved from a cache.
    void refreshSource();
     * Retrieves all add-ons.
     * It is expected that this method is rather cheap to call and will return quickly, i.e. some caching should be
     * implemented if required.
     * @param locale the locale to use for the result
     * @return the localized add-ons
    List<Addon> getAddons(@Nullable Locale locale);
     * Retrieves the add-on for the given id.
     * @return the localized add-on or <code>null</code>, if no add-on exists with this id
    Addon getAddon(String id, @Nullable Locale locale);
     * Retrieves all possible types of add-ons.
     * @return the localized types
    List<AddonType> getTypes(@Nullable Locale locale);
     * Installs the given add-on.
     * This can be a long running process. The framework makes sure that this is called within a separate thread and
     * add-on events will be sent upon its completion.
     * @param id the id of the add-on to install
    void install(String id);
     * Uninstalls the given add-on.
     * @param id the id of the add-on to uninstall
    void uninstall(String id);
     * Parses the given URI and extracts an add-on Id.
     * This must not be a long running process but return immediately.
     * @param addonURI the URI from which to parse the add-on Id.
     * @return the add-on Id if the URI can be parsed, otherwise <code>null</code>
    String getAddonId(URI addonURI);
