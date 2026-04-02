import org.openhab.core.thing.binding.BaseDynamicStateDescriptionProvider;
import org.openhab.core.thing.type.DynamicStateDescriptionProvider;
 * Dynamic provider of state options while leaving other state description fields as original.
@Component(service = { DynamicStateDescriptionProvider.class, MagicDynamicStateDescriptionProvider.class })
public class MagicDynamicStateDescriptionProvider extends BaseDynamicStateDescriptionProvider {
    public MagicDynamicStateDescriptionProvider(final @Reference EventPublisher eventPublisher, //
        LoggerFactory.getLogger(getClass()).error("called activate constructor");
        LoggerFactory.getLogger(getClass()).error("called activate method");
