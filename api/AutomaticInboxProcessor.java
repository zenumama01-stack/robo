package org.openhab.core.config.discovery.internal;
import static org.openhab.core.config.discovery.inbox.InboxPredicates.*;
import org.openhab.core.config.discovery.inbox.Inbox;
import org.openhab.core.config.discovery.inbox.InboxAutoApprovePredicate;
import org.openhab.core.config.discovery.inbox.InboxListener;
import org.openhab.core.events.AbstractTypedEventSubscriber;
import org.openhab.core.thing.type.ThingType;
import org.openhab.core.thing.type.ThingTypeRegistry;
 * This class implements a service to automatically ignore or approve {@link Inbox} entries of newly discovered things.
 * <strong>Automatically ignoring inbox entries</strong>
 * The {@link AutomaticInboxProcessor} service implements an {@link EventSubscriber} that is triggered
 * for each thing when coming ONLINE. {@link Inbox} entries with the same representation value like the
 * newly created thing will be automatically set to {@link DiscoveryResultFlag#IGNORED}.
 * If a thing is being removed, possibly existing {@link Inbox} entries with the same representation value
 * are removed from the {@link Inbox} so they could be discovered again afterwards.
 * Automatically ignoring inbox entries can be enabled or disabled by setting the {@code autoIgnore} property to either
 * {@code true} or {@code false} via ConfigAdmin.
 * <strong>Automatically approving inbox entries</strong>
 * For each new discovery result, the {@link AutomaticInboxProcessor} queries all DS components implementing
 * {@link InboxAutoApprovePredicate} whether the result should be automatically approved.
 * If all new discovery results should be automatically approved (regardless of {@link InboxAutoApprovePredicate}s), the
 * {@code autoApprove} configuration property can be set to {@code true}.
 * @author Kai Kreuzer - added auto-approve functionality
 * @author Henning Sudbrock - added hook for selectively auto-approving inbox entries
@Component(immediate = true, configurationPid = "org.openhab.inbox", service = EventSubscriber.class, //
        property = Constants.SERVICE_PID + "=org.openhab.inbox")
@ConfigurableService(category = "system", label = "Inbox", description_uri = AutomaticInboxProcessor.CONFIG_URI)
public class AutomaticInboxProcessor extends AbstractTypedEventSubscriber<ThingStatusInfoChangedEvent>
        implements InboxListener, RegistryChangeListener<Thing> {
    public static final String AUTO_IGNORE_CONFIG_PROPERTY = "autoIgnore";
    public static final String ALWAYS_AUTO_APPROVE_CONFIG_PROPERTY = "autoApprove";
    protected static final String CONFIG_URI = "system:inbox";
    private final Logger logger = LoggerFactory.getLogger(AutomaticInboxProcessor.class);
    private final ThingTypeRegistry thingTypeRegistry;
    private final Inbox inbox;
    private boolean autoIgnore = true;
    private boolean alwaysAutoApprove = false;
    private final Set<InboxAutoApprovePredicate> inboxAutoApprovePredicates = new CopyOnWriteArraySet<>();
    public AutomaticInboxProcessor(final @Reference ThingTypeRegistry thingTypeRegistry,
            final @Reference ThingRegistry thingRegistry, final @Reference Inbox inbox) {
        super(ThingStatusInfoChangedEvent.TYPE);
        this.thingTypeRegistry = thingTypeRegistry;
        this.inbox = inbox;
    protected void activate(@Nullable Map<String, Object> properties) {
        thingRegistry.addRegistryChangeListener(this);
        inbox.addInboxListener(this);
    protected void modified(@Nullable Map<String, Object> properties) {
            Object value = properties.get(AUTO_IGNORE_CONFIG_PROPERTY);
            autoIgnore = value == null || !"false".equals(value.toString());
            value = properties.get(ALWAYS_AUTO_APPROVE_CONFIG_PROPERTY);
            alwaysAutoApprove = value != null && "true".equals(value.toString());
            autoApproveInboxEntries();
        inbox.removeInboxListener(this);
        thingRegistry.removeRegistryChangeListener(this);
    public void receiveTypedEvent(ThingStatusInfoChangedEvent event) {
        if (autoIgnore) {
            Thing thing = thingRegistry.get(event.getThingUID());
            ThingStatus thingStatus = event.getStatusInfo().getStatus();
            autoIgnore(thing, thingStatus);
    public void thingAdded(Inbox inbox, DiscoveryResult result) {
            String value = getRepresentationValue(result);
                Optional<Thing> thing = thingRegistry.stream()
                        .filter(t -> Objects.equals(value, getRepresentationPropertyValueForThing(t)))
                        .filter(t -> Objects.equals(t.getThingTypeUID(), result.getThingTypeUID())).findFirst();
                if (thing.isPresent()) {
                    logger.debug("Auto-ignoring the inbox entry for the representation value '{}'.", value);
                    inbox.setFlag(result.getThingUID(), DiscoveryResultFlag.IGNORED);
        if (alwaysAutoApprove || isToBeAutoApproved(result)) {
            inbox.approve(result.getThingUID(), result.getLabel(), null);
    public void thingUpdated(Inbox inbox, DiscoveryResult result) {
    public void thingRemoved(Inbox inbox, DiscoveryResult result) {
    public void added(Thing element) {
    public void removed(Thing element) {
        removePossiblyIgnoredResultInInbox(element);
    public void updated(Thing oldElement, Thing element) {
    private @Nullable String getRepresentationValue(DiscoveryResult result) {
        return result.getRepresentationProperty() != null
                ? Objects.toString(result.getProperties().get(result.getRepresentationProperty()), null)
    private void autoIgnore(@Nullable Thing thing, ThingStatus thingStatus) {
        if (ThingStatus.ONLINE.equals(thingStatus)) {
            checkAndIgnoreInInbox(thing);
    private void checkAndIgnoreInInbox(@Nullable Thing thing) {
            String representationValue = getRepresentationPropertyValueForThing(thing);
            if (representationValue != null) {
                ignoreInInbox(thing.getThingTypeUID(), representationValue);
    private void ignoreInInbox(ThingTypeUID thingtypeUID, String representationValue) {
        List<DiscoveryResult> results = inbox.stream().filter(withRepresentationPropertyValue(representationValue))
                .filter(forThingTypeUID(thingtypeUID)).toList();
        if (results.size() == 1) {
            logger.debug("Auto-ignoring the inbox entry for the representation value '{}'.", representationValue);
            inbox.setFlag(results.getFirst().getThingUID(), DiscoveryResultFlag.IGNORED);
    private void removePossiblyIgnoredResultInInbox(@Nullable Thing thing) {
                removeFromInbox(thing.getThingTypeUID(), representationValue);
    private @Nullable String getRepresentationPropertyValueForThing(Thing thing) {
        ThingType thingType = thingTypeRegistry.getThingType(thing.getThingTypeUID());
        if (thingType != null) {
            String representationProperty = thingType.getRepresentationProperty();
            if (representationProperty == null) {
            Map<String, String> properties = thing.getProperties();
            if (properties.containsKey(representationProperty)) {
                return properties.get(representationProperty);
            Configuration configuration = thing.getConfiguration();
            if (configuration.containsKey(representationProperty)) {
                return String.valueOf(configuration.get(representationProperty));
    private void removeFromInbox(ThingTypeUID thingtypeUID, String representationValue) {
                .filter(forThingTypeUID(thingtypeUID)).filter(withFlag(DiscoveryResultFlag.IGNORED)).toList();
            logger.debug("Removing the ignored result from the inbox for the representation value '{}'.",
                    representationValue);
            inbox.remove(results.getFirst().getThingUID());
    private void autoApproveInboxEntries() {
        for (DiscoveryResult result : inbox.getAll()) {
            if (DiscoveryResultFlag.NEW.equals(result.getFlag())) {
    private boolean isToBeAutoApproved(DiscoveryResult result) {
        return inboxAutoApprovePredicates.stream().anyMatch(predicate -> predicate.test(result));
    protected void addInboxAutoApprovePredicate(InboxAutoApprovePredicate inboxAutoApprovePredicate) {
        inboxAutoApprovePredicates.add(inboxAutoApprovePredicate);
            if (DiscoveryResultFlag.NEW.equals(result.getFlag()) && inboxAutoApprovePredicate.test(result)) {
    protected void removeInboxAutoApprovePredicate(InboxAutoApprovePredicate inboxAutoApprovePredicate) {
        inboxAutoApprovePredicates.remove(inboxAutoApprovePredicate);
