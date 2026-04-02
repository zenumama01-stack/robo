package org.openhab.core.io.rest.core.config;
 * {@link ConfigurationService} manages configurations in the {@link ConfigurationAdmin}. The config id is the
 * equivalent to the {@link Constants#SERVICE_PID}.
@Component(service = ConfigurationService.class)
public class ConfigurationService {
    private ConfigurationAdmin configurationAdmin;
    private final Logger logger = LoggerFactory.getLogger(ConfigurationService.class);
     * Returns a configuration for a config id.
     * @param configId config id
     * @return config or null if no config with the given config id exists
     * @throws IOException if configuration can not be read
    public @Nullable Configuration get(String configId) throws IOException {
        org.osgi.service.cm.Configuration configuration = configurationAdmin.getConfiguration(configId, null);
        return toConfiguration(properties);
     * Creates or updates a configuration for a config id.
     * @param newConfiguration the configuration
     * @return old config or null if no old config existed
     * @throws IOException if configuration can not be stored
    public Configuration update(String configId, Configuration newConfiguration) throws IOException {
        return update(configId, newConfiguration, false);
    public String getProperty(String servicePID, String key) {
            org.osgi.service.cm.Configuration configuration = configurationAdmin.getConfiguration(servicePID, null);
            if (configuration != null && configuration.getProperties() != null) {
                return (String) configuration.getProperties().get(key);
            logger.debug("Error while retrieving property {} for PID {}.", key, servicePID);
     * @param override if true, it overrides the old config completely. means it deletes all parameters even if they are
     *            not defined in the given configuration.
    public Configuration update(String configId, Configuration newConfiguration, boolean override) throws IOException {
        org.osgi.service.cm.Configuration configuration = null;
        if (newConfiguration.containsKey(OpenHAB.SERVICE_CONTEXT)) {
                configuration = getConfigurationWithContext(configId);
                logger.error("Failed to lookup config for PID '{}'", configId);
                configuration = configurationAdmin.createFactoryConfiguration(configId, null);
            configuration = configurationAdmin.getConfiguration(configId, null);
        Configuration oldConfiguration = toConfiguration(configuration.getProperties());
        Dictionary<String, Object> properties = getProperties(configuration);
        Set<Entry<String, Object>> configurationParameters = newConfiguration.getProperties().entrySet();
            Set<String> keySet = oldConfiguration.keySet();
            for (String key : keySet) {
                properties.remove(key);
        for (Entry<String, Object> configurationParameter : configurationParameters) {
            Object value = configurationParameter.getValue();
                properties.remove(configurationParameter.getKey());
            } else if (value instanceof String || value instanceof Integer || value instanceof Boolean
                    || value instanceof Object[] || value instanceof Collection) {
                properties.put(configurationParameter.getKey(), value);
                // the config admin does not support complex object types, so let's store the string representation
                properties.put(configurationParameter.getKey(), value.toString());
        return oldConfiguration;
    private org.osgi.service.cm.Configuration getConfigurationWithContext(String serviceId)
        org.osgi.service.cm.Configuration[] configs = configurationAdmin
                .listConfigurations("(&(" + Constants.SERVICE_PID + "=" + serviceId + "))");
        if (configs == null) {
            throw new IllegalStateException("More than one configuration with PID " + serviceId + " exists");
        return configs[0];
     * Deletes a configuration for a config id.
     * @throws IOException if configuration can not be removed
    public @Nullable Configuration delete(String configId) throws IOException {
        org.osgi.service.cm.Configuration serviceConfiguration = configurationAdmin.getConfiguration(configId, null);
        if (serviceConfiguration == null) {
        Configuration oldConfiguration = toConfiguration(serviceConfiguration.getProperties());
        serviceConfiguration.delete();
    private @Nullable Configuration toConfiguration(Dictionary<String, Object> dictionary) {
        if (dictionary == null) {
        Map<String, Object> properties = new HashMap<>(dictionary.size());
        Enumeration<String> keys = dictionary.keys();
        while (keys.hasMoreElements()) {
            String key = keys.nextElement();
            if (!Constants.SERVICE_PID.equals(key)) {
                properties.put(key, dictionary.get(key));
        return new Configuration(properties);
    private Dictionary<String, Object> getProperties(org.osgi.service.cm.Configuration configuration) {
        return properties != null ? properties : new Hashtable<>();
    protected void setConfigurationAdmin(ConfigurationAdmin configurationAdmin) {
    protected void unsetConfigurationAdmin(ConfigurationAdmin configurationAdmin) {
