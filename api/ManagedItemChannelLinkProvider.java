import org.openhab.core.common.registry.DefaultAbstractManagedProvider;
 * {@link ManagedItemChannelLinkProvider} is responsible for managed {@link ItemChannelLink}s at runtime.
@Component(immediate = true, service = { ItemChannelLinkProvider.class, ManagedItemChannelLinkProvider.class })
public class ManagedItemChannelLinkProvider extends DefaultAbstractManagedProvider<ItemChannelLink, String>
        implements ItemChannelLinkProvider {
    public ManagedItemChannelLinkProvider(final @Reference StorageService storageService) {
        return ItemChannelLink.class.getName();
