import org.openhab.core.thing.binding.BaseDynamicCommandDescriptionProvider;
import org.openhab.core.thing.i18n.ChannelTypeI18nLocalizationService;
import org.openhab.core.thing.type.DynamicCommandDescriptionProvider;
 * Dynamic provider of command options.
@Component(service = { DynamicCommandDescriptionProvider.class, MagicDynamicCommandDescriptionProvider.class })
public class MagicDynamicCommandDescriptionProvider extends BaseDynamicCommandDescriptionProvider {
    public MagicDynamicCommandDescriptionProvider(final @Reference EventPublisher eventPublisher, //
            final @Reference ChannelTypeI18nLocalizationService channelTypeI18nLocalizationService, //
            final @Reference ItemChannelLinkRegistry itemChannelLinkRegistry) {
        this.channelTypeI18nLocalizationService = channelTypeI18nLocalizationService;
