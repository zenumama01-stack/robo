package org.openhab.core.magic.binding.internal;
import static org.openhab.core.magic.binding.MagicBindingConstants.THING_TYPE_CONFIG_THING;
 * The {@link MagicDiscoveryService} magically discovers magic things.
@Component(service = DiscoveryService.class, immediate = true)
public class MagicDiscoveryService extends AbstractDiscoveryService {
    public MagicDiscoveryService() throws IllegalArgumentException {
        super(Set.of(THING_TYPE_CONFIG_THING), 0);
        String serialNumber = createRandomSerialNumber();
        DiscoveryResult discoveryResult = DiscoveryResultBuilder
                .create(new ThingUID(THING_TYPE_CONFIG_THING, serialNumber))
                .withRepresentationProperty(Thing.PROPERTY_SERIAL_NUMBER)
                .withProperty(Thing.PROPERTY_SERIAL_NUMBER, serialNumber).withLabel("Magic Thing").build();
    private String createRandomSerialNumber() {
