@Component(configurationPid = "org.openhab.magic", service = ConfigOptionProvider.class, immediate = true, //
        property = Constants.SERVICE_PID + "=org.openhab.magic")
@ConfigurableService(category = "test", label = "Magic", description_uri = "test:magic")
public class MagicServiceImpl implements MagicService {
    private final Logger logger = LoggerFactory.getLogger(MagicServiceImpl.class);
    static final String PARAMETER_BACKEND_DECIMAL = "select_decimal_limit";
        if (!CONFIG_URI.equals(uri)) {
        if (PARAMETER_BACKEND_DECIMAL.equals(param)) {
            return List.of(new ParameterOption(BigDecimal.ONE.toPlainString(), "1"),
                    new ParameterOption(BigDecimal.TEN.toPlainString(), "10"),
                    new ParameterOption(BigDecimal.valueOf(21d).toPlainString(), "21"));
    public void activate(Map<String, Object> properties) {
    public void modified(Map<String, Object> properties) {
        MagicServiceConfig config = new Configuration(properties).as(MagicServiceConfig.class);
        logger.debug("Magic Service has been modified: {}", config);
