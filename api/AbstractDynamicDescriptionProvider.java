package org.openhab.core.thing.binding;
 * The {@link AbstractDynamicDescriptionProvider} provides a base implementation for dynamic description providers.
 * It holds a reference to the {@link ChannelTypeI18nLocalizationService} to provide localized descriptions. Therefore
 * the inheriting class has to request a reference for the {@link ChannelTypeI18nLocalizationService} on its own.
 * It posts {@link ChannelDescriptionChangedEvent}s through the openHAB events bus about a changed dynamic description.
 * Therefore the subclass has to request references for the {@link EventPublisher} and
 * {@link ItemChannelLinkRegistry}.
public abstract class AbstractDynamicDescriptionProvider {
    private final Logger logger = LoggerFactory.getLogger(getClass());
    protected @Nullable EventPublisher eventPublisher;
    protected @Nullable ChannelTypeI18nLocalizationService channelTypeI18nLocalizationService;
    protected @Nullable ItemChannelLinkRegistry itemChannelLinkRegistry;
     * This method can be used in a subclass in order to post events through the openHAB events bus. A common use case
     * is to notify event subscribers about a changed dynamic description.
     * @param event the {@link Event}
    protected void postEvent(Event event) {
                logger.error("Cannot post '{}' event: {}", event.getType(), e.getMessage(), e);
            logger.debug("Cannot post event as EventPublisher is missing");
    protected void activate(ComponentContext componentContext) {
        bundleContext = componentContext.getBundleContext();
