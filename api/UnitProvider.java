 * Provides {@link Unit}s and the current {@link SystemOfUnits}.
public interface UnitProvider {
     * Retrieves the default {@link Unit} for the given {@link Quantity} according to the current
     * {@link SystemOfUnits}.
     * @param dimension The {@link Quantity}, called dimension here, defines the base unit for the retrieved unit. E.g.
     *            call {@code getUnit(javax.measure.quantity.Temperature.class)} to retrieve the temperature unit
     *            according to the current {@link SystemOfUnits}.
     * @return The {@link Unit} matching the given {@link Quantity}
     * @throws IllegalArgumentException when the dimension is unknown
    <T extends Quantity<T>> Unit<T> getUnit(Class<T> dimension) throws IllegalArgumentException;
     * Returns the {@link SystemOfUnits} which is currently set, must not be null.
     * @return the {@link SystemOfUnits} which is currently set, must not be null.
    SystemOfUnits getMeasurementSystem();
    Collection<Class<? extends Quantity<?>>> getAllDimensions();
