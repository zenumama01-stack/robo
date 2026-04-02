 * Delegate SI units to {@link Units} to hide this dependency from the rest of openHAB.
 * See members of {@link Units} for a detailed description.
public final class SIUnits extends CustomUnits {
    public static final String MEASUREMENT_SYSTEM_NAME = "SI";
    private static final SIUnits INSTANCE = new SIUnits();
    public static final Unit<Temperature> CELSIUS = addUnit(Units.CELSIUS);
    public static final Unit<Speed> KILOMETRE_PER_HOUR = addUnit(Units.KILOMETRE_PER_HOUR);
    public static final Unit<Length> METRE = addUnit(Units.METRE);
    public static final Unit<Mass> KILOGRAM = addUnit(Units.KILOGRAM);
    public static final Unit<Mass> GRAM = addUnit(Units.GRAM);
    public static final Unit<Area> SQUARE_METRE = addUnit(Units.SQUARE_METRE);
    public static final Unit<Volume> CUBIC_METRE = addUnit(Units.CUBIC_METRE);
    public static final Unit<Pressure> PASCAL = addUnit(Units.PASCAL);
        // Override the default unit symbol ℃ to better support TTS and UIs:
        SimpleUnitFormat.getInstance().label(CELSIUS, "°C");
    private SIUnits() {
