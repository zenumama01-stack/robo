import static org.openhab.core.library.unit.CurrencyUnits.BASE_CURRENCY;
import org.openhab.core.library.unit.CurrencyUnit;
import tech.units.indriya.format.SimpleUnitFormat;
 * The {@link CurrencyService} allows to register and switch {@link CurrencyProvider}s and provides exchange rates
 * for currencies
@Component(service = CurrencyService.class, immediate = true, configurationPid = CurrencyService.CONFIGURATION_PID, property = {
        Constants.SERVICE_PID + "=org.openhab.units", //
        "service.config.label=Unit Settings", //
        "service.config.description.uri=system:units" })
public class CurrencyService {
    public static final String CONFIGURATION_PID = "org.openhab.units";
    public static final String CONFIG_OPTION_CURRENCY_PROVIDER = "currencyProvider";
    private final Logger logger = LoggerFactory.getLogger(CurrencyService.class);
    public static Function<Unit<Currency>, @Nullable BigDecimal> FACTOR_FCN = unit -> null;
    private final Map<String, CurrencyProvider> currencyProviders = new ConcurrentHashMap<>();
    private CurrencyProvider enabledCurrencyProvider = DefaultCurrencyProvider.getInstance();
    private String configuredCurrencyProvider = DefaultCurrencyProvider.getInstance().getName();
    public CurrencyService(Map<String, Object> config) {
    public void modified(Map<String, Object> config) {
        String configOption = (String) config.get(CONFIG_OPTION_CURRENCY_PROVIDER);
        configuredCurrencyProvider = Objects.requireNonNullElse(configOption,
                DefaultCurrencyProvider.getInstance().getName());
        CurrencyProvider currencyProvider = currencyProviders.getOrDefault(configuredCurrencyProvider,
                DefaultCurrencyProvider.getInstance());
        enableProvider(currencyProvider);
        currencyProviders.put(currencyProvider.getName(), currencyProvider);
        if (configuredCurrencyProvider.equals(currencyProvider.getName())) {
        if (currencyProvider.equals(enabledCurrencyProvider)) {
            logger.warn("The currently activated currency provider is being removed. Enabling default.");
            enableProvider(DefaultCurrencyProvider.getInstance());
        currencyProviders.remove(currencyProvider.getName());
    private synchronized void enableProvider(CurrencyProvider currencyProvider) {
        SimpleUnitFormat unitFormatter = SimpleUnitFormat.getInstance();
        // remove units from old provider
        enabledCurrencyProvider.getAdditionalCurrencies().forEach(CurrencyUnits::removeUnit);
        unitFormatter.removeLabel(enabledCurrencyProvider.getBaseCurrency());
        // add new units
        FACTOR_FCN = currencyProvider.getExchangeRateFunction();
        Unit<Currency> baseCurrency = currencyProvider.getBaseCurrency();
        ((CurrencyUnit) BASE_CURRENCY).setSymbol(baseCurrency.getSymbol());
        ((CurrencyUnit) BASE_CURRENCY).setName(baseCurrency.getName());
        unitFormatter.label(BASE_CURRENCY, baseCurrency.getName());
        if (baseCurrency.getSymbol() != null) {
            unitFormatter.alias(BASE_CURRENCY, baseCurrency.getSymbol());
        currencyProvider.getAdditionalCurrencies().forEach(CurrencyUnits::addUnit);
        this.enabledCurrencyProvider = currencyProvider;
    private static class DefaultCurrencyProvider implements CurrencyProvider {
        private static final CurrencyProvider INSTANCE = new DefaultCurrencyProvider();
        public Unit<Currency> getBaseCurrency() {
            return new CurrencyUnit("DEF", null);
        public Collection<Unit<Currency>> getAdditionalCurrencies() {
        public Function<Unit<Currency>, @Nullable BigDecimal> getExchangeRateFunction() {
            return unit -> null;
        public static CurrencyProvider getInstance() {
            return INSTANCE;
