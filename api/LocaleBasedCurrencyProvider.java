 * The {@link LocaleBasedCurrencyProvider} is an implementation of {@link CurrencyProvider} that provides the base
 * currency based on the configured locale
@Component(service = CurrencyProvider.class, property = { Constants.SERVICE_PID + "=org.openhab.localebasedcurrency" })
public class LocaleBasedCurrencyProvider implements CurrencyProvider {
    public LocaleBasedCurrencyProvider(@Reference LocaleProvider localeProvider) {
        String currencyCode = java.util.Currency.getInstance(localeProvider.getLocale()).getCurrencyCode();
        if (currencyCode != null) {
            // either the currency was set or determined from the locale
            String symbol = java.util.Currency.getInstance(currencyCode).getSymbol();
