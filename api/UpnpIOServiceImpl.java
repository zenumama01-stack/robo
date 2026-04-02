package org.openhab.core.io.transport.upnp.internal;
import org.jupnp.controlpoint.ActionCallback;
import org.jupnp.controlpoint.ControlPoint;
import org.jupnp.controlpoint.SubscriptionCallback;
import org.jupnp.model.action.ActionArgumentValue;
import org.jupnp.model.action.ActionException;
import org.jupnp.model.action.ActionInvocation;
import org.jupnp.model.gena.CancelReason;
import org.jupnp.model.gena.GENASubscription;
import org.jupnp.model.message.UpnpResponse;
import org.jupnp.model.meta.Action;
import org.jupnp.model.meta.Device;
import org.jupnp.model.meta.DeviceIdentity;
import org.jupnp.model.meta.Service;
import org.jupnp.model.state.StateVariableValue;
import org.jupnp.model.types.ServiceId;
import org.jupnp.model.types.UDAServiceId;
import org.openhab.core.io.transport.upnp.UpnpIOParticipant;
import org.openhab.core.io.transport.upnp.UpnpIOService;
 * The {@link UpnpIOServiceImpl} is the implementation of the UpnpIOService
 * interface
 * @author Karel Goderis - Initial contribution; added simple polling mechanism
 * @author Markus Rathgeb - added NP checks in subscription ended callback
 * @author Andre Fuechsel - added methods to remove subscriptions
 * @author Ivan Iliev - made sure resubscribe is only done when subscription ended CancelReason was EXPIRED or
 *         RENEW_FAILED
public class UpnpIOServiceImpl implements UpnpIOService, RegistryListener {
    private final Logger logger = LoggerFactory.getLogger(UpnpIOServiceImpl.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool(POOL_NAME);
    private static final int DEFAULT_POLLING_INTERVAL = 60;
    private static final String POOL_NAME = "upnp-io";
    final Set<UpnpIOParticipant> participants = new CopyOnWriteArraySet<>();
    final Map<UpnpIOParticipant, ScheduledFuture> pollingJobs = new ConcurrentHashMap<>();
    final Map<UpnpIOParticipant, Boolean> currentStates = new ConcurrentHashMap<>();
    final Map<Service, UpnpSubscriptionCallback> subscriptionCallbacks = new ConcurrentHashMap<>();
    public class UpnpSubscriptionCallback extends SubscriptionCallback {
        public UpnpSubscriptionCallback(Service service) {
            super(service);
        public UpnpSubscriptionCallback(Service service, int requestedDurationSeconds) {
            super(service, requestedDurationSeconds);
        protected void ended(GENASubscription subscription, CancelReason reason, UpnpResponse response) {
            final Service service = subscription.getService();
                final ServiceId serviceId = service.getServiceId();
                final Device device = service.getDevice();
                if (device != null) {
                    final Device deviceRoot = device.getRoot();
                    if (deviceRoot != null) {
                        final DeviceIdentity deviceRootIdentity = deviceRoot.getIdentity();
                        if (deviceRootIdentity != null) {
                            final UDN deviceRootUdn = deviceRootIdentity.getUdn();
                            logger.debug("A GENA subscription '{}' for device '{}' was ended", serviceId.getId(),
                                    deviceRootUdn);
                if ((CancelReason.EXPIRED.equals(reason) || CancelReason.RENEWAL_FAILED.equals(reason))
                        && upnpService != null) {
                    final ControlPoint cp = upnpService.getControlPoint();
                    if (cp != null) {
                        final UpnpSubscriptionCallback callback = new UpnpSubscriptionCallback(service,
                                subscription.getActualDurationSeconds());
                        cp.execute(callback);
        protected void established(GENASubscription subscription) {
            Device deviceRoot = subscription.getService().getDevice().getRoot();
            String serviceId = subscription.getService().getServiceId().getId();
            logger.trace("A GENA subscription '{}' for device '{}' is established", serviceId,
                    deviceRoot.getIdentity().getUdn());
            for (UpnpIOParticipant participant : participants) {
                if (Objects.equals(getDevice(participant), deviceRoot)) {
                        participant.onServiceSubscribed(serviceId, true);
                        logger.error("Participant threw an exception onServiceSubscribed", e);
        protected void eventReceived(GENASubscription sub) {
            Map<String, StateVariableValue> values = sub.getCurrentValues();
            Device device = sub.getService().getDevice();
            String serviceId = sub.getService().getServiceId().getId();
            logger.trace("Receiving a GENA subscription '{}' response for device '{}'", serviceId,
                    device.getIdentity().getUdn());
                Device participantDevice = getDevice(participant);
                if (Objects.equals(participantDevice, device) || Objects.equals(participantDevice, device.getRoot())) {
                    for (Entry<String, StateVariableValue> entry : values.entrySet()) {
                        Object value = entry.getValue().getValue();
                                participant.onValueReceived(entry.getKey(), value.toString(), serviceId);
                                logger.error("Participant threw an exception onValueReceived", e);
        protected void eventsMissed(GENASubscription subscription, int numberOfMissedEvents) {
            logger.debug("A GENA subscription '{}' for device '{}' missed events",
                    subscription.getService().getServiceId(),
                    subscription.getService().getDevice().getRoot().getIdentity().getUdn());
        protected void failed(GENASubscription subscription, UpnpResponse response, Exception e, String defaultMsg) {
            logger.debug("A GENA subscription '{}' for device '{}' failed", serviceId,
                        participant.onServiceSubscribed(serviceId, false);
                    } catch (Exception e2) {
                        logger.error("Participant threw an exception onServiceSubscribed", e2);
    public UpnpIOServiceImpl(final @Reference UpnpService upnpService) {
        logger.debug("Starting UPnP IO service...");
        upnpService.getRegistry().getRemoteDevices().forEach(device -> informParticipants(device, true));
        logger.debug("Stopping UPnP IO service...");
    private Device getDevice(UpnpIOParticipant participant) {
        return upnpService.getRegistry().getDevice(new UDN(participant.getUDN()), false);
    public void addSubscription(UpnpIOParticipant participant, String serviceID, int duration) {
        if (participant != null && serviceID != null) {
            registerParticipant(participant);
            Device device = getDevice(participant);
                Service subService = searchSubService(serviceID, device);
                if (subService != null) {
                    logger.trace("Setting up an UPNP service subscription '{}' for particpant '{}'", serviceID,
                            participant.getUDN());
                    UpnpSubscriptionCallback callback = new UpnpSubscriptionCallback(subService, duration);
                    subscriptionCallbacks.put(subService, callback);
                    upnpService.getControlPoint().execute(callback);
                    logger.trace("Could not find service '{}' for device '{}'", serviceID,
                logger.trace("Could not find an upnp device for participant '{}'", participant.getUDN());
    private Service searchSubService(String serviceID, Device device) {
        Service subService = findService(device, null, serviceID);
        if (subService == null) {
            // service not on the root device, we search the embedded devices as well
            Device[] embedded = device.getEmbeddedDevices();
            if (embedded != null) {
                for (Device aDevice : embedded) {
                    subService = findService(aDevice, null, serviceID);
        return subService;
    public void removeSubscription(UpnpIOParticipant participant, String serviceID) {
                    logger.trace("Removing an UPNP service subscription '{}' for particpant '{}'", serviceID,
                    UpnpSubscriptionCallback callback = subscriptionCallbacks.remove(subService);
                        callback.end();
    public Map<String, String> invokeAction(UpnpIOParticipant participant, String serviceID, String actionID,
            Map<String, String> inputs) {
        return invokeAction(participant, null, serviceID, actionID, inputs);
    public Map<String, String> invokeAction(UpnpIOParticipant participant, @Nullable String namespace, String serviceID,
            String actionID, Map<String, String> inputs) {
        Map<String, String> resultMap = new HashMap<>();
        if (serviceID != null && actionID != null && participant != null) {
                Service service = findService(device, namespace, serviceID);
                    Action action = service.getAction(actionID);
                    if (action != null) {
                        ActionInvocation invocation = new ActionInvocation(action);
                            for (Entry<String, String> entry : inputs.entrySet()) {
                                invocation.setInput(entry.getKey(), entry.getValue());
                        logger.trace("Invoking Action '{}' of service '{}' for participant '{}'", actionID, serviceID,
                        new ActionCallback.Default(invocation, upnpService.getControlPoint()).run();
                        ActionException anException = invocation.getFailure();
                        if (anException != null && anException.getMessage() != null) {
                            logger.debug("{}", anException.getMessage());
                        Map<String, ActionArgumentValue> result = invocation.getOutputMap();
                            for (Entry<String, ActionArgumentValue> entry : result.entrySet()) {
                                String variable = entry.getKey();
                                final ActionArgumentValue newArgument;
                                    newArgument = entry.getValue();
                                    logger.debug("An exception '{}' occurred, cannot get argument for variable '{}'",
                                            ex.getMessage(), variable);
                                    if (newArgument.getValue() != null) {
                                        resultMap.put(variable, newArgument.getValue().toString());
                                            "An exception '{}' occurred processing ActionArgumentValue '{}' with value '{}'",
                                            ex.getMessage(), newArgument.getArgument().getName(),
                                            newArgument.getValue());
                        logger.debug("Could not find action '{}' for participant '{}'", actionID, participant.getUDN());
                    logger.debug("Could not find service '{}' for participant '{}'", serviceID, participant.getUDN());
                logger.debug("Could not find an upnp device for participant '{}'", participant.getUDN());
    public boolean isRegistered(UpnpIOParticipant participant) {
        return upnpService.getRegistry().getDevice(new UDN(participant.getUDN()), false) != null;
    public void registerParticipant(UpnpIOParticipant participant) {
        if (participant != null) {
    public void unregisterParticipant(UpnpIOParticipant participant) {
            stopPollingForParticipant(participant);
            pollingJobs.remove(participant);
            currentStates.remove(participant);
    public URL getDescriptorURL(UpnpIOParticipant participant) {
        RemoteDevice device = upnpService.getRegistry().getRemoteDevice(new UDN(participant.getUDN()), true);
            return device.getIdentity().getDescriptorURL();
    private Service findService(Device device, @Nullable String namespace, String serviceID) {
        Service service;
            namespace = device.getType().getNamespace();
        if (UDAServiceId.DEFAULT_NAMESPACE.equals(namespace)
                || UDAServiceId.BROKEN_DEFAULT_NAMESPACE.equals(namespace)) {
            service = device.findService(new UDAServiceId(serviceID));
            service = device.findService(new ServiceId(namespace, serviceID));
     * Propagates a device status change to all participants
     * @param device the device that has changed its status
     * @param status true, if device is reachable, false otherwise
    private void informParticipants(RemoteDevice device, boolean status) {
            if (participant.getUDN().equals(device.getIdentity().getUdn().getIdentifierString())) {
                setDeviceStatus(participant, status);
    private void setDeviceStatus(UpnpIOParticipant participant, boolean newStatus) {
        if (!Objects.equals(currentStates.get(participant), newStatus)) {
            currentStates.put(participant, newStatus);
            logger.debug("Device '{}' reachability status changed to '{}'", participant.getUDN(), newStatus);
            participant.onStatusChanged(newStatus);
    private class UPNPPollingRunnable implements Runnable {
        private final UpnpIOParticipant participant;
        private final String serviceID;
        private final String actionID;
        public UPNPPollingRunnable(UpnpIOParticipant participant, String serviceID, String actionID) {
            this.participant = participant;
            this.serviceID = serviceID;
            this.actionID = actionID;
            // It is assumed that during addStatusListener() a check is made whether the participant is correctly
                    Service service = findService(device, null, serviceID);
                            logger.debug("Polling participant '{}' through Action '{}' of Service '{}' ",
                                    participant.getUDN(), actionID, serviceID);
                            if (anException != null
                                    && anException.getMessage().contains("Connection error or no response received")) {
                                // The UDN is not reachable anymore
                                setDeviceStatus(participant, false);
                                // The UDN functions correctly
                                setDeviceStatus(participant, true);
                            logger.debug("Could not find action '{}' for participant '{}'", actionID,
                        logger.debug("Could not find service '{}' for participant '{}'", serviceID,
                logger.error("An exception occurred while polling an UPNP device: '{}'", e.getMessage(), e);
    public void addStatusListener(UpnpIOParticipant participant, String serviceID, String actionID, int interval) {
            int pollingInterval = interval == 0 ? DEFAULT_POLLING_INTERVAL : interval;
            // remove the previous polling job, if any
            currentStates.put(participant, true);
            Runnable pollingRunnable = new UPNPPollingRunnable(participant, serviceID, actionID);
            pollingJobs.put(participant,
                    scheduler.scheduleWithFixedDelay(pollingRunnable, 0, pollingInterval, TimeUnit.SECONDS));
    private void stopPollingForParticipant(UpnpIOParticipant participant) {
        if (pollingJobs.containsKey(participant)) {
            ScheduledFuture<?> pollingJob = pollingJobs.get(participant);
            if (pollingJob != null) {
                pollingJob.cancel(true);
    public void removeStatusListener(UpnpIOParticipant participant) {
            unregisterParticipant(participant);
        informParticipants(device, true);
            informParticipants(childDevice, true);
        informParticipants(device, false);
            informParticipants(childDevice, false);
