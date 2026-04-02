import javax.ws.rs.core.CacheControl;
 * Public constants for the REST API
public class RESTConstants {
    public static final String REST_URI = "/rest";
    public static final String JAX_RS_NAME = "openhab";
     * Version of the openHAB API
     * Version 1: initial version
     * Version 2: include invisible widgets into sitemap response (#499)
     * Version 3: Addition of anyFormat icon parameter (#978)
     * Version 4: OH3, refactored extensions to addons (#1560)
     * Version 5: transparent charts (#2502)
     * Version 6: extended chart period parameter format (#3863)
     * Version 7: extended chart period parameter format to cover past and future
     * Version 8: Buttongrid as container for new Button elements
    public static final String API_VERSION = "8";
    public static final CacheControl CACHE_CONTROL = new CacheControl();
        CACHE_CONTROL.setNoCache(true);
        CACHE_CONTROL.setMustRevalidate(true);
        CACHE_CONTROL.setPrivate(true);
