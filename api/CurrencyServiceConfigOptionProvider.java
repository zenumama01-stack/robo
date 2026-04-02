import org.openhab.core.library.unit.CurrencyProvider;
 * The {@link CurrencyServiceConfigOptionProvider} is an implementation of {@link ConfigOptionProvider} for the
 * available currency providers.
@Component(service = ConfigOptionProvider.class)
public class CurrencyServiceConfigOptionProvider implements ConfigOptionProvider {
    private final List<CurrencyProvider> currencyProviders = new CopyOnWriteArrayList<>();
    public void addCurrencyProvider(CurrencyProvider currencyProvider) {
        currencyProviders.add(currencyProvider);
    public void removeCurrencyProvider(CurrencyProvider currencyProvider) {
        currencyProviders.remove(currencyProvider);
        if ("system:units".equals(uri.toString()) && "currencyProvider".equals(param)) {
            return currencyProviders.stream().map(this::mapProvider).toList();
    private ParameterOption mapProvider(CurrencyProvider currencyProvider) {
        String providerName = currencyProvider.getName();
        int lastDot = providerName.lastIndexOf(".");
        String providerDescription = lastDot > -1 ? providerName.substring(lastDot + 1) : providerName;
        return new ParameterOption(providerName, providerDescription);
