package org.openhab.core.io.rest.core.internal.service;
import org.openhab.core.config.core.ConfigurableServiceUtil;
import org.openhab.core.io.rest.core.service.ConfigurableServiceDTO;
 * {@link ConfigurableServiceResource} provides access to configurable services.
 * It lists the available services and allows to get, update and delete the
 * configuration for a service ID. See also {@link ConfigurableService}.
@Component(service = { RESTResource.class, ConfigurableServiceResource.class })
@JaxrsName(ConfigurableServiceResource.PATH_SERVICES)
@Path(ConfigurableServiceResource.PATH_SERVICES)
@Tag(name = ConfigurableServiceResource.PATH_SERVICES)
public class ConfigurableServiceResource implements RESTResource {
    public static final String PATH_SERVICES = "services";
    private final Logger logger = LoggerFactory.getLogger(ConfigurableServiceResource.class);
    public ConfigurableServiceResource( //
            final BundleContext bundleContext, //
            final @Reference ConfigurationService configurationService, //
            final @Reference ConfigDescriptionRegistry configDescRegistry, //
            final @Reference TranslationProvider translationProvider, //
    @Operation(operationId = "getServices", summary = "Get all configurable services.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ConfigurableServiceDTO.class)))) })
    public List<ConfigurableServiceDTO> getAll(
        return getConfigurableServices(locale);
    @Path("/{serviceId}")
    @Operation(operationId = "getServicesById", summary = "Get configurable service for given service ID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ConfigurableServiceDTO.class))),
            @PathParam("serviceId") @Parameter(description = "service ID") String serviceId) {
        ConfigurableServiceDTO configurableService = getServiceById(serviceId, locale);
        if (configurableService != null) {
            return Response.ok(configurableService).build();
    private @Nullable ConfigurableServiceDTO getServiceById(String serviceId, Locale locale) {
        ConfigurableServiceDTO multiService = getMultiConfigServiceById(serviceId, locale);
        if (multiService != null) {
            return multiService;
        List<ConfigurableServiceDTO> configurableServices = getConfigurableServices(locale);
        for (ConfigurableServiceDTO configurableService : configurableServices) {
            if (configurableService.id.equals(serviceId)) {
                return configurableService;
    private @Nullable ConfigurableServiceDTO getMultiConfigServiceById(String serviceId, Locale locale) {
        String filter = "(&(" + Constants.SERVICE_PID + "=" + serviceId + ")(" + ConfigurationAdmin.SERVICE_FACTORYPID
                + "=*))";
        List<ConfigurableServiceDTO> services = getServicesByFilter(filter, locale);
        if (services.size() == 1) {
            return services.getFirst();
    @Path("/{serviceId}/contexts")
    @Operation(operationId = "getServiceContext", summary = "Get existing multiple context service configurations for the given factory PID.", responses = {
    public List<ConfigurableServiceDTO> getMultiConfigServicesByFactoryPid(
        return collectServicesById(serviceId, locale);
    private List<ConfigurableServiceDTO> collectServicesById(String serviceId, Locale locale) {
        String filter = "(" + ConfigurationAdmin.SERVICE_FACTORYPID + "=" + serviceId + ")";
        return getServicesByFilter(filter, locale);
    @Path("/{serviceId}/config")
    @Operation(operationId = "getServiceConfig", summary = "Get service configuration for given service ID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(type = "object"))),
    public Response getConfiguration(@PathParam("serviceId") @Parameter(description = "service ID") String serviceId) {
            Configuration configuration = configurationService.get(serviceId);
            logger.error("Cannot get configuration for service {}: ", serviceId, ex);
    @Operation(operationId = "updateServiceConfig", summary = "Updates a service configuration for given service ID and returns the old configuration.", requestBody = @RequestBody(required = false, content = @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(type = "object"))), responses = {
    public Response updateConfiguration(
            @PathParam("serviceId") @Parameter(description = "service ID") String serviceId,
            Configuration oldConfiguration = configurationService.get(serviceId);
            configurationService.update(serviceId,
                    new Configuration(normalizeConfiguration(configuration, serviceId, locale)));
            logger.error("Cannot update configuration for service {}: {}", serviceId, ex.getMessage(), ex);
            @Nullable Map<String, @Nullable Object> properties, String serviceId, Locale locale) {
        ConfigurableServiceDTO service = getServiceById(serviceId, locale);
            uri = new URI(service.configDescriptionURI);
            logger.warn("Not a valid URI: {}", service.configDescriptionURI);
        ConfigDescription configDesc = configDescRegistry.getConfigDescription(uri);
        if (configDesc == null) {
    @Operation(operationId = "deleteServiceConfig", summary = "Deletes a service configuration for given service ID and returns the old configuration.", responses = {
            @ApiResponse(responseCode = "500", description = "Configuration can not be deleted due to internal error") })
    public Response deleteConfiguration(
            Configuration oldConfiguration = configurationService.delete(serviceId);
            return oldConfiguration != null ? Response.ok(oldConfiguration).build() : Response.noContent().build();
            logger.error("Cannot delete configuration for service {}: {}", serviceId, ex.getMessage(), ex);
    private List<ConfigurableServiceDTO> getServicesByFilter(String filter, Locale locale) {
        List<ConfigurableServiceDTO> services = new ArrayList<>();
        ServiceReference<?>[] serviceReferences = null;
            serviceReferences = bundleContext.getServiceReferences((String) null, filter);
        } catch (InvalidSyntaxException ex) {
            logger.error("Cannot get service references because the syntax of the filter '{}' is invalid.", filter);
        if (serviceReferences != null) {
            for (ServiceReference<?> serviceReference : serviceReferences) {
                String id = getServiceId(serviceReference);
                ConfigurableService configurableService = ConfigurableServiceUtil
                        .asConfigurableService(serviceReference::getProperty);
                String defaultLabel = configurableService.label();
                if (defaultLabel.isEmpty()) { // for multi context services the label can be changed and must be read
                                              // from config admin.
                    defaultLabel = configurationService.getProperty(id, OpenHAB.SERVICE_CONTEXT);
                String key = I18nUtil.stripConstantOr(defaultLabel,
                        () -> inferKey(configurableService.description_uri(), "label"));
                // i18n file containing label for system:addons service is exceptionally located in bundle
                // org.openhab.core (and not in bundle org.openhab.core.addon.eclipse)
                String label = i18nProvider.getText("system:addons".equals(configurableService.description_uri())
                        ? FrameworkUtil.getBundle(OpenHAB.class)
                        : serviceReference.getBundle(), key, defaultLabel, locale);
                String category = configurableService.category();
                String configDescriptionURI = configurableService.description_uri();
                if (configDescriptionURI.isEmpty()) {
                    String factoryPid = (String) serviceReference.getProperty(ConfigurationAdmin.SERVICE_FACTORYPID);
                    configDescriptionURI = getConfigDescriptionByFactoryPid(factoryPid);
                boolean multiple = configurableService.factory();
                services.add(new ConfigurableServiceDTO(id, label == null ? defaultLabel : label, category,
                        configDescriptionURI, multiple));
    private @Nullable String getConfigDescriptionByFactoryPid(String factoryPid) {
        String configDescriptionURI = null;
        String filter = "(" + Constants.SERVICE_PID + "=" + factoryPid + ")";
            ServiceReference<?>[] refs = bundleContext.getServiceReferences((String) null, filter);
            if (refs != null && refs.length > 0) {
                        .asConfigurableService(key -> refs[0].getProperty(key));
                configDescriptionURI = configurableService.description_uri();
    private List<ConfigurableServiceDTO> getConfigurableServices(Locale locale) {
        services.addAll(getServicesByFilter(ConfigurableServiceUtil.CONFIGURABLE_SERVICE_FILTER, locale));
        services.addAll(getServicesByFilter(ConfigurableServiceUtil.CONFIGURABLE_MULTI_CONFIG_SERVICE_FILTER, locale));
    private String getServiceId(ServiceReference<?> serviceReference) {
        final String cn = (String) serviceReference.getProperty(ComponentConstants.COMPONENT_NAME);
        Object pid = serviceReference.getProperty(Constants.SERVICE_PID);
        if (pid == null) {
            return cn;
        final String serviceId;
        if (pid instanceof String string) {
            serviceId = string;
        } else if (pid instanceof String[] pids) {
            serviceId = getServicePID(cn, Arrays.asList(pids));
        } else if (pid instanceof Collection<?> pids) {
            serviceId = getServicePID(cn, pids.stream().map(Object::toString).toList());
            logger.warn("The component \"{}\" is using an unhandled service PID type ({}). Use component name.", cn,
                    pid.getClass());
            serviceId = cn;
        if (serviceId.isEmpty()) {
            logger.debug("Missing service PID for component \"{}\", use component name.", cn);
    private String getServicePID(final String cn, final List<String> pids) {
        switch (pids.size()) {
                return pids.getFirst();
            default: // multiple entries
                final String first = pids.getFirst();
                boolean differences = false;
                for (int i = 1; i < pids.size(); ++i) {
                    if (!first.equals(pids.get(i))) {
                        differences = true;
                if (differences) {
                            "The component \"{}\" is using different service PIDs ({}). Different service PIDs are not supported, the first one ({}) is used.",
                            cn, pids, first);
    private String inferKey(String uri, String lastSegment) {
        return "service." + uri.replace(":", ".") + "." + lastSegment;
