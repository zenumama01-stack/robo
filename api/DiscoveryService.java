 * The {@link DiscoveryService} is a service interface which each binding can
 * implement to provide an auto discovery process for one or more {@code Thing} s.
 * As an example, a typical discovery mechanism could scan the network for <i>UPnP</i> devices, if requested.
 * A {@link DiscoveryService} must be able to finish its discovery process without any user interaction.
 * <b>There are two different kind of executions:</b>
 * <li><b>Background discovery:</b> If this mode is enabled, the discovery process should run in the background as long
 * as this mode is not disabled again. Background discovery can be enabled and disabled and is configured through the
 * configuration admin. The implementation class that registers an OSGi service must define a PID and has to react on
 * configuration changes for it. See also {@link DiscoveryService#CONFIG_PROPERTY_BACKGROUND_DISCOVERY}.</li>
 * <li><b>Active scan:</b> If an active scan is triggered, the the service should try to actively query for new devices
 * and should report new results within the defined scan timeout. An active scan can be aborted.</li>
 * @see DiscoveryServiceRegistry
public interface DiscoveryService {
     * Configuration property for enabling the auto discovery feature of a
     * DiscoveryService.
    String CONFIG_PROPERTY_BACKGROUND_DISCOVERY = "background";
    Collection<ThingTypeUID> getSupportedThingTypes();
     * Returns {@code true} if the discovery supports an optional input parameter to run, otherwise {@code false}.
     * @return true if the discovery supports an optional input parameter to run, otherwise false
    boolean isScanInputSupported();
     * Returns the label of the supported input parameter to start the discovery.
     * @return the label of the supported input parameter to start the discovery or null if input parameter not
     *         supported
    String getScanInputLabel();
     * Returns the description of the supported input parameter to start the discovery.
     * @return the description of the supported input parameter to start the discovery or null if input parameter not
    String getScanInputDescription();
     * Returns the amount of time in seconds after which an active scan ends.
     * @return the scan timeout in seconds (>= 0).
    int getScanTimeout();
     * Returns {@code true} if the background discovery mode is enabled, otherwise {@code false}.
     * @return true if the background discovery mode is enabled, otherwise false
    boolean isBackgroundDiscoveryEnabled();
     * Triggers this service to start an active scan for new devices.<br>
     * This method must not block any calls such as {@link #abortScan()} and
     * must return fast.
     * If started, any registered {@link DiscoveryListener} must be notified about {@link DiscoveryResult}s.
     * If there is already a scan running, it is aborted and a new scan is triggered.
     * @param listener a listener that is notified about errors or termination of the scan
    void startScan(@Nullable ScanListener listener);
     * Triggers this service to start an active scan for new devices using an input parameter for that.<br>
     * @param input an input parameter to be used during discovery scan
    void startScan(String input, @Nullable ScanListener listener);
     * Stops an active scan for devices.<br>
     * This method must not block any calls such as {@link #startScan} and must
     * return fast.
     * After this method returns, no further notifications about {@link DiscoveryResult}s are allowed to be sent to any
     * registered listener, exceptional the background discovery mode is active.
     * This method returns silently, if the scan has not been started before.
    void abortScan();
     * Adds a {@link DiscoveryListener} to the listeners' registry.
     * Directly after registering the listener, it will receive
     * {@link DiscoveryListener#thingDiscovered(DiscoveryService, DiscoveryResult)} notifications about all devices that
     * have been previously discovered by the service already (tracker behaviour). This is also done, if the listener
     * has already been registered previously.
     * When a {@link DiscoveryResult} is created while the discovery process is active (e.g. by starting a scan or
     * through the enabled background discovery mode), the specified listener is notified.
     * This method returns silently if the specified listener is {@code null}.
     * @param listener the listener to be added (could be null)
    void addDiscoveryListener(@Nullable DiscoveryListener listener);
     * Removes a {@link DiscoveryListener} from the listeners' registry.
     * When this method returns, the specified listener is no longer notified about a created {@link DiscoveryResult}
     * while the discovery process is active (e.g. by forcing the startup of the discovery process or while enabling the
     * auto discovery mode)
     * This method returns silently if the specified listener is {@code null} or has not been registered before.
     * @param listener the listener to be removed (could be null)
    void removeDiscoveryListener(@Nullable DiscoveryListener listener);
