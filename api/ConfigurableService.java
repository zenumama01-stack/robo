import org.osgi.service.component.annotations.ComponentPropertyType;
 * {@link ConfigurableService} can be used as a marker interface for configurable services. But the interface itself is
 * not relevant for the runtime. Each service which has the property
 * {@link ConfigurableService#description_uri} set will be considered as a configurable service. The
 * properties {@link ConfigurableService#label} and {@link ConfigurableService#category} are optional.
 * The services are configured through the OSGi configuration admin. Therefore each service must provide a PID or a
 * component name service property if the configuration is done by declarative services. If the
 * {@link Constants#SERVICE_PID} property is not set the
 * {@link ComponentConstants#COMPONENT_NAME} property will be used as fallback.
 * @author Wouter Born - Change to ComponentPropertyType
@ComponentPropertyType
@Retention(RetentionPolicy.CLASS)
public @interface ConfigurableService {
    String PREFIX_ = "service.config.";
     * The config description URI for the configurable service. See also {@link ConfigDescription}.
    String description_uri();
     * The label of the service to be configured.
     * The category of the service to be configured (e.g. binding).
    String category() default "";
     * Marker for multiple configurations for this service ("true" = multiple configurations possible)
    boolean factory() default false;
