 * The {@link CurrencyProvider} can be implemented by services that supply currencies and their exchange rates
public interface CurrencyProvider {
     * Get the name of this {@link CurrencyProvider}
     * @return the name, defaults to the class name
    default String getName() {
        return getClass().getName();
     * Get the base currency from this provider
     * This currency is used as base for calculating exchange rates.
     * @return the base currency of this provider
    Unit<Currency> getBaseCurrency();
     * Get all additional currency that are supported by this provider
     * The collection does NOT include the base currency.
     * @return a {@link Collection} of {@link Unit<Currency>}s
    Collection<Unit<Currency>> getAdditionalCurrencies();
     * Get a {@link Function} that supplies exchanges rates for currencies supported by this provider
     * This needs to be dynamic because in most cases exchange rates are not constant over time.
     * @return the function
    Function<Unit<Currency>, @Nullable BigDecimal> getExchangeRateFunction();
