@Component(immediate = true, service = MagicMultiInstanceServiceMarker.class, //
        property = Constants.SERVICE_PID + "=org.openhab.magicMultiInstance")
@ConfigurableService(category = "test", label = "Magic Multi Instance Service", description_uri = "test:multipleMagic", factory = true)
public class MagicMultiInstanceServiceMarker {
    // this is a marker service and represents a service factory so multiple configuration instances of type
    // "org.openhab.core.magicMultiInstance" can be created.
