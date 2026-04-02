 * The {@link FixedCurrencyProvider} is an implementation of {@link CurrencyProvider} that provides only a single
 * (configurable) currency.
@Component(service = CurrencyProvider.class, configurationPid = CurrencyService.CONFIGURATION_PID)
public class FixedCurrencyProvider implements CurrencyProvider {
    public static final String CONFIG_OPTION_BASE_CURRENCY = "fixedBaseCurrency";
    private String currencyCode = "DEF";
    public FixedCurrencyProvider(Map<String, Object> config) {
        String code = (String) config.get(CONFIG_OPTION_BASE_CURRENCY);
        currencyCode = Objects.requireNonNullElse(code, "DEF");
        String symbol = null;
            symbol = java.util.Currency.getInstance(currencyCode).getSymbol();
        } catch (IllegalArgumentException ignored) {
        return new CurrencyUnit(currencyCode, symbol);
