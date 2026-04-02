import org.osgi.service.jaxrs.whiteboard.propertytypes.JaxrsApplicationBase;
 * The JAX-RS application for the openHAB JAX-RS resources.
@Component(service = Application.class, property = {
        // https://lists.apache.org/thread.html/
        // r1379789bd90c6b7e3971d5ffeedb2e0d1e1c9103fd2392cb95458596%40%3Cuser.aries.apache.org%3E
        "servlet.init.hide-service-list-page=true" })
@JaxrsName(RESTConstants.JAX_RS_NAME)
@JaxrsApplicationBase("rest")
public class RESTApplicationImpl extends Application {
