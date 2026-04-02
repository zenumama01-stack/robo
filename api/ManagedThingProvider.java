import org.openhab.core.thing.internal.BridgeImpl;
import org.openhab.core.thing.internal.ThingStorageEntity;
 * {@link ManagedThingProvider} is an OSGi service, that allows to add or remove
 * things at runtime by calling {@link ManagedThingProvider#add} or
 * {@link ManagedThingProvider#remove}. An added thing is
 * automatically exposed to the {@link ThingRegistry}.
 * @author Dennis Nobel - Integrated Storage
 * @author Michael Grammling - Added dynamic configuration update
@Component(immediate = true, service = { ThingProvider.class, ManagedThingProvider.class })
public class ManagedThingProvider extends AbstractManagedProvider<Thing, ThingUID, ThingStorageEntity>
        implements ThingProvider {
    public ManagedThingProvider(final @Reference StorageService storageService) {
        return Thing.class.getName();
    protected String keyToString(ThingUID key) {
        return key.toString();
    protected @Nullable Thing toElement(String key, ThingStorageEntity persistableElement) {
            return ThingDTOMapper.map(persistableElement, persistableElement.isBridge);
            logger.warn("Failed to create thing with UID '{}' from stored entity: {}", key, e.getMessage());
    protected ThingStorageEntity toPersistableElement(Thing element) {
        return new ThingStorageEntity(ThingDTOMapper.map(element), element instanceof BridgeImpl);
